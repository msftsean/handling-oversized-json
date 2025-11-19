# ⚡ Quick Start Guide

![Version](https://img.shields.io/badge/version-1.1.0-blue.svg)
![Status](https://img.shields.io/badge/status-Production%20Ready-brightgreen.svg)
![Time](https://img.shields.io/badge/time-5%20minutes-brightblue.svg)

**Get up and running with the oversized JSON handler in 5 minutes.**

---

## 1️⃣ Prerequisites

- .NET 8.0 or higher
- Azure OpenAI resource with gpt-4o deployment
- Azure credentials configured

### Check .NET Version

```bash
dotnet --version
# Should be 8.0.0 or higher
```

### Set Up Azure Credentials

Option A: Environment variables
```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_DEPLOYMENT="gpt-4o"
```

Option B: Direct in code
```csharp
const string AzureEndpoint = "https://your-resource.openai.azure.com/";
const string DeploymentName = "gpt-4o";
```

Option C: Azure CLI (if already logged in)
```bash
az login
# Code will use your credentials automatically
```

---

## 2️⃣ Clone & Setup

```bash
# Clone repository
git clone https://github.com/msftsean/handling-oversized-json.git
cd handling-oversized-json

# Restore dependencies
dotnet restore

# Build
dotnet build
```

---

## 3️⃣ Run Example

```bash
dotnet run
```

You should see:

```
================================================================================
LARGE JSON HANDLING WITH AZURE AI FOUNDRY
Contoso Example
================================================================================

[STEP 1] Preprocessing - Filtering JSON to relevant fields...
  ✓ Original: 19.8 KB
  ✓ Filtered: 2.3 KB
  ✓ Reduction: 88.4%

[STEP 2] Semantic Chunking - Grouping records into manageable chunks...
  ✓ Created 5 chunks

[STEP 3] Token Budget Management - Validating chunks fit within limits...
  ✓ Chunk 0: 8,234 tokens (6.7% utilization)

[STEP 4] Structured Output Processing - Analyzing chunks with LLM...
  Processing chunk 1/5... ✓ (3 high-priority issues)

[STEP 5] Aggregation - Combining results from all chunks...

📊 ANALYSIS COMPLETE
  High Priority Issues: 5
  Medium Priority Issues: 8
  Recommendations: 3

✓ Report saved to: audit_report.json
```

---

## 4️⃣ Customize for Your Data

Edit `Program.cs`:

```csharp
// Change relevant fields to match your data
var relevantFields = new[]
{
    "record_id",          // YOUR FIELDS HERE
    "status",
    "priority_level",
    "created_date",
    "assigned_to"
    // Add/remove as needed for your use case
};

// Change sort key for semantic grouping
static (string priority, double riskScore) GetSortKey(Dictionary<string, object> record)
{
    var priority = record["priority_level"].ToString();
    var riskScore = Convert.ToDouble(record["risk_score"]);
    return (priority, riskScore);  // Adjust this
}

// Replace sample data with real API call
var sampleRecords = GenerateSampleRecords(150);
// OR
var sampleRecords = await YourApiClient.GetRecordsAsync();
```

---

## 5️⃣ Check Output

Look for `audit_report.json`:

```json
{
  "audit_date": "2024-11-19T15:30:00Z",
  "total_records_analyzed": 150,
  "chunks_processed": 3,
  "high_priority_issues": [
    {
      "record_id": "REC-2024001",
      "issue_type": "missing_data",
      "severity": "HIGH",
      "description": "...",
      "required_action": "...",
      "priority_days": 3
    }
  ],
  "medium_priority_issues": [...],
  "recommendations": [...],
  "processing_metadata": {
    "original_payload_size_kb": 19800.0,
    "filtered_payload_size_kb": 231.0,
    "reduction_percent": 98.8,
    "chunks_created": 3,
    "token_budget_utilized": true
  }
}
```

---

## 6️⃣ Next Steps

- 📖 Read [FIVE_STEP_APPROACH.md](FIVE_STEP_APPROACH.md) for detailed explanation
- 🔍 Read [MODEL_DRIFT_MITIGATION_GUIDE.md](MODEL_DRIFT_MITIGATION_GUIDE.md) for monitoring
- ⚙️ Customize `relevantFields` for your domain
- 🧪 Test with your real API data
- 📊 Monitor token usage and costs
- 🚀 Deploy to production

---

## 7️⃣ Common Tasks

### Replace Sample Data with Real API

```csharp
// Instead of GenerateSampleRecords()
var records = await FetchFromYourApi();

var orchestrator = new OversizedJsonOrchestrator(
    azureEndpoint: AzureEndpoint,
    deploymentName: DeploymentName,
    relevantFields: relevantFields);

var report = await orchestrator.ProcessLargeApiResponseAsync(
    rawData: records,
    sortKeyFunc: GetSortKey);
```

### Adjust Chunk Size

```csharp
// Smaller chunks = more API calls, better granularity
var chunker = new SemanticChunker(tokenCounter, maxChunkTokens: 4000);

// Larger chunks = fewer API calls, more context
var chunker = new SemanticChunker(tokenCounter, maxChunkTokens: 10000);
```

### Enable Logging

```csharp
// Add to Program.cs
Console.WriteLine($"Analyzing {sampleRecords.Count} records...");
Console.WriteLine($"Using {relevantFields.Length} relevant fields");
// Results are printed automatically
```

### Process Multiple Batches

```csharp
var allReports = new List<AuditReport>();

foreach (var batch in batches)
{
    var report = await orchestrator.ProcessLargeApiResponseAsync(batch);
    allReports.Add(report);
}

// Combine reports...
```

---

## 8️⃣ Troubleshooting

### Error: "Azure credentials not found"
```bash
# Make sure Azure CLI is installed and you're logged in
az login

# Or set environment variables
export AZURE_OPENAI_ENDPOINT="https://..."
export AZURE_OPENAI_DEPLOYMENT="gpt-4o"
```

### Error: "Could not find deployment"
- Verify deployment name matches Azure portal
- Check AZURE_OPENAI_DEPLOYMENT environment variable
- Ensure resource is in same region

### Error: "Token limit exceeded"
- Increase preprocessing aggressiveness
- Remove more fields from relevantFields
- Reduce maxChunkTokens (e.g., 4000 instead of 8000)

### Slow Processing
- Use smaller chunks for parallel processing
- Check if token counting is accurate
- Verify JSON serialization isn't bottleneck

---

## 9️⃣ Production Deployment

When ready for production:

- [ ] Use accurate token counter (not approximate)
- [ ] Test with production-sized data
- [ ] Add error handling and retry logic
- [ ] Set up logging to Application Insights
- [ ] Monitor costs with Azure Cost Management
- [ ] Document field selection rationale
- [ ] Train team on drift monitoring
- [ ] Set up alerting for failures

---

## 🔗 Learn More

- **Detailed Guide:** [FIVE_STEP_APPROACH.md](FIVE_STEP_APPROACH.md)
- **Model Drift:** [MODEL_DRIFT_MITIGATION_GUIDE.md](MODEL_DRIFT_MITIGATION_GUIDE.md)
- **Full README:** [README.md](README.md)
- **Azure AI Foundry:** https://learn.microsoft.com/en-us/azure/ai-foundry/

---

**That's it! You're ready to handle large JSON responses with confidence.** 🚀

---

**Version:** 1.1.0 | **Released:** 2025-11-19 | **Status:** ✅ Production Ready

---

**Version:** 1.1.0 | **Released:** 2025-11-19 | **Status:** ✅ Production Ready
