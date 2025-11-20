# 🛡️ Model Drift Mitigation Guide

![Version](https://img.shields.io/badge/version-1.1.0-blue.svg)
![Status](https://img.shields.io/badge/status-Reference-blue.svg)
## Continuous Monitoring and Automated Detection

---

## WHAT IS MODEL DRIFT?

**Model drift** occurs when an AI model's performance degrades over time due to changes in:

1. **Data Drift** - Input data distribution changes
   - Example: Device types change, new alert patterns emerge
   - Your preprocessing filters don't capture new fields

2. **Concept Drift** - Relationship between inputs/outputs changes
   - Example: What constitutes a "critical alert" evolves
   - Business rules change but model doesn't adapt

3. **Model Degradation** - Natural performance decline
   - Example: New edge cases not in training data
   - Provider updates model behavior

4. **Upstream Changes** - API response format changes
   - Example: Your device API adds/removes fields
   - JSON structure changes break preprocessing

---

## WHY THIS MATTERS FOR ZAVA

**Real-World Scenario:**

```
Month 1: Model correctly identifies 95% of critical alerts
         ✅ Working perfectly

Month 3: New device types added to fleet
         📉 Accuracy drops to 87%
         ⚠️  Model drift detected!

Month 6: API adds new alert categories
         📉 Accuracy drops to 78%
         🚨 Critical drift - action needed!
```

**Without Monitoring:** You don't know performance is degrading until users complain

**With AI Foundry Evals:** Automatic detection within days, not months

---

## HOW MICROSOFT FOUNDRY DETECTS DRIFT

### 1. Continuous Evaluation Pipeline

**What It Does:**
- Runs evaluations automatically on schedule (daily, weekly)
- Compares current performance against baseline
- Tracks metrics over time
- Alerts when thresholds exceeded

**How It Works:**

```
┌──────────────────────────────────────────────────────┐
│  Production Traffic                                  │
│  ↓                                                    │
│  Sample subset (e.g., 1% of requests)                │
│  ↓                                                    │
│  Run evaluation on sampled data                      │
│  ↓                                                    │
│  Compare to baseline metrics                         │
│  ↓                                                    │
│  Drift detected? → Alert team                        │
└──────────────────────────────────────────────────────┘
```

---

### 2. Baseline Comparison

**Establishing Baseline (Month 1):**

```python
# Initial evaluation establishes baseline
baseline_evaluation = {
    "accuracy": 0.95,
    "precision": 0.93,
    "recall": 0.94,
    "f1_score": 0.935,
    "avg_tokens": 15000,
    "avg_latency": 3.2,
    "user_satisfaction": 4.5/5
}
```

**Ongoing Monitoring (Monthly):**

```python
# Each month, compare to baseline
current_evaluation = run_evaluation(production_sample)

drift_detected = {
    "accuracy": baseline - current < -0.05,  # 5% drop
    "token_usage": abs(baseline - current) > 0.20,  # 20% change
    "latency": current - baseline > 1.0  # 1 sec increase
}

if any(drift_detected.values()):
    alert_team()
    trigger_investigation()
```

---

### 3. Automated Metric Tracking

**Microsoft Foundry Tracks:**

```
Quality Metrics:
├─ Accuracy / Precision / Recall
├─ Groundedness (factual correctness)
├─ Relevance (response appropriateness)
├─ Coherence (logical flow)
├─ Fluency (language quality)
└─ Custom metrics (your business rules)

Performance Metrics:
├─ Token usage (input + output)
├─ Latency (response time)
├─ Error rates
├─ Retry counts
└─ Timeout incidents

Cost Metrics:
├─ Cost per request
├─ Daily/monthly spend
├─ Cost vs budget
└─ Cost trend analysis
```

---

## IMPLEMENTING DRIFT DETECTION FOR ZAVA

### Step 1: Set Up Continuous Evaluation

**In Microsoft Foundry:**

```yaml
# continuous_evaluation.yaml

name: "Zava Device Analysis - Drift Detection"

schedule:
  frequency: "daily"  # Run every day
  time: "02:00 UTC"   # During low traffic

sampling:
  method: "random"
  percentage: 2.0     # Sample 2% of production traffic
  min_samples: 100    # Minimum 100 requests per day

evaluation_dataset:
  source: "production_logs"
  time_window: "last_24_hours"
  
metrics:
  # Quality metrics
  - accuracy
  - precision
  - recall
  - groundedness
  
  # Performance metrics
  - avg_token_usage
  - p95_latency
  - error_rate
  
  # Cost metrics
  - cost_per_request
  - daily_spend

baseline:
  reference: "baseline_eval_20250115"
  
alerts:
  - metric: "accuracy"
    threshold: -0.05      # Alert if drops 5%
    severity: "critical"
    
  - metric: "avg_token_usage"
    threshold: 0.20       # Alert if changes 20%
    severity: "warning"
    
  - metric: "error_rate"
    threshold: 0.02       # Alert if exceeds 2%
    severity: "critical"
```

---

### Step 2: Create Ground Truth Dataset

**Ground Truth = Known Correct Answers**

For Zava's use case:

```json
{
  "test_cases": [
    {
      "input": {
        "device_id": "DEV-12345",
        "alert_type": "CRITICAL_MALFUNCTION",
        "telemetry": {...}
      },
      "expected_output": {
        "classification": "CRITICAL",
        "requires_immediate_attention": true,
        "recommended_action": "Dispatch technician within 4 hours"
      }
    },
    {
      "input": {
        "device_id": "DEV-67890",
        "alert_type": "LOW_BATTERY",
        "telemetry": {...}
      },
      "expected_output": {
        "classification": "WARNING",
        "requires_immediate_attention": false,
        "recommended_action": "Schedule routine maintenance"
      }
    }
    // ... 100-500 test cases covering diverse scenarios
  ]
}
```

**How to Build:**

1. **Curate from production:**
   - Select representative examples
   - Include edge cases
   - Cover all device types and alert categories

2. **Have experts label:**
   - Technical team reviews each case
   - Marks correct expected outputs
   - Validates edge cases

3. **Update regularly:**
   - Add new device types
   - Include new alert patterns
   - Reflect business rule changes

---

### Step 3: Configure Automated Monitoring

**Azure Monitor Integration:**

```python
# alert_configuration.py

from azure.monitor.query import LogsQueryClient
from azure.identity import DefaultAzureCredential

# Configure drift detection alerts
alert_rules = [
    {
        "name": "Model Accuracy Drift",
        "condition": "accuracy < baseline_accuracy - 0.05",
        "severity": "Critical",
        "action": "email_team + create_incident",
        "frequency": "daily"
    },
    {
        "name": "Token Usage Spike",
        "condition": "avg_tokens > baseline_tokens * 1.3",
        "severity": "Warning",
        "action": "email_team",
        "frequency": "daily"
    },
    {
        "name": "Error Rate Increase",
        "condition": "error_rate > 0.02",
        "severity": "Critical",
        "action": "email_team + page_oncall",
        "frequency": "hourly"
    },
    {
        "name": "Latency Degradation",
        "condition": "p95_latency > baseline_latency + 2.0",
        "severity": "Warning",
        "action": "email_team",
        "frequency": "daily"
    }
]

# Set up Application Insights dashboard
dashboard_widgets = [
    "accuracy_over_time",      # Line chart
    "token_usage_trend",       # Area chart
    "error_rate_heatmap",      # Heatmap by hour
    "cost_vs_budget",          # Gauge
    "drift_score"              # KPI
]
```

---

### Step 4: Implement A/B Testing for Model Updates

**When New Model Versions Release:**

```
┌─────────────────────────────────────────────────────┐
│  Production Traffic                                 │
│  ↓                                                  │
│  Split:                                             │
│  ├─ 90% → Current Model (gpt-4o-2024-08-06)       │
│  └─ 10% → New Model (gpt-4o-2025-xx-xx)           │
│  ↓                                                  │
│  Evaluate both in parallel:                        │
│  ├─ Current: accuracy, cost, latency              │
│  └─ New: accuracy, cost, latency                  │
│  ↓                                                  │
│  Compare results:                                  │
│  ├─ Is new model better?                          │
│  ├─ Is cost acceptable?                           │
│  └─ Any regressions?                              │
│  ↓                                                  │
│  Decision:                                         │
│  ├─ Roll out new model (if better)                │
│  └─ Stick with current (if degraded)              │
└─────────────────────────────────────────────────────┘
```

**Configuration:**

```python
# a_b_test_config.py

ab_test = {
    "name": "GPT-4o Model Update Test",
    "start_date": "2025-01-20",
    "duration_days": 14,
    
    "variant_a": {
        "name": "Current Production",
        "model": "gpt-4o-2024-08-06",
        "traffic_percentage": 90
    },
    
    "variant_b": {
        "name": "New Model",
        "model": "gpt-4o-2025-01-15",
        "traffic_percentage": 10
    },
    
    "success_criteria": {
        "accuracy": {
            "target": ">= variant_a + 0.02",  # At least 2% better
            "required": True
        },
        "cost_per_request": {
            "target": "<= variant_a * 1.1",  # Max 10% more expensive
            "required": True
        },
        "latency": {
            "target": "<= variant_a + 0.5",  # Max 0.5 sec slower
            "required": True
        }
    },
    
    "rollout_strategy": {
        "if_success": "gradual_rollout",  # 10% → 25% → 50% → 100%
        "if_failure": "immediate_rollback"
    }
}
```

---

## DRIFT DETECTION DASHBOARD

**Microsoft Foundry Provides:**

```
┌──────────────────────────────────────────────────────────┐
│  Model Performance Dashboard - Last 30 Days             │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  Accuracy Trend                                          │
│  ████████████████████████░░░░  95% → 91% ⚠️             │
│  Jan 1 ─────────────────────────────── Jan 30           │
│                                                          │
│  Token Usage Trend                                       │
│  ██████████████████████████████  15K → 19K ⚠️           │
│  Jan 1 ─────────────────────────────── Jan 30           │
│                                                          │
│  Latency Trend                                           │
│  ████████████████████████████  3.2s → 3.1s ✅           │
│  Jan 1 ─────────────────────────────── Jan 30           │
│                                                          │
│  Error Rate                                              │
│  ██  0.5% → 0.4% ✅                                      │
│  Jan 1 ─────────────────────────────── Jan 30           │
│                                                          │
│  🚨 ALERTS                                               │
│  ├─ Accuracy dropped 4% in last week                    │
│  ├─ Token usage increased 27% in last 2 weeks           │
│  └─ Recommended: Investigate preprocessing pipeline     │
│                                                          │
│  📊 DRIFT SCORE: 6.2/10 (MODERATE DRIFT)                │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## RESPONDING TO DRIFT ALERTS

### Alert Type 1: Accuracy Drift

**Alert:** "Model accuracy dropped from 95% to 89% over last week"

**Investigation Checklist:**

1. **Check for data drift:**
   ```python
   # Analyze recent inputs
   recent_data = get_production_data(days=7)
   baseline_data = get_baseline_data()
   
   # Compare distributions
   compare_distributions(recent_data, baseline_data)
   # → Are new device types appearing?
   # → Have alert patterns changed?
   # → Is JSON structure different?
   ```

2. **Review preprocessing:**
   ```python
   # Check if filtering logic needs update
   new_fields = detect_new_fields(recent_data)
   missing_fields = detect_missing_fields(recent_data)
   
   # Update preprocessing if needed
   if new_fields:
       update_relevant_fields(new_fields)
   ```

3. **Test with ground truth:**
   ```python
   # Re-run evaluation with latest ground truth
   current_eval = run_evaluation(ground_truth_dataset)
   
   # Compare to baseline
   if current_eval.accuracy < baseline.accuracy - 0.05:
       trigger_model_update()
   ```

**Remediation:**
- Update preprocessing to capture new fields
- Retrain/fine-tune if needed
- Update ground truth dataset
- Re-establish baseline

---

### Alert Type 2: Token Usage Spike

**Alert:** "Average token usage increased 30% in last 2 weeks"

**Investigation Checklist:**

1. **Analyze token breakdown:**
   ```python
   recent_requests = sample_production_logs(n=1000)
   
   for request in recent_requests:
       input_tokens = count_tokens(request.input)
       output_tokens = count_tokens(request.output)
       
       if input_tokens > baseline_input * 1.5:
           flag_for_review(request)
   ```

2. **Check for changes:**
   - API returning more data?
   - Preprocessing logic failing?
   - Output format changed?
   - Users asking more complex questions?

3. **Cost impact:**
   ```python
   token_increase = 0.30  # 30% increase
   current_monthly_cost = 1000
   additional_cost = current_monthly_cost * token_increase
   # → $300/month additional cost
   
   if additional_cost > cost_threshold:
       trigger_optimization_review()
   ```

**Remediation:**
- Optimize preprocessing filters
- Reduce output verbosity in prompts
- Implement caching for repeated queries
- Consider switch to cheaper model if appropriate

---

### Alert Type 3: Latency Degradation

**Alert:** "P95 latency increased from 3.5s to 5.2s"

**Investigation:**

1. **Identify bottleneck:**
   - Azure OpenAI service issues?
   - Preprocessing taking longer?
   - Network latency?
   - Larger responses?

2. **Analyze patterns:**
   ```python
   slow_requests = get_requests_above_threshold(threshold=5.0)
   
   # Common characteristics?
   - Device count per request?
   - Time of day?
   - Specific alert types?
   - Geographic region?
   ```

**Remediation:**
- Optimize chunking strategy
- Enable parallel processing
- Use PTU for more predictable latency
- Add caching layer

---

## PROACTIVE DRIFT PREVENTION

### 1. Regular Ground Truth Updates

**Schedule:**
```
Weekly: Add 10-20 new production examples
Monthly: Expert review of 100 cases
Quarterly: Full ground truth refresh (500+ cases)
```

**Process:**
```python
# Automated ground truth pipeline

def maintain_ground_truth():
    # 1. Sample interesting cases from production
    interesting_cases = sample_diverse_production_data(
        strategies=[
            "high_confidence_errors",  # Model confident but wrong
            "low_confidence_correct",  # Model unsure but right
            "edge_cases",              # Unusual patterns
            "new_device_types",        # Recently added devices
            "random_sample"            # General coverage
        ],
        n_per_strategy=20
    )
    
    # 2. Send for expert labeling
    for case in interesting_cases:
        expert_label = get_expert_label(case)
        add_to_ground_truth(case, expert_label)
    
    # 3. Re-run baseline evaluation
    new_baseline = run_evaluation(updated_ground_truth)
    
    # 4. Update monitoring thresholds if needed
    update_alert_thresholds(new_baseline)
```

---

### 2. Shadow Model Testing

**What:** Run new model versions alongside production without affecting users

```
Production Request
↓
├─ Main Model (user sees this response)
│  └─ Log response + metrics
│
└─ Shadow Model (user doesn't see this)
   └─ Log response + metrics

Compare both:
- Is shadow model better?
- Any regressions?
- Ready for A/B test?
```

**Benefits:**
- Test new models risk-free
- Detect issues before users affected
- Build confidence in model updates
- Data-driven rollout decisions

---

### 3. Canary Deployments

**What:** Gradual rollout with continuous monitoring

```
Week 1: 5% of traffic → New model
        ├─ Monitor closely
        └─ Ready to rollback

Week 2: If stable → 25% of traffic
        ├─ Continue monitoring
        └─ Expand gradually

Week 3: If still stable → 50% of traffic

Week 4: If successful → 100% rollout
        └─ New model is now production
```

**Microsoft Foundry supports:**
- Traffic splitting
- Automated monitoring during rollout
- One-click rollback if issues detected

---

## MEASURING DRIFT SEVERITY

**Microsoft Foundry Drift Score (0-10):**

```python
def calculate_drift_score(current_metrics, baseline_metrics):
    """
    0-2: No drift (normal variation)
    3-5: Minor drift (monitor)
    6-7: Moderate drift (investigate)
    8-9: Significant drift (action required)
    10: Critical drift (immediate action)
    """
    
    accuracy_drift = abs(current - baseline) / baseline
    token_drift = abs(current - baseline) / baseline
    latency_drift = abs(current - baseline) / baseline
    error_drift = current - baseline
    
    # Weighted scoring
    drift_score = (
        accuracy_drift * 40 +    # Accuracy most important
        error_drift * 30 +        # Errors very important
        token_drift * 20 +        # Cost important
        latency_drift * 10        # Latency less critical
    ) * 10
    
    return min(drift_score, 10)
```

**Example Scenarios:**

```
Scenario 1: Normal Operation
├─ Accuracy: 95% → 94.5% (0.5% drop)
├─ Tokens: 15K → 15.2K (1.3% increase)
├─ Latency: 3.2s → 3.3s (3% increase)
└─ Drift Score: 1.8/10 ✅ No action needed

Scenario 2: Minor Drift
├─ Accuracy: 95% → 92% (3% drop)
├─ Tokens: 15K → 16.5K (10% increase)
├─ Latency: 3.2s → 3.5s (9% increase)
└─ Drift Score: 4.5/10 ⚠️ Monitor closely

Scenario 3: Critical Drift
├─ Accuracy: 95% → 85% (10% drop)
├─ Tokens: 15K → 22K (47% increase)
├─ Latency: 3.2s → 6.1s (91% increase)
└─ Drift Score: 8.7/10 🚨 Immediate action!
```

---

## TALKING POINTS FOR ZAVA

### "How do you prevent model drift?"

**Response:**

*"Great question - model drift is a critical concern for production AI systems. Microsoft Foundry provides enterprise-grade drift detection through continuous evaluation.*

*Here's how it works:*

*First, we establish a baseline using your ground truth dataset - essentially test cases with known correct answers. This might be 200-500 examples covering all your device types and alert scenarios.*

*Second, Microsoft Foundry automatically runs evaluations on a sample of your production traffic - typically daily. It compares current performance against the baseline across metrics like accuracy, token usage, latency, and error rates.*

*Third, when drift is detected - say accuracy drops 5% or token usage spikes 20% - the system automatically alerts your team. You get detailed dashboards showing exactly what changed and when.*

*For Zava specifically, we'd set this up to:*
- *Run daily evaluations on 2% of production traffic*
- *Alert if accuracy drops below 90%*
- *Alert if token costs increase more than 20%*
- *Provide monthly drift reports*

*We've also seen customers prevent drift proactively by:*
- *Updating ground truth monthly with new device types*
- *Shadow testing new model versions before rollout*
- *A/B testing updates with 10% of traffic first*

*The result? Instead of discovering performance issues after users complain - which might be months - you detect and fix them within days."*

---

### "What if Azure OpenAI releases a new model version?"

**Response:**

*"Another excellent question. Microsoft Foundry makes model updates safe through A/B testing and shadow deployments.*

*When OpenAI releases a new model version - say a new gpt-4o - here's our recommended process:*

*First, shadow mode: Run the new model alongside your current one for a week. Users don't see its responses yet, but you collect data on how it performs.*

*Second, analysis: Compare the shadow model's results to your current model. Is it more accurate? Does it use more tokens? Any regressions?*

*Third, A/B test: If shadow testing looks good, route 10% of traffic to the new model. Monitor both versions in parallel. Microsoft Foundry tracks:*
- *Quality metrics (accuracy, relevance)*
- *Cost metrics (tokens per request)*
- *Performance metrics (latency, errors)*

*Fourth, gradual rollout: If the 10% cohort performs well for a week, expand to 25%, then 50%, then 100%. If issues appear at any stage, one-click rollback to the previous version.*

*For example, when gpt-4o-2024-08-06 released, a typical rollout would be:*
- *Week 1: Shadow test*
- *Week 2: 10% A/B test*
- *Week 3: 50% if successful*
- *Week 4: 100% rollout*

*The entire process is monitored automatically, and you're alerted immediately if the new version underperforms."*

---

## IMPLEMENTATION TIMELINE FOR ZAVA

### Week 1-2: Setup

- [ ] Create ground truth dataset (200-500 test cases)
- [ ] Establish baseline evaluation
- [ ] Configure Microsoft Foundry continuous evaluation
- [ ] Set up monitoring dashboard
- [ ] Define alert thresholds

### Week 3-4: Validation

- [ ] Run first automated daily evaluations
- [ ] Validate alerts are working
- [ ] Train team on dashboard usage
- [ ] Document response procedures

### Month 2+: Maintenance

- [ ] Weekly: Review evaluation results
- [ ] Monthly: Update ground truth dataset
- [ ] Quarterly: Refresh baseline if needed
- [ ] Ongoing: Respond to drift alerts

---

## COST OF DRIFT MONITORING

**Evaluation Costs:**

```
Daily Evaluation:
- Sample 2% of production traffic
- 500 devices/day × 0.02 = 10 evaluations/day
- 10 evaluations × $0.10 = $1/day
- Monthly: ~$30

Ground Truth Maintenance:
- Monthly expert review: 2 hours × $100/hr = $200/month

Total: ~$230/month for comprehensive drift monitoring
```

**ROI:**
- Early detection saves weeks of degraded performance
- Prevents user complaints and reputation damage
- Reduces debugging time (days → hours)
- Enables confident model updates

**Typical scenario:** One avoided drift incident (2-week degraded performance affecting 500 daily users) easily justifies the $230/month investment.

---

## SUMMARY

**Microsoft Foundry Mitigates Drift Through:**

✅ **Continuous Evaluation** - Automated daily monitoring
✅ **Baseline Comparison** - Track performance over time
✅ **Automated Alerts** - Immediate notification of issues
✅ **Ground Truth Management** - Regularly updated test cases
✅ **A/B Testing** - Safe model update process
✅ **Shadow Deployments** - Risk-free new version testing
✅ **Drift Scoring** - Quantified severity assessment
✅ **Integrated Dashboards** - Visual performance tracking

**For Zava:**
- ~$230/month for comprehensive monitoring
- Daily drift detection (vs. months without monitoring)
- Automated alerts when issues arise
- Safe model update processes
- Data-driven decision making

**Result:** Confident production AI deployment with proactive issue detection.
