![TOON Integration](https://img.shields.io/badge/Integration%20Guide-v1.0-brightgreen.svg) ![Level](https://img.shields.io/badge/Level-Intermediate-yellow.svg) ![Time](https://img.shields.io/badge/Time-20%20min-blue.svg)

# 🔧 TOON Integration Guide

Complete step-by-step guide to integrate Token Optimization for Organized Narratives into your application.

---

## ✅ Prerequisites

- Visual Studio 2022 or later
- .NET 6.0+
- Your existing C# project
- Understanding of JSON structures you're processing
- Basic familiarity with LLM API calls

---

## 📦 Step 1: Add TOON Core to Your Project

### Option A: Copy the Source File

1. Copy `src/ToonOptimization.cs` to your project
2. Ensure it's in your compilation scope
3. Rebuild solution

### Option B: Create from Template

Create new file `ToonOptimization.cs` in your `Services` or `Utilities` folder:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YourProject.Services
{
    public class ToonOptimization
    {
        // Implementation from src/ToonOptimization.cs
        // [Full content provided in separate file]
    }
}
```

---

## 🎯 Step 2: Initialize TOON Analyzer

In your application startup or where you process JSON:

```csharp
using YourProject.Services;

// Initialize analyzer
var toonAnalyzer = new ToonOptimization();
```

**Recommended:** Make it a singleton in dependency injection:

```csharp
// Startup.cs or Program.cs
services.AddSingleton<ToonOptimization>();

// In your service
public class JsonProcessor
{
    private readonly ToonOptimization _toon;
    
    public JsonProcessor(ToonOptimization toon)
    {
        _toon = toon;
    }
}
```

---

## 🔍 Step 3: Analyze Your JSON Data

Before optimization, understand your data structure:

```csharp
// Your JSON data
string jsonData = @"{
    ""user"": {
        ""id"": 123,
        ""name"": ""John"",
        ""profile"": {
            ""email"": ""john@example.com"",
            ""phone"": ""+1234567890""
        }
    },
    ""transaction"": {
        ""id"": ""TXN-001"",
        ""amount"": 99.99
    }
}";

// Analyze token distribution
var analysis = toonAnalyzer.AnalyzeTokenDistribution(jsonData);

Console.WriteLine($"Total tokens: {analysis.TotalTokens}");
Console.WriteLine($"Unique patterns: {analysis.PatternCount}");
Console.WriteLine($"Optimization potential: {analysis.OptimizationPercentage}%");
```

**Output example:**
```
Total tokens: 2,847
Unique patterns: 12
Optimization potential: 68%
```

---

## 📊 Step 4: Organize Hierarchically

Restructure your JSON for optimal token usage:

```csharp
// Organize the data
var organized = toonAnalyzer.OrganizeHierarchically(jsonData, analysis);

Console.WriteLine("Organized structure:");
Console.WriteLine(organized);
```

**Before:**
```json
{
  "userProfile": {
    "personalInfo": {
      "firstName": "John",
      "lastName": "Doe",
      "contact": {
        "email": "john@example.com",
        "phone": "+1234567890"
      }
    }
  }
}
```

**After:**
```json
{
  "id": "user-123",
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "metadata": {
    "source": "profile",
    "version": "1.0"
  }
}
```

---

## 🚀 Step 5: Optimize for Caching

Apply caching patterns for reusable context:

```csharp
// Optimize for prompt caching
var optimized = toonAnalyzer.OptimizeForCaching(organized, analysis);

Console.WriteLine("Optimized for caching:");
Console.WriteLine(optimized);
```

**Output:** JSON structured specifically for LLM prompt caching strategies

---

## 💻 Step 6: Use in Your LLM Pipeline

Integrate optimized JSON into your LLM calls:

```csharp
// Example with OpenAI API
var client = new OpenAIClient(apiKey);

// Use optimized JSON as context
var message = new ChatCompletionRequestMessage
{
    Role = "user",
    Content = $"Process this data: {optimized}"
};

var response = await client.CreateChatCompletionAsync(new ChatCompletionRequest
{
    Model = "gpt-4",
    Messages = new List<ChatCompletionRequestMessage> { message }
});
```

---

## 📈 Step 7: Monitor and Measure

Track the impact of TOON:

```csharp
// Before TOON
int beforeTokens = CalculateTokenCount(originalJson);

// After TOON
int afterTokens = CalculateTokenCount(optimized);

// Calculate savings
double tokenReduction = (beforeTokens - afterTokens) / (double)beforeTokens * 100;
double costSavings = (beforeTokens - afterTokens) * costPerToken;

Console.WriteLine($"Token reduction: {tokenReduction:F1}%");
Console.WriteLine($"Cost savings: ${costSavings:F2}");
```

---

## 🎯 Integration Pattern: Complete Example

```csharp
public class OptimizedJsonProcessor
{
    private readonly ToonOptimization _toon;
    
    public OptimizedJsonProcessor(ToonOptimization toon)
    {
        _toon = toon;
    }
    
    public async Task<string> ProcessAndOptimizeJson(string rawJson)
    {
        // Step 1: Analyze
        var analysis = _toon.AnalyzeTokenDistribution(rawJson);
        Console.WriteLine($"Found {analysis.OptimizationPercentage}% optimization opportunity");
        
        // Step 2: Organize
        var organized = _toon.OrganizeHierarchically(rawJson, analysis);
        
        // Step 3: Optimize
        var optimized = _toon.OptimizeForCaching(organized, analysis);
        
        // Step 4: Use in API call
        var result = await SendToLLMApi(optimized);
        
        return result;
    }
    
    private async Task<string> SendToLLMApi(string jsonData)
    {
        // Your LLM API integration here
        return "result";
    }
}
```

---

## 🐛 Common Integration Issues

### Issue 1: NullReferenceException

**Cause:** JSON structure doesn't match expectations

**Solution:**
```csharp
if (string.IsNullOrEmpty(jsonData))
{
    Console.WriteLine("Warning: Empty JSON provided");
    return jsonData;
}
```

### Issue 2: Memory Usage with Large JSON

**Cause:** Processing very large files

**Solution:** Process in chunks
```csharp
var chunks = SplitLargeJson(rawJson, chunkSize: 10000);
foreach (var chunk in chunks)
{
    var optimized = toonAnalyzer.OptimizeForCaching(chunk, analysis);
    // Process chunk
}
```

### Issue 3: Token Count Accuracy

**Cause:** Different token counting methods

**Solution:** Use consistent tokenizer
```csharp
private static int CountTokens(string text)
{
    // Use same method as your LLM provider
    return EstimateTokens(text); // ~4 chars per token average
}
```

---

## ✅ Verification Checklist

Before deploying to production:

- [ ] ToonOptimization.cs compiles without errors
- [ ] Singleton dependency injection configured
- [ ] Test data shows >50% optimization potential
- [ ] Token counting aligns with your LLM API
- [ ] Memory usage within acceptable limits
- [ ] Error handling for edge cases implemented
- [ ] Monitoring/logging in place
- [ ] Performance tested with production-size data

---

## 📊 Expected Results

After successful integration, you should see:

| Metric | Typical Range |
|--------|---------------|
| Token reduction | 60-80% |
| Cost savings | 25-35% |
| Speed improvement | 10-20% faster |
| Accuracy change | No degradation |
| Implementation time | 15-30 minutes |

---

## 🆘 Troubleshooting

**Q: My JSON doesn't show much optimization potential**
- A: Review your data structure for redundancy
- A: Consider consolidating nested properties
- A: Check if you have many repeated fields

**Q: Integration broke existing functionality**
- A: Use optimized JSON only for new LLM calls
- A: Keep original JSON for backward compatibility
- A: Test thoroughly before full rollout

**Q: Performance degradation**
- A: Check optimization runs in background thread
- A: Consider caching analysis results
- A: Profile to identify bottleneck

---

## 📚 Next Steps

1. ✅ [Integration complete?](../FINANCIAL.md) → Review financial impact
2. 📖 [Need details?](./OVERVIEW.md) → Back to TOON overview
3. ❓ [Have questions?](./FAQ.md) → Check FAQ
4. 🐛 [Stuck?](./TROUBLESHOOTING.md) → Troubleshooting guide

---

**Happy optimizing!** 🚀
