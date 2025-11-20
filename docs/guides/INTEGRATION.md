![Integration Guide](https://img.shields.io/badge/Integration%20Guide-v2.0-brightgreen.svg) ![Level](https://img.shields.io/badge/Level-Beginner%20to%20Intermediate-yellow.svg) ![Time](https://img.shields.io/badge/Time-30%20min%20to%202hr-blue.svg)

# 🔧 Integration Guide

Complete step-by-step guide to integrate the 5-step pipeline and optional TOON optimization into your application.

---

## ✅ Prerequisites

- Visual Studio 2022 or later
- .NET 6.0+
- Your existing C# project
- Understanding of JSON structures you're processing
- Basic familiarity with LLM API calls (OpenAI, Azure OpenAI, etc.)

---

## 📦 Step 1: Add Core Pipeline Classes to Your Project

### Option A: Copy Source Files

1. Copy these files from `src/` folder to your project:
   - `JsonPreprocessor.cs`
   - `SemanticChunker.cs`
   - `TokenBudgetManager.cs`
   - `OversizedJsonOrchestrator.cs`
2. Ensure they're in your compilation scope
3. Rebuild solution

### Option B: Create from Template

If copying files isn't practical, create the structure in your `Services/` or `Utilities/` folder:

```csharp
namespace YourProject.Services
{
    public class JsonPreprocessor { }
    public class SemanticChunker { }
    public class TokenBudgetManager { }
    public class OversizedJsonOrchestrator { }
}
```

**Note:** Full implementations are in `src/` - reference those files

---

## 🎯 Step 2: Execute the 5-Step Pipeline

Follow these steps in order in your application:

### Step 1: Preprocessing

```csharp
using YourProject.Services;

var preprocessor = new JsonPreprocessor();

// Define which fields to keep
var fieldsToKeep = new[] { "severity", "timestamp", "description", "affected_systems" };

// Filter your JSON
string originalJson = @"{ /* your large JSON */ }";
var filtered = preprocessor.FilterRecords(originalJson, fieldsToKeep);

// Check reduction
var reduction = preprocessor.CalculateReduction(originalJson, filtered);
Console.WriteLine($"Reduction: {reduction}%");
```

**Expected output:**
- Original: 19.8 KB
- Filtered: 2.3 KB
- Reduction: 88.4%

### Step 2: Semantic Chunking

```csharp
var chunker = new SemanticChunker();

// Chunk by semantic context (keeps related records together)
var chunks = chunker.ChunkBySemanticContext(
    filtered, 
    tokenLimit: 2000  // Adjust based on your model
);

Console.WriteLine($"Created {chunks.Length} chunks");
```

### Step 3: Token Budget Validation

```csharp
var validator = new TokenBudgetManager();

foreach (var chunk in chunks)
{
    // Validate each chunk
    var isValid = validator.ValidateTokenBudget(chunk, maxTokens: 3000);
    var tokenCount = validator.CountTokens(chunk);
    
    Console.WriteLine($"Chunk tokens: {tokenCount} - Valid: {isValid}");
    
    if (!isValid)
        throw new InvalidOperationException("Chunk exceeds token limit");
}
```

### Step 4: Structured Processing with LLM

```csharp
var orchestrator = new OversizedJsonOrchestrator();

// Configure your LLM connection
var llmConfig = new LlmConfiguration
{
    Endpoint = "your-azure-openai-endpoint",
    ApiKey = "your-api-key",
    Model = "gpt-4"
};

// Process all chunks
var results = await orchestrator.ProcessLargeApiResponseAsync(
    chunks,
    llmConfig
);

foreach (var result in results)
{
    Console.WriteLine($"High Priority Issues: {result.HighPriorityCount}");
    Console.WriteLine($"Medium Priority Issues: {result.MediumPriorityCount}");
}
```

### Step 5: Aggregation

```csharp
// Combine results from all chunks
var finalReport = new
{
    TotalHighPriority = results.Sum(r => r.HighPriorityCount),
    TotalMediumPriority = results.Sum(r => r.MediumPriorityCount),
    AllIssues = results.SelectMany(r => r.Issues).ToList(),
    ProcessedAt = DateTime.UtcNow
};

Console.WriteLine(JsonConvert.SerializeObject(finalReport, Formatting.Indented));
```

---

## 🚀 Step 3 (Optional): Add TOON Optimization

If you want cost optimization on top of the 5-step pipeline:

In your application startup or where you process JSON:

```csharp
using YourProject.Services;

// Step 1: Add TOON to your project
// Copy src/ToonOptimization.cs to your project

// Step 2: Initialize before your 5-step pipeline
var toon = new ToonOptimization();

// Step 3: Analyze your JSON
var analysis = toon.AnalyzeTokenDistribution(originalJson);
Console.WriteLine($"Optimization potential: {analysis.OptimizationPercentage}%");

// Step 4: Organize hierarchically
var organized = toon.OrganizeHierarchically(originalJson, analysis);

// Step 5: Optimize for caching
var optimized = toon.OptimizeForCaching(organized, analysis);

// Now use 'optimized' instead of original JSON in the 5-step pipeline
var filtered = preprocessor.FilterRecords(optimized, fieldsToKeep);
var chunks = chunker.ChunkBySemanticContext(filtered, tokenLimit: 2000);
// ... continue with steps 3-5 as above
```

**Key difference:** Use the optimized JSON output from TOON as input to Step 1 of the pipeline

---

## 📊 Complete Integration Example

### Full 5-Step Pipeline (No TOON)


```csharp
public class LargeJsonProcessor
{
    private readonly JsonPreprocessor _preprocessor;
    private readonly SemanticChunker _chunker;
    private readonly TokenBudgetManager _validator;
    private readonly OversizedJsonOrchestrator _orchestrator;
    
    public LargeJsonProcessor()
    {
        _preprocessor = new JsonPreprocessor();
        _chunker = new SemanticChunker();
        _validator = new TokenBudgetManager();
        _orchestrator = new OversizedJsonOrchestrator();
    }
    
    public async Task<AnalysisResult> ProcessLargeJson(string rawJson)
    {
        // STEP 1: Preprocessing
        var fieldsToKeep = new[] { "severity", "timestamp", "description" };
        var filtered = _preprocessor.FilterRecords(rawJson, fieldsToKeep);
        Console.WriteLine($"Preprocessing reduction: {_preprocessor.CalculateReduction(rawJson, filtered)}%");
        
        // STEP 2: Semantic Chunking
        var chunks = _chunker.ChunkBySemanticContext(filtered, tokenLimit: 2000);
        Console.WriteLine($"Created {chunks.Length} chunks");
        
        // STEP 3: Token Budget Validation
        foreach (var chunk in chunks)
        {
            var tokenCount = _validator.CountTokens(chunk);
            var isValid = _validator.ValidateTokenBudget(chunk, maxTokens: 3000);
            Console.WriteLine($"Chunk: {tokenCount} tokens - Valid: {isValid}");
        }
        
        // STEP 4: Structured Processing
        var results = await _orchestrator.ProcessLargeApiResponseAsync(chunks);
        
        // STEP 5: Aggregation
        var final = new AnalysisResult
        {
            HighPriority = results.Sum(r => r.HighPriorityCount),
            MediumPriority = results.Sum(r => r.MediumPriorityCount),
            AllIssues = results.SelectMany(r => r.Issues).ToList()
        };
        
        return final;
    }
}
```

### Full 5-Step + TOON Pipeline

```csharp
public class OptimizedLargeJsonProcessor
{
    private readonly ToonOptimization _toon;
    private readonly LargeJsonProcessor _pipeline;
    
    public OptimizedLargeJsonProcessor()
    {
        _toon = new ToonOptimization();
        _pipeline = new LargeJsonProcessor();
    }
    
    public async Task<AnalysisResult> ProcessWithOptimization(string rawJson)
    {
        // OPTIONAL: TOON Analysis Phase
        var analysis = _toon.AnalyzeTokenDistribution(rawJson);
        Console.WriteLine($"Optimization potential: {analysis.OptimizationPercentage}%");
        
        if (analysis.OptimizationPercentage < 20)
        {
            Console.WriteLine("Low optimization potential, skipping TOON");
            return await _pipeline.ProcessLargeJson(rawJson);
        }
        
        // OPTIONAL: TOON Organization Phase
        var organized = _toon.OrganizeHierarchically(rawJson, analysis);
        
        // OPTIONAL: TOON Optimization Phase
        var optimized = _toon.OptimizeForCaching(organized, analysis);
        
        // Then run the 5-step pipeline with optimized JSON
        Console.WriteLine("Running 5-step pipeline with TOON-optimized JSON");
        return await _pipeline.ProcessLargeJson(optimized);
    }
}
```

---

## 🛠️ Dependency Injection Setup (Recommended)

```csharp
// Program.cs (with dependency injection)
services.AddSingleton<JsonPreprocessor>();
services.AddSingleton<SemanticChunker>();
services.AddSingleton<TokenBudgetManager>();
services.AddSingleton<OversizedJsonOrchestrator>();
services.AddSingleton<ToonOptimization>();
services.AddSingleton<LargeJsonProcessor>();
services.AddSingleton<OptimizedLargeJsonProcessor>();

// In your controller or service
public class DataAnalysisService
{
    private readonly OptimizedLargeJsonProcessor _processor;
    
    public DataAnalysisService(OptimizedLargeJsonProcessor processor)
    {
        _processor = processor;
    }
    
    public async Task<AnalysisResult> AnalyzeIncidents(string incidentJson)
    {
        return await _processor.ProcessWithOptimization(incidentJson);
    }
}
```

}
```

---

## 🐛 Troubleshooting

### Issue 1: JSON Chunk Exceeds Token Limit

**Cause:** Chunk size exceeds token limit

**Solution:**
```csharp
// Reduce token limit per chunk
var chunks = _chunker.ChunkBySemanticContext(filtered, tokenLimit: 1000);

// Or check and split manually
foreach (var chunk in chunks)
{
    if (!_validator.ValidateTokenBudget(chunk, maxTokens: 3000))
    {
        // Split this chunk further
        var subChunks = _chunker.ChunkBySemanticContext(chunk, tokenLimit: 1000);
    }
}
```

### Issue 2: TOON Doesn't Show Improvement

**Cause:** Data is already well-optimized or has low redundancy

**Solution:**
```csharp
var analysis = _toon.AnalyzeTokenDistribution(rawJson);

// Skip TOON if potential is low
if (analysis.OptimizationPercentage < 20)
{
    Console.WriteLine("Low optimization potential, skipping TOON");
    return await _pipeline.ProcessLargeJson(rawJson);  // Skip TOON phase
}
```

### Issue 3: Memory Issues with Large JSON

**Cause:** Processing very large files at once

**Solution:** Process in batches
```csharp
var batches = SplitJsonIntoBatches(rawJson, batchSize: 5000);

foreach (var batch in batches)
{
    var result = await _processor.ProcessWithOptimization(batch);
    // Process each result
    yieldResults.Add(result);
}
```

### Issue 4: Accuracy Seems Lower

**Cause:** Over-optimization removing needed context

**Solution:**
```csharp
// Validate optimized JSON structure before processing
var analysis = _toon.AnalyzeTokenDistribution(rawJson);

// Check if critical fields are preserved
if (!ContainsCriticalFields(optimized, criticalFieldsList))
{
    throw new InvalidOperationException("Critical fields removed during optimization");
}
```

### Issue 5: NullReferenceException

**Cause:** JSON structure doesn't match expectations

**Solution:**
```csharp
if (string.IsNullOrEmpty(jsonData) || !IsValidJson(jsonData))
{
    Console.WriteLine("Warning: Invalid JSON provided");
    return null;
}

private bool IsValidJson(string json)
{
    try
    {
        JsonConvert.DeserializeObject(json);
        return true;
    }
    catch
    {
        return false;
    }
}
```

### Issue 6: Token Count Accuracy

**Cause:** Different token counting methods

**Solution:** Use consistent tokenizer matching your LLM provider
```csharp
private static int CountTokens(string text)
{
    // Use exact method from your LLM provider
    // For Azure OpenAI: use their tokenizer
    // For OpenAI: use tiktoken
    // Fallback: ~4 characters per token average
    return (int)Math.Ceiling(text.Length / 4.0);
}
```

**Cause:** Different token counting methods

**Solution:** Use consistent tokenizer
```csharp
private static int CountTokens(string text)
{
    // Use exact method from your LLM provider
    // For Azure OpenAI: use their tokenizer
    // For OpenAI: use tiktoken
    // Fallback: ~4 characters per token average
    return (int)Math.Ceiling(text.Length / 4.0);
}
```

---

## ✅ Verification Checklist

Before deploying to production:

**5-Step Pipeline:**
- [ ] JsonPreprocessor.cs compiles without errors
- [ ] SemanticChunker.cs compiles without errors
- [ ] TokenBudgetManager.cs compiles without errors
- [ ] OversizedJsonOrchestrator.cs compiles without errors
- [ ] Dependency injection configured properly
- [ ] Test data chunking works correctly
- [ ] Token validation passes for all chunks
- [ ] LLM API calls succeed with chunked data
- [ ] Results aggregate correctly

**TOON Optimization (if added):**
- [ ] ToonOptimization.cs compiles without errors
- [ ] Test data shows >30% optimization potential
- [ ] Optimized JSON maintains critical fields
- [ ] Token counting aligns with your LLM API
- [ ] Cost savings are measurable
- [ ] Accuracy doesn't degrade

**General:**
- [ ] Memory usage within acceptable limits
- [ ] Error handling for edge cases implemented
- [ ] Monitoring/logging in place
- [ ] Performance tested with production-size data
- [ ] All tests pass locally before deployment

---

## 📊 Expected Results

After successful integration:

**5-Step Pipeline Only:**

| Metric | Typical Range |
|--------|---------------|
| Preprocessing reduction | 70-95% |
| Cost savings | 15-20% |
| Accuracy improvement | +10% (context preservation) |
| Speed improvement | 10-15% faster |
| Implementation time | 30 minutes |

**With TOON Optimization:**

| Metric | Typical Range |
|--------|---------------|
| Combined token reduction | 60-80% |
| Cost savings | 25-35% |
| Speed improvement | 15-25% faster |
| Accuracy change | No degradation + caching benefits |
| Implementation time | 1-2 hours |

---

---

## 🔍 When to Use 5-Step vs When to Add TOON

| Scenario | 5-Step Only | + TOON |
|----------|----|----|
| Large JSON beyond token limits | ✅ Required | ✅ With this |
| One-off API calls | ✅ Good | ❌ Skip |
| High-volume (1K+ calls/month) | ✅ Good | ✅ Recommended |
| Cost is critical factor | ✅ OK (15-20% savings) | ✅ Better (25-35% savings) |
| Repeated context in requests | ✅ Works | ✅ Optimized |
| Development/testing | ✅ Yes | ❌ Maybe skip |
| Production environment | ✅ Required | ✅ Highly recommended |

---

## 📚 Next Steps

1. ✅ **Integration complete?** → Review [`../FINANCIAL.md`](../FINANCIAL.md) for ROI
2. 📖 **Need more details?** → Return to [`./OVERVIEW.md`](./OVERVIEW.md)
3. ❓ **Have questions?** → Check [`./FAQ.md`](./FAQ.md)
4. 🚀 **Ready to code?** → Check [`../QUICKSTART.md`](../QUICKSTART.md) for examples
5. 🏗️ **Architecture questions?** → See [`../architecture/ARCHITECTURE.md`](../architecture/ARCHITECTURE.md)

---

## 📞 Support & Issues

- **Integration stuck?** Open an issue on GitHub with integration error
- **TOON not optimizing?** Check optimization potential first (`AnalyzeTokenDistribution`)
- **Accuracy concerns?** Validate optimized JSON preserves critical fields
- **Performance issues?** Profile with production-size data before deployment

---

**Happy integrating!** 🚀

Updated: 2025-11-20 | v2.0
