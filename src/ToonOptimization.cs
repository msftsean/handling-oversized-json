using System;
using System.Collections.Generic;
using System.Linq;

namespace Zava.AIFoundry.JsonProcessing
{
    /// <summary>
    /// Optimization levels for TOON strategy
    /// </summary>
    public enum OptimizationLevel
    {
        Conservative,
        Balanced,
        Aggressive
    }

    /// <summary>
    /// TOON Configuration: Token Optimization for Organized Narratives
    /// Configures which caching mechanisms are enabled based on optimization level
    /// </summary>
    public class ToonOptimizationConfig
    {
        public OptimizationLevel OptimizationLevel { get; }
        public bool EnableSystemPromptCaching { get; }
        public bool EnableInstructionTemplateCaching { get; }
        public bool EnableContextPreservation { get; }
        public bool EnableDynamicTokenBudgeting { get; }
        public int MaxContextWindowTokens { get; }

        public ToonOptimizationConfig(OptimizationLevel level = OptimizationLevel.Balanced)
        {
            OptimizationLevel = level;
            
            switch (level)
            {
                case OptimizationLevel.Conservative:
                    EnableSystemPromptCaching = true;
                    EnableInstructionTemplateCaching = false;
                    EnableContextPreservation = false;
                    EnableDynamicTokenBudgeting = false;
                    MaxContextWindowTokens = 4096;
                    break;
                    
                case OptimizationLevel.Balanced:
                    EnableSystemPromptCaching = true;
                    EnableInstructionTemplateCaching = true;
                    EnableContextPreservation = true;
                    EnableDynamicTokenBudgeting = true;
                    MaxContextWindowTokens = 8192;
                    break;
                    
                case OptimizationLevel.Aggressive:
                    EnableSystemPromptCaching = true;
                    EnableInstructionTemplateCaching = true;
                    EnableContextPreservation = true;
                    EnableDynamicTokenBudgeting = true;
                    MaxContextWindowTokens = 16384;
                    break;
            }
        }
    }

    /// <summary>
    /// Metrics collected during TOON optimization
    /// </summary>
    public class ToonOptimizationMetrics
    {
        public int SystemPromptCacheHits { get; set; }
        public int InstructionPromptCacheHits { get; set; }
        public int TotalTokensSavedFromSystemCache { get; set; }
        public int TotalTokensSavedFromInstructionCache { get; set; }
        
        public int TotalTokensSaved =>
            TotalTokensSavedFromSystemCache + TotalTokensSavedFromInstructionCache;
        
        public int TotalChunksProcessed =>
            SystemPromptCacheHits + InstructionPromptCacheHits;
        
        public int AverageTokensSavedPerChunk =>
            TotalChunksProcessed > 0 ? TotalTokensSaved / TotalChunksProcessed : 0;
        
        public double CacheEfficiencyPercentage =>
            TotalChunksProcessed > 0 
                ? (double)TotalTokensSaved / (TotalChunksProcessed * 500) * 100 
                : 0;
    }

    /// <summary>
    /// Implements prompt caching strategy for TOON optimization.
    /// Caches system prompts and instruction templates to reduce token usage.
    /// </summary>
    public class PromptCacheOptimizer
    {
        private string _cachedSystemPrompt;
        private Dictionary<string, string> _cachedInstructionTemplates;
        private ToonOptimizationMetrics _metrics;
        private bool _systemPromptInitialized;

        public PromptCacheOptimizer()
        {
            _cachedInstructionTemplates = new Dictionary<string, string>();
            _metrics = new ToonOptimizationMetrics();
            _systemPromptInitialized = false;
        }

        /// <summary>
        /// Gets the cached system prompt. On first call, initializes the cache.
        /// </summary>
        public string GetCachedSystemPrompt()
        {
            if (!_systemPromptInitialized)
            {
                _cachedSystemPrompt = GenerateSystemPrompt();
                _systemPromptInitialized = true;
            }
            else
            {
                _metrics.SystemPromptCacheHits++;
                _metrics.TotalTokensSavedFromSystemCache += 800; // Estimated tokens for system prompt
            }

            return _cachedSystemPrompt;
        }

        /// <summary>
        /// Gets the analysis prompt for a specific analysis type.
        /// Uses cached instruction templates when available.
        /// </summary>
        public string GetAnalysisPrompt(string analysisType, string chunkContent, string previousSummary = null)
        {
            string key = $"{analysisType}_template";
            
            if (!_cachedInstructionTemplates.ContainsKey(key))
            {
                _cachedInstructionTemplates[key] = GenerateInstructionTemplate(analysisType);
            }
            else
            {
                _metrics.InstructionPromptCacheHits++;
                _metrics.TotalTokensSavedFromInstructionCache += 300; // Estimated tokens for instruction template
            }

            return BuildAnalysisPrompt(
                _cachedInstructionTemplates[key],
                chunkContent,
                previousSummary
            );
        }

        /// <summary>
        /// Gets the current optimization metrics
        /// </summary>
        public ToonOptimizationMetrics GetMetrics() => _metrics;

        private string GenerateSystemPrompt()
        {
            return @"You are an AI analyst specializing in incident data analysis.
Your role is to:
1. Process incident information systematically
2. Identify patterns and anomalies
3. Provide actionable insights
4. Maintain consistency across multiple chunk analyses
5. Preserve context from previous chunks

Guidelines:
- Focus on accuracy and clarity
- Flag uncertain or incomplete data
- Suggest follow-up actions when appropriate
- Use structured formats for output
- Maintain thread of analysis across chunks";
        }

        private string GenerateInstructionTemplate(string analysisType)
        {
            return analysisType switch
            {
                "compliance" => @"Analyze this chunk for compliance violations:
1. Check against incident policies
2. Identify any breach patterns
3. Flag severity levels
4. Recommend corrective actions",
                
                "patterns" => @"Identify patterns in this chunk:
1. Group similar incidents
2. Find recurring themes
3. Detect outliers
4. Suggest root causes",
                
                "summary" => @"Summarize key findings from this chunk:
1. Top 3 incidents by severity
2. Geographic distribution
3. Temporal patterns
4. Key recommendations",
                
                "history" => @"Analyze historical context:
1. Compare with previous chunks
2. Track trends over time
3. Identify escalation patterns
4. Predict next incidents",
                
                _ => "Analyze and provide insights on this chunk of incident data."
            };
        }

        private string BuildAnalysisPrompt(string template, string content, string previousSummary)
        {
            string prompt = template + "\n\nData to analyze:\n" + content;
            
            if (!string.IsNullOrEmpty(previousSummary))
            {
                prompt += "\n\nPrevious analysis context:\n" + previousSummary;
            }

            return prompt;
        }
    }
}
