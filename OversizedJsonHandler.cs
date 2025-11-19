using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;

namespace Contoso.AIFoundry.JsonProcessing
{
    /// <summary>
    /// Handles large JSON responses that exceed LLM token limits.
    /// Implements the 5-step approach:
    /// 1. Preprocessing - Filter to relevant fields
    /// 2. Semantic Chunking - Group data intelligently
    /// 3. Token Budget Management - Validate before sending
    /// 4. Structured Output Processing - Use JSON schema
    /// 5. Aggregation - Combine results
    /// </summary>

    // ============================================================================
    // STEP 1: PREPROCESSING LAYER - Filter JSON to Relevant Fields
    // ============================================================================

    /// <summary>
    /// Filters large JSON responses to extract only fields relevant to analysis.
    /// This dramatically reduces token count before sending to the LLM.
    /// </summary>
    public class JsonPreprocessor<T>
    {
        private readonly HashSet<string> _relevantFields;

        public JsonPreprocessor(params string[] relevantFields)
        {
            _relevantFields = new HashSet<string>(relevantFields);
        }

        /// <summary>
        /// Extract only relevant fields from each record.
        /// </summary>
        public List<Dictionary<string, object>> FilterRecords(List<T> rawRecords)
        {
            var filtered = new List<Dictionary<string, object>>();

            foreach (var record in rawRecords)
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(record));

                if (dict != null)
                {
                    var filteredDict = dict
                        .Where(kvp => _relevantFields.Contains(kvp.Key))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    filtered.Add(filteredDict);
                }
            }

            return filtered;
        }

        /// <summary>
        /// Calculate payload reduction statistics.
        /// </summary>
        public ReductionStats CalculateReduction(object original, List<Dictionary<string, object>> filtered)
        {
            var originalStr = JsonSerializer.Serialize(original);
            var filteredStr = JsonSerializer.Serialize(filtered);

            var originalBytes = originalStr.Length;
            var filteredBytes = filteredStr.Length;

            return new ReductionStats
            {
                OriginalSizeKb = originalBytes / 1024.0,
                FilteredSizeKb = filteredBytes / 1024.0,
                ReductionPercent = (1 - (double)filteredBytes / originalBytes) * 100
            };
        }
    }

    public class ReductionStats
    {
        public double OriginalSizeKb { get; set; }
        public double FilteredSizeKb { get; set; }
        public double ReductionPercent { get; set; }
    }


    // ============================================================================
    // STEP 2: SEMANTIC CHUNKING - Based on JSON Structure
    // ============================================================================

    /// <summary>
    /// Intelligently chunks JSON data based on semantic boundaries and token limits.
    /// Groups related items together while respecting token budgets.
    /// </summary>
    public class SemanticChunker
    {
        private readonly int _maxChunkTokens;
        private readonly ITokenCounter _tokenCounter;

        public SemanticChunker(ITokenCounter tokenCounter, int maxChunkTokens = 8000)
        {
            _tokenCounter = tokenCounter;
            _maxChunkTokens = maxChunkTokens;
        }

        /// <summary>
        /// Chunk records into groups that fit within token limits.
        /// </summary>
        public List<List<Dictionary<string, object>>> ChunkRecords(
            List<Dictionary<string, object>> records,
            Func<Dictionary<string, object>, (string priority, double riskScore)> getSortKey = null)
        {
            var sortKey = getSortKey ?? (r => ("MEDIUM", 0.5));

            // Sort by semantic importance (priority/risk)
            var sorted = records
                .Select(r => (record: r, key: sortKey(r)))
                .OrderByDescending(x => (x.key.priority == "HIGH" ? 3 : x.key.priority == "MEDIUM" ? 2 : 1))
                .ThenByDescending(x => x.key.riskScore)
                .Select(x => x.record)
                .ToList();

            var chunks = new List<List<Dictionary<string, object>>>();
            var currentChunk = new List<Dictionary<string, object>>();
            int currentTokens = 0;

            foreach (var record in sorted)
            {
                var recordJson = JsonSerializer.Serialize(record);
                var recordTokens = _tokenCounter.CountTokens(recordJson);

                // Start new chunk if this one would exceed limit
                if (currentTokens + recordTokens > _maxChunkTokens && currentChunk.Count > 0)
                {
                    chunks.Add(currentChunk);
                    currentChunk = new List<Dictionary<string, object>>();
                    currentTokens = 0;
                }

                currentChunk.Add(record);
                currentTokens += recordTokens;
            }

            if (currentChunk.Count > 0)
            {
                chunks.Add(currentChunk);
            }

            return chunks;
        }

        /// <summary>
        /// Create metadata for each chunk for tracking and logging.
        /// </summary>
        public List<ChunkMetadata> CreateChunkMetadata(List<List<Dictionary<string, object>>> chunks)
        {
            var metadata = new List<ChunkMetadata>();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var chunkJson = JsonSerializer.Serialize(chunk);
                var tokens = _tokenCounter.CountTokens(chunkJson);

                metadata.Add(new ChunkMetadata
                {
                    ChunkIndex = i,
                    TotalChunks = chunks.Count,
                    RecordCount = chunk.Count,
                    EstimatedTokens = tokens
                });
            }

            return metadata;
        }
    }

    public class ChunkMetadata
    {
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public int RecordCount { get; set; }
        public int EstimatedTokens { get; set; }
    }


    // ============================================================================
    // STEP 3: TOKEN BUDGET MANAGEMENT
    // ============================================================================

    /// <summary>
    /// Manages token budgets to ensure we never exceed model limits.
    /// </summary>
    public class TokenBudgetManager
    {
        private readonly int _contextWindow;
        private readonly int _maxOutputTokens;
        private readonly int _safetyMargin;
        private readonly ITokenCounter _tokenCounter;

        public TokenBudgetManager(
            ITokenCounter tokenCounter,
            int contextWindow = 128000,
            int maxOutputTokens = 4000,
            int safetyMargin = 500)
        {
            _tokenCounter = tokenCounter;
            _contextWindow = contextWindow;
            _maxOutputTokens = maxOutputTokens;
            _safetyMargin = safetyMargin;
        }

        /// <summary>
        /// Validate that a request fits within token budget before sending to LLM.
        /// </summary>
        public TokenValidationResult ValidateRequest(
            string systemPrompt,
            string userMessage,
            string jsonData)
        {
            var systemTokens = _tokenCounter.CountTokens(systemPrompt);
            var userTokens = _tokenCounter.CountTokens(userMessage);
            var dataTokens = _tokenCounter.CountTokens(jsonData);

            var totalInputTokens = systemTokens + userTokens + dataTokens;
            var availableTokens = _contextWindow - _maxOutputTokens - _safetyMargin;
            var remainingTokens = availableTokens - totalInputTokens;

            return new TokenValidationResult
            {
                SystemPromptTokens = systemTokens,
                UserMessageTokens = userTokens,
                DataTokens = dataTokens,
                TotalInputTokens = totalInputTokens,
                AvailableTokens = availableTokens,
                RemainingTokens = remainingTokens,
                FitsBudget = remainingTokens >= 0,
                UtilizationPercent = (double)totalInputTokens / availableTokens * 100
            };
        }
    }

    public class TokenValidationResult
    {
        public int SystemPromptTokens { get; set; }
        public int UserMessageTokens { get; set; }
        public int DataTokens { get; set; }
        public int TotalInputTokens { get; set; }
        public int AvailableTokens { get; set; }
        public int RemainingTokens { get; set; }
        public bool FitsBudget { get; set; }
        public double UtilizationPercent { get; set; }
    }


    // ============================================================================
    // TOKEN COUNTER INTERFACE - For token counting implementations
    // ============================================================================

    /// <summary>
    /// Interface for token counting. Implement this with your token counting strategy.
    /// </summary>
    public interface ITokenCounter
    {
        int CountTokens(string text);
    }

    /// <summary>
    /// Simple token counter (approximate - roughly 1 token per 4 characters).
    /// For production, use a more accurate implementation based on the model.
    /// </summary>
    public class ApproximateTokenCounter : ITokenCounter
    {
        private const double AvgCharsPerToken = 4.0;

        public int CountTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            return (int)Math.Ceiling(text.Length / AvgCharsPerToken);
        }
    }
}
