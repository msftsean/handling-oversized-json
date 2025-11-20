# Model Drift Monitoring & Evaluation Guide
## For Large Language Model Incident Analysis

---

## Overview

**From the meeting:** "Model drift requires frequent evaluations, recommending at least weekly checks to maintain output quality."

This guide provides concrete steps for monitoring LLM performance on incident analysis tasks using Microsoft Foundry's evaluation tools.

---

## Understanding Model Drift

### 🔍 What is Model Drift?

Model drift occurs when the LLM's output quality degrades over time due to:

- **Input distribution shift**: Your incident data changes (new incident types, formats)
- **Model updates**: Azure updates the underlying gpt-4o model
- **Prompt changes**: You modify prompts, changing expected output
- **Context changes**: Your chunking strategy evolves

### ⚠️ Why It Matters for Incident Analysis

Poor incident summaries can cause:
- Dispatchers miss critical context
- Supervisors make decisions based on incomplete information
- Compliance team detects patterns late
- Response times increase
- Public safety impacted

### 📈 Detection Strategy

Weekly evaluation catches drift early, before it affects operations:

```
Week 1: Establish baseline metrics
Week 2-4: Compare to baseline, alert if >5% drop
Week 5+: Trend analysis, identify patterns
```

---

## Weekly Evaluation Checklist

### Setup (One-time)

```csharp
// Step 1: Create reference summaries (human-written gold standard)
var referenceSummaries = new Dictionary<string, string>()
{
    ["incident-batch-1"] = "Summary written by experienced supervisor...",
    ["incident-batch-2"] = "Summary written by experienced supervisor...",
    // Collect 10-20 reference summaries from your team
};

// Step 2: Store baseline metrics
var baseline = await EvaluateModelPerformance(
    incidents: sampleIncidents,
    model: "gpt-4o",
    references: referenceSummaries);

await StoreBaseline(baseline);
```

### Weekly Process

```csharp
public class ModelDriftMonitor
{
    public async Task RunWeeklyEvaluation()
    {
        // Get this week's analyses
        var thisWeekAnalyses = await FetchAnalysesSince(
            DateTime.Now.AddDays(-7));
        
        // Evaluate performance
        var currentMetrics = await EvaluateAnalyses(thisWeekAnalyses);
        
        // Compare to baseline
        var baseline = await GetStoredBaseline();
        var drift = ComputeDrift(currentMetrics, baseline);
        
        // Alert if needed
        if (drift.AccuracyDrop > 0.05)  // >5% drop
        {
            await AlertTeam("Model drift detected", drift);
            await InvestigateAndRemediate(drift);
        }
        
        // Store this week's metrics for trend analysis
        await StoreMetrics(currentMetrics);
    }
}
```

---

## Evaluation Metrics

### Primary Metrics

**1. BLEU Score (Content Matching)**
- Measures how similar LLM output is to human reference summaries
- Range: 0-100 (higher is better)
- Threshold: Should stay within ±5 of baseline
- Check: Compare against reference summaries

```csharp
var bleuScore = CalculateBLEUScore(
    llmSummary: llmOutput,
    referenceSummaries: humanSummaries);
// Target: 75-85 for incident analysis
```

**2. ROUGE Score (Content Overlap)**
- Measures overlap in meaningful content
- Range: 0-1 (higher is better)
- Threshold: Should stay ≥0.70
- Check: Precision and recall of key phrases

```csharp
var rougeScore = CalculateROUGEScore(
    llmOutput,
    humanReferences);
// Target: 0.72-0.82
```

**3. Semantic Similarity (Meaning Preservation)**
- Measures if LLM understood the incident correctly
- Range: 0-1 (higher is better)
- Threshold: Should stay ≥0.80
- Check: Use embeddings to compare meaning

```csharp
var similarity = await CalculateSemanticSimilarity(
    llmSummary,
    referenceSummary,
    embeddingModel: "text-embedding-3-large");
// Target: 0.80+
```

### Secondary Metrics

**4. Action Extraction Accuracy**
- What % of required actions did the LLM identify?
- Range: 0-100 (higher is better)
- Threshold: Should be 100% (can't miss critical actions)

```csharp
var actionAccuracy = await EvaluateActionExtraction(
    llmOutput,
    expectedActions: referenceSummary.Actions);
// Target: 100%
```

**5. Severity Prediction Accuracy**
- Did the LLM correctly classify incident severity?
- Range: 0-100 (higher is better)
- Threshold: Should stay ≥95% (one misclassification per 20 incidents max)

```csharp
var severityAccuracy = await CalculateSeverityAccuracy(
    llmPredictions,
    humanLabels);
// Target: 95%+
```

**6. Response Time Compliance**
- Does the analysis complete within SLA?
- Threshold: For supervisor dashboard, <2 seconds
- Check: Monitor latency over time

```csharp
var avgLatency = await MeasureAverageLatency(
    thisWeekAnalyses);
// Target: <2 seconds for real-time uses
```

---

## Using Microsoft Foundry Evaluation Tools

### Step 1: Set Up Evaluation Dataset

```csharp
// Prepare evaluation dataset
var evaluationData = new List<(string incident, string reference)>();

// Collect diverse incident samples
var samples = await FetchIncidentSamples(
    count: 50,
    stratified: true);  // Mix of severity levels, types, districts

foreach (var sample in samples)
{
    var reference = GetHumanSummary(sample.id);
    evaluationData.Add((sample.json, reference));
}

// Save for Microsoft Foundry
await SaveEvaluationDataset(evaluationData, "incident-eval-dataset-2024-11");
```

### Step 2: Create Evaluation Configuration

```csharp
var evaluationConfig = new EvaluationConfiguration
{
    DatasetName = "incident-eval-dataset-2024-11",
    MetricsList = new[]
    {
        MetricType.BLEU,
        MetricType.ROUGE,
        MetricType.SemanticSimilarity,
        MetricType.F1,
        MetricType.ExactMatch
    },
    Model = "gpt-4o",
    Prompts = new[]
    {
        GetSuperviserSummaryPrompt(),
        GetDispatcherContextPrompt(),
        GetComplianceAnalysisPrompt()
    }
};

var evaluationId = await foundryClient.StartEvaluation(evaluationConfig);
```

### Step 3: Analyze Results

```csharp
// Poll for results
var results = await WaitForEvaluation(evaluationId);

// Display metrics
Console.WriteLine($"Model: gpt-4o");
Console.WriteLine($"Dataset: incident-eval-dataset-2024-11");
Console.WriteLine($"BLEU Score: {results.Metrics[MetricType.BLEU]:.2f}");
Console.WriteLine($"ROUGE Score: {results.Metrics[MetricType.ROUGE]:.4f}");
Console.WriteLine($"Semantic Similarity: {results.Metrics[MetricType.SemanticSimilarity]:.4f}");

// Compare to baseline
var baseline = await GetStoredBaseline();
foreach (var metric in results.Metrics)
{
    var baselineValue = baseline.Metrics[metric.Key];
    var change = metric.Value - baselineValue;
    var percent = (change / baselineValue) * 100;
    
    var status = Math.Abs(change) <= 0.05 ? "✓ STABLE" : "⚠️ DRIFT";
    Console.WriteLine($"{metric.Key}: {change:+0.0000} ({percent:+0.0}%) {status}");
}
```

---

## Setting Up Automated Weekly Evaluations

### Option 1: Azure Function (Scheduled)

```csharp
[FunctionName("WeeklyModelDriftCheck")]
public async Task RunWeeklyCheck(
    [TimerTrigger("0 9 * * MON")] TimerInfo timer,  // Every Monday 9 AM
    IAsyncCollector<string> alertQueue)
{
    var monitor = new ModelDriftMonitor();
    var result = await monitor.RunWeeklyEvaluation();
    
    if (result.DriftDetected)
    {
        await alertQueue.AddAsync(JsonSerializer.Serialize(result));
    }
}
```

### Option 2: Scheduled Container Task

```bash
#!/bin/bash
# File: weekly-model-eval.sh

# Run weekly evaluation
dotnet run --project ModelDriftMonitor \
    --dataset incident-eval-dataset \
    --model gpt-4o \
    --save-results true

# Alert if drift detected
if [ $? -ne 0 ]; then
    curl -X POST https://your-alert-webhook.com \
        -d "Model drift detected in weekly evaluation"
fi
```

### Option 3: Weekly Manual Check

```csharp
// In your monitoring dashboard
public class DriftMonitoringDashboard
{
    [HttpPost("trigger-evaluation")]
    public async Task<IActionResult> TriggerEvaluation()
    {
        var results = await _modelMonitor.RunWeeklyEvaluation();
        
        return Ok(new
        {
            Status = results.DriftDetected ? "DRIFT_DETECTED" : "STABLE",
            Metrics = results.Metrics,
            Recommendation = results.Recommendation
        });
    }
}
```

---

## Responding to Drift

### Alert Thresholds

```
🟢 GREEN (No Action):
- All metrics within ±5% of baseline
- Action: Continue monitoring

🟡 YELLOW (Watch):
- One metric 5-10% below baseline
- Action: Increase monitoring frequency to twice weekly
- Action: Check for input distribution changes

🔴 RED (Investigate):
- One metric >10% below baseline, or
- Multiple metrics drifting simultaneously
- Action: Immediate investigation required
```

### Investigation Steps

```csharp
public async Task InvestigateDrift(DriftAlert alert)
{
    // 1. Check input distribution
    var recentIncidents = await FetchRecentIncidents(7);  // Last 7 days
    var oldIncidents = await FetchHistoricalIncidents(7, 14);  // Days 8-14
    
    var distribution = CompareDistributions(recentIncidents, oldIncidents);
    if (distribution.Shift > 0.2)  // >20% difference
    {
        Console.WriteLine("Input distribution shifted!");
        Console.WriteLine($"New incident types: {distribution.NewTypes}");
    }
    
    // 2. Check model updates
    var modelInfo = await _client.GetModelInfo("gpt-4o");
    if (modelInfo.LastUpdated > _lastEvaluationTime)
    {
        Console.WriteLine($"Model updated: {modelInfo.LastUpdated}");
        Console.WriteLine("Drift may be due to model changes");
    }
    
    // 3. Check prompt effectiveness
    var failedAnalyses = await FetchAnalysesWith(
        confidence: threshold=0.5);  // Low confidence outputs
    
    Console.WriteLine($"Found {failedAnalyses.Count} low-confidence outputs");
    LogFailurePatterns(failedAnalyses);
}
```

### Remediation Options

**Option 1: Adjust Prompts**
```csharp
// Original prompt may be losing effectiveness
var newPrompt = """
You are summarizing incident data for supervisors.

IMPORTANT: Recent incidents show changing patterns.
Focus especially on:
- Emerging incident types (e.g., cyber-related)
- Multi-unit coordinations
- Cross-district patterns

Be concise and actionable.
""";
```

**Option 2: Retrain Evaluation Baseline**
```csharp
// If incident types legitimately changed
await monitor.ReestablishBaseline(
    newIncidents: await FetchLastMonthIncidents(),
    reason: "Incident distribution shifted due to seasonal changes");
```

**Option 3: Switch Models**
```csharp
// If drift is severe and consistent
var performance = await CompareModels(
    models: new[] { "gpt-4o", "gpt-4o-mini", "gpt-4-turbo" },
    dataset: evaluationDataset);

foreach (var result in performance)
{
    Console.WriteLine($"{result.Model}: {result.Score}% accuracy");
}

// If gpt-4o-mini is close, switch for cost savings!
```

**Option 4: Model Update Strategy**
```csharp
// Coordinate with model update schedule
Console.WriteLine("Azure just released gpt-4o version 2024-11-19");
Console.WriteLine("Option 1: Gradual rollout (10% traffic)");
Console.WriteLine("Option 2: Immediate update with rebaselining");
Console.WriteLine("Option 3: A/B test old vs new model");
```

---

## Model Comparison Evaluation

### Comparing Multiple Models

When choosing a model for production, compare performance:

```csharp
public async Task CompareModelsForIncidentAnalysis()
{
    var candidates = new[] { "gpt-4o", "gpt-4o-mini", "gpt-4-turbo" };
    var results = new Dictionary<string, ModelPerformance>();
    
    foreach (var model in candidates)
    {
        Console.Write($"Evaluating {model}...");
        
        var metrics = await EvaluateModel(
            dataset: "incident-eval-dataset",
            model: model);
        
        results[model] = new ModelPerformance
        {
            Model = model,
            Accuracy = metrics.F1Score,
            Cost = await EstimateMonthlyCost(model, 500),  // 500 analyses/month
            Latency = metrics.AvgLatency
        };
        
        Console.WriteLine($" ✓ {metrics.F1Score:.1%} accuracy");
    }
    
    // Display comparison
    Console.WriteLine("\nModel Comparison:");
    Console.WriteLine("┌─────────────┬──────────────┬────────┬────────┐");
    Console.WriteLine("│ Model       │ Accuracy     │ Cost   │ Latency│");
    Console.WriteLine("├─────────────┼──────────────┼────────┼────────┤");
    foreach (var (model, perf) in results)
    {
        Console.WriteLine($"│ {model,-11} │ {perf.Accuracy,12:P1} │ ${perf.Cost,6:F2} │ {perf.Latency,5}ms │");
    }
    Console.WriteLine("└─────────────┴──────────────┴────────┴────────┘");
    
    // Recommend
    var recommended = SelectBestModel(results);
    Console.WriteLine($"\n→ Recommended: {recommended.Model} (best trade-off)");
}
```

---

## Trend Analysis & Reporting

### Monthly Trend Report

```csharp
public async Task GenerateMonthlyReport()
{
    var thisMonth = await FetchMetricsForMonth(DateTime.Now);
    var lastMonth = await FetchMetricsForMonth(DateTime.Now.AddMonths(-1));
    
    Console.WriteLine("╔════════════════════════════════════════════╗");
    Console.WriteLine("║ Monthly Model Performance Report           ║");
    Console.WriteLine("╚════════════════════════════════════════════╝");
    Console.WriteLine();
    
    // Weekly trends
    Console.WriteLine("Week-by-Week Metrics:");
    foreach (var week in GetWeeks(thisMonth))
    {
        var avgBleu = week.Metrics.Average(m => m.BLEU);
        var trend = week.Number == 1 ? "-" : 
                   avgBleu > GetLastWeekAverage() ? "📈" : "📉";
        Console.WriteLine($"  Week {week.Number}: BLEU={avgBleu:.1f} {trend}");
    }
    
    Console.WriteLine();
    Console.WriteLine("Summary:");
    Console.WriteLine($"  Month-over-month change: {Metric_Diff}%");
    Console.WriteLine($"  Incidents analyzed: {thisMonth.Count}");
    Console.WriteLine($"  Average latency: {thisMonth.AvgLatency}ms");
    Console.WriteLine($"  Critical failures: {thisMonth.Failures}");
    
    if (Metric_Diff < -0.05)
    {
        Console.WriteLine();
        Console.WriteLine("⚠️ RECOMMENDATION: Quality degrading");
        Console.WriteLine("   - Review recent prompt changes");
        Console.WriteLine("   - Check incident type distribution");
        Console.WriteLine("   - Consider model update");
    }
}
```

---

## CJIS Compliance Monitoring

### From the Meeting

**Open question:** "CJIS compliance of Gov Cloud models needs verification."

For incident data in Gov Cloud, add compliance checks:

```csharp
public class CJISComplianceMonitor
{
    public async Task VerifyCompliance()
    {
        // Check 1: Data residency
        var modelLocation = await GetModelDeploymentLocation("gpt-4o");
        Console.WriteLine($"Model deployed in: {modelLocation}");
        
        if (!IsCJISCompliant(modelLocation))
        {
            Console.WriteLine("⚠️ WARNING: Model not in CJIS-compliant region");
        }
        
        // Check 2: Encryption status
        var encryption = await VerifyEncryption();
        Console.WriteLine($"Encryption: {encryption.Status}");
        
        // Check 3: Access logging
        var auditLog = await CheckAccessLogging();
        Console.WriteLine($"Access logs: {auditLog.RecordCount} entries");
        
        // Check 4: Model SOC 2 compliance
        var modelCompliance = await GetModelComplianceCertification("gpt-4o");
        Console.WriteLine($"Model SOC 2 Type II: {modelCompliance.HasCertification}");
    }
}
```

---

## Escalation Path

When serious drift is detected:

```
Level 1: Automated alert to team Slack
         → Investigate distribution changes
         → Check model updates
         
Level 2: 5-10% drift on multiple metrics
         → Create incident ticket
         → Notify lead analyst
         → Plan retraining
         
Level 3: >10% drift OR action extraction failures
         → Immediate escalation to leadership
         → May require model rollback
         → Pause production until resolved
         
Level 4: CJIS compliance violation
         → Halt all processing
         → Security incident report
         → Notify compliance officer
```

---

## Summary

Model drift monitoring is critical for maintaining incident analysis quality:

- **Weekly evaluations**: Catch issues early
- **Automated alerts**: Notify team immediately
- **Trend tracking**: Identify patterns over time
- **Fast remediation**: Adjust prompts or models quickly
- **Compliance**: Ensure CJIS requirements maintained

Recommended cadence:
- **Daily**: Latency and error tracking
- **Weekly**: Full metric evaluation
- **Monthly**: Trend analysis and reporting
- **Quarterly**: Model comparison and strategy review

---

**Next Steps:**
1. Set up evaluation dataset with 20-50 reference summaries
2. Run baseline evaluation
3. Schedule weekly automated checks
4. Create team alert channels
5. Plan quarterly model updates

