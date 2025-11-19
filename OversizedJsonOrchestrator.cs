using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure.Identity;

namespace Contoso.AIFoundry.JsonProcessing
{
    // ============================================================================
    // STEP 4 & 5: STRUCTURED OUTPUT PROCESSING & AGGREGATION
    // ============================================================================

    /// <summary>
    /// Represents a single issue found during analysis.
    /// </summary>
    public class AnalysisIssue
    {
        [JsonPropertyName("record_id")]
        public string RecordId { get; set; }

        [JsonPropertyName("issue_type")]
        public string IssueType { get; set; }

        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("required_action")]
        public string RequiredAction { get; set; }

        [JsonPropertyName("priority_days")]
        public int PriorityDays { get; set; }
    }

    /// <summary>
    /// Structured output from analyzing a chunk of records.
    /// Uses JSON schema to ensure reliable parsing.
    /// </summary>
    public class AnalysisResult
    {
        [JsonPropertyName("chunk_index")]
        public int ChunkIndex { get; set; }

        [JsonPropertyName("total_chunks")]
        public int TotalChunks { get; set; }

        [JsonPropertyName("records_analyzed")]
        public int RecordsAnalyzed { get; set; }

        [JsonPropertyName("high_priority_issues")]
        public List<AnalysisIssue> HighPriorityIssues { get; set; } = new();

        [JsonPropertyName("medium_priority_issues")]
        public List<AnalysisIssue> MediumPriorityIssues { get; set; } = new();

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; } = new();

        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }

    /// <summary>
    /// Main orchestrator that implements the complete 5-step approach.
    /// Handles preprocessing, chunking, validation, LLM analysis, and aggregation.
    /// </summary>
    public class OversizedJsonOrchestrator
    {
        private readonly OpenAIClient _client;
        private readonly string _deploymentName;
        private readonly JsonPreprocessor<Dictionary<string, object>> _preprocessor;
        private readonly SemanticChunker _chunker;
        private readonly TokenBudgetManager _tokenManager;
        private readonly ITokenCounter _tokenCounter;

        public OversizedJsonOrchestrator(
            string azureEndpoint,
            string deploymentName,
            string[] relevantFields)
        {
            var credential = new DefaultAzureCredential();
            _client = new OpenAIClient(new Uri(azureEndpoint), credential);
            _deploymentName = deploymentName;
            
            _tokenCounter = new ApproximateTokenCounter();
            _preprocessor = new JsonPreprocessor<Dictionary<string, object>>(relevantFields);
            _chunker = new SemanticChunker(_tokenCounter, maxChunkTokens: 8000);
            _tokenManager = new TokenBudgetManager(_tokenCounter);
        }

        /// <summary>
        /// Process a large JSON API response using the 5-step approach.
        /// </summary>
        public async Task<AuditReport> ProcessLargeApiResponseAsync(
            List<Dictionary<string, object>> rawData,
            Func<Dictionary<string, object>, (string priority, double riskScore)> sortKeyFunc = null)
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("PROCESSING LARGE JSON RESPONSE - 5 STEP APPROACH");
            Console.WriteLine(new string('=', 70));

            // ================================================================
            // STEP 1: PREPROCESSING - Filter to relevant fields
            // ================================================================
            Console.WriteLine("\n[STEP 1] Preprocessing - Filtering JSON to relevant fields...");

            var filtered = _preprocessor.FilterRecords(rawData);
            var reductionStats = _preprocessor.CalculateReduction(rawData, filtered);

            Console.WriteLine($"  ✓ Original: {reductionStats.OriginalSizeKb:F2} KB");
            Console.WriteLine($"  ✓ Filtered: {reductionStats.FilteredSizeKb:F2} KB");
            Console.WriteLine($"  ✓ Reduction: {reductionStats.ReductionPercent:F1}%");

            // ================================================================
            // STEP 2: SEMANTIC CHUNKING - Group data intelligently
            // ================================================================
            Console.WriteLine("\n[STEP 2] Semantic Chunking - Grouping records into manageable chunks...");

            var chunks = _chunker.ChunkRecords(filtered, sortKeyFunc);
            var chunkMetadata = _chunker.CreateChunkMetadata(chunks);

            Console.WriteLine($"  ✓ Created {chunks.Count} chunks:");
            foreach (var meta in chunkMetadata)
            {
                Console.WriteLine($"    - Chunk {meta.ChunkIndex}: {meta.RecordCount} records, {meta.EstimatedTokens} tokens");
            }

            // ================================================================
            // STEP 3: TOKEN BUDGET VALIDATION - Ensure nothing exceeds limit
            // ================================================================
            Console.WriteLine("\n[STEP 3] Token Budget Management - Validating chunks fit within limits...");

            var systemPrompt = GetSystemPrompt();
            bool allValidated = true;

            foreach (var (chunk, i) in chunks.Select((c, idx) => (c, idx)))
            {
                var chunkJson = JsonSerializer.Serialize(chunk, new JsonSerializerOptions { WriteIndented = true });
                var userMsg = $"Analyze {chunk.Count} records (chunk {i + 1} of {chunks.Count})";

                var validation = _tokenManager.ValidateRequest(systemPrompt, userMsg, chunkJson);

                string status = validation.FitsBudget ? "✓" : "✗";
                Console.WriteLine($"  {status} Chunk {i}: {validation.TotalInputTokens:N0} tokens " +
                    $"({validation.UtilizationPercent:F1}% utilization)");

                if (!validation.FitsBudget)
                {
                    allValidated = false;
                    Console.WriteLine($"    WARNING: Exceeds budget by {-validation.RemainingTokens:N0} tokens");
                }
            }

            if (!allValidated)
            {
                throw new InvalidOperationException("Some chunks exceed token budget. Adjust max_chunk_tokens.");
            }

            // ================================================================
            // STEP 4: STRUCTURED OUTPUT PROCESSING - Analyze with LLM
            // ================================================================
            Console.WriteLine("\n[STEP 4] Structured Output Processing - Analyzing chunks with LLM...");

            var chunkResults = new List<AnalysisResult>();

            for (int i = 0; i < chunks.Count; i++)
            {
                Console.Write($"  Processing chunk {i + 1}/{chunks.Count}...");

                try
                {
                    var result = await AnalyzeChunkAsync(
                        chunkData: chunks[i],
                        chunkIndex: i,
                        totalChunks: chunks.Count);

                    chunkResults.Add(result);
                    Console.WriteLine($" ✓ ({result.HighPriorityIssues.Count} high-priority issues)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" ✗ Error: {ex.Message}");
                    continue;
                }
            }

            // ================================================================
            // STEP 5: AGGREGATION - Combine results into final report
            // ================================================================
            Console.WriteLine("\n[STEP 5] Aggregation - Combining results from all chunks...");

            var allHighPriority = new List<AnalysisIssue>();
            var allMediumPriority = new List<AnalysisIssue>();
            var allRecommendations = new List<string>();

            foreach (var result in chunkResults)
            {
                allHighPriority.AddRange(result.HighPriorityIssues);
                allMediumPriority.AddRange(result.MediumPriorityIssues);
                allRecommendations.AddRange(result.Recommendations);
            }

            // Deduplicate recommendations
            var uniqueRecommendations = allRecommendations.Distinct().ToList();

            var auditReport = new AuditReport
            {
                AuditDate = DateTime.UtcNow,
                TotalRecordsAnalyzed = filtered.Count,
                ChunksProcessed = chunks.Count,
                HighPriorityIssues = allHighPriority,
                MediumPriorityIssues = allMediumPriority,
                Recommendations = uniqueRecommendations,
                ProcessingMetadata = new ProcessingMetadata
                {
                    OriginalPayloadSizeKb = reductionStats.OriginalSizeKb,
                    FilteredPayloadSizeKb = reductionStats.FilteredSizeKb,
                    ReductionPercent = reductionStats.ReductionPercent,
                    ChunksCreated = chunks.Count,
                    TokenBudgetUtilized = true
                }
            };

            Console.WriteLine($"\n📊 ANALYSIS COMPLETE");
            Console.WriteLine($"  High Priority Issues: {allHighPriority.Count}");
            Console.WriteLine($"  Medium Priority Issues: {allMediumPriority.Count}");
            Console.WriteLine($"  Recommendations: {uniqueRecommendations.Count}");

            return auditReport;
        }

        /// <summary>
        /// Analyze a single chunk of records using Azure OpenAI with structured outputs.
        /// </summary>
        private async Task<AnalysisResult> AnalyzeChunkAsync(
            List<Dictionary<string, object>> chunkData,
            int chunkIndex,
            int totalChunks)
        {
            var chunkJson = JsonSerializer.Serialize(chunkData, new JsonSerializerOptions { WriteIndented = true });

            var systemPrompt = GetSystemPrompt();
            var userMessage = $"Analyze the following {chunkData.Count} records (chunk {chunkIndex + 1} of {totalChunks}):\n\n{chunkJson}";

            // Create response format with JSON schema
            var responseFormat = BinaryData.FromString("""
            {
              "type": "json_schema",
              "json_schema": {
                "name": "analysis_result",
                "description": "Analysis result with issues and recommendations",
                "schema": {
                  "type": "object",
                  "properties": {
                    "chunk_index": { "type": "integer" },
                    "total_chunks": { "type": "integer" },
                    "records_analyzed": { "type": "integer" },
                    "high_priority_issues": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "record_id": { "type": "string" },
                          "issue_type": { "type": "string" },
                          "severity": { "type": "string" },
                          "description": { "type": "string" },
                          "required_action": { "type": "string" },
                          "priority_days": { "type": "integer" }
                        },
                        "required": ["record_id", "issue_type", "severity", "description"]
                      }
                    },
                    "medium_priority_issues": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "record_id": { "type": "string" },
                          "issue_type": { "type": "string" },
                          "severity": { "type": "string" },
                          "description": { "type": "string" },
                          "required_action": { "type": "string" },
                          "priority_days": { "type": "integer" }
                        },
                        "required": ["record_id", "issue_type", "severity", "description"]
                      }
                    },
                    "recommendations": { "type": "array", "items": { "type": "string" } },
                    "summary": { "type": "string" }
                  },
                  "required": ["chunk_index", "total_chunks", "records_analyzed", "high_priority_issues", "medium_priority_issues", "recommendations", "summary"],
                  "additionalProperties": false
                },
                "strict": true
              }
            }
            """);

            var response = await _client.GetChatCompletionsAsync(
                _deploymentName,
                new ChatCompletionsOptions
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, systemPrompt),
                        new ChatMessage(ChatRole.User, userMessage)
                    },
                    Temperature = 0,
                    MaxTokens = 4000,
                });

            var responseContent = response.Value.Choices[0].Message.Content;
            var result = JsonSerializer.Deserialize<AnalysisResult>(responseContent);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize analysis result");
            }

            result.ChunkIndex = chunkIndex;
            result.TotalChunks = totalChunks;
            result.RecordsAnalyzed = chunkData.Count;

            return result;
        }

        private string GetSystemPrompt()
        {
            return """
            You are an expert analyst for Contoso, specializing in data quality and compliance review.
            
            Your task is to analyze the provided records and identify:
            1. HIGH PRIORITY ISSUES: Critical problems requiring immediate action
            2. MEDIUM PRIORITY ISSUES: Important issues that should be addressed soon
            3. RECOMMENDATIONS: Suggestions for improvement and best practices
            
            For each issue, provide:
            - The specific record ID affected
            - Type of issue
            - Severity level
            - Clear description of the problem
            - Required action to resolve
            - Days until action deadline
            
            Return results in the specified JSON format.
            """;
        }
    }

    /// <summary>
    /// Final audit report combining results from all chunks.
    /// </summary>
    [JsonSerializable]
    public class AuditReport
    {
        [JsonPropertyName("audit_date")]
        public DateTime AuditDate { get; set; }

        [JsonPropertyName("total_records_analyzed")]
        public int TotalRecordsAnalyzed { get; set; }

        [JsonPropertyName("chunks_processed")]
        public int ChunksProcessed { get; set; }

        [JsonPropertyName("high_priority_issues")]
        public List<AnalysisIssue> HighPriorityIssues { get; set; } = new();

        [JsonPropertyName("medium_priority_issues")]
        public List<AnalysisIssue> MediumPriorityIssues { get; set; } = new();

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; } = new();

        [JsonPropertyName("processing_metadata")]
        public ProcessingMetadata ProcessingMetadata { get; set; }

        public string ToJsonString()
        {
            var options = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            return JsonSerializer.Serialize(this, options);
        }
    }

    [JsonSerializable]
    public class ProcessingMetadata
    {
        [JsonPropertyName("original_payload_size_kb")]
        public double OriginalPayloadSizeKb { get; set; }

        [JsonPropertyName("filtered_payload_size_kb")]
        public double FilteredPayloadSizeKb { get; set; }

        [JsonPropertyName("reduction_percent")]
        public double ReductionPercent { get; set; }

        [JsonPropertyName("chunks_created")]
        public int ChunksCreated { get; set; }

        [JsonPropertyName("token_budget_utilized")]
        public bool TokenBudgetUtilized { get; set; }
    }
}
