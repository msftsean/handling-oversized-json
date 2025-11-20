using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Contoso.AIFoundry.JsonProcessing;

namespace Contoso.AIFoundry.Examples
{
    /// <summary>
    /// Example demonstrating the 5-step approach for handling large JSON responses
    /// that exceed LLM token limits.
    /// 
    /// Includes multiple use cases:
    /// - Supervisor incident summary (real-time dashboard)
    /// - Dispatcher historical context (active call lookup)
    /// - Compliance analysis (pattern detection)
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            PrintHeader();

            // Configuration - Replace with your Azure values
            const string AzureEndpoint = "https://your-aoai-resource.openai.azure.com/";
            const string DeploymentName = "gpt-4o";  // Your deployment name

            // Define which fields are relevant for incident analysis
            // These focus on incident narrative and context, not verbose internals
            var relevantFields = new[]
            {
                // Core incident identification
                "incident_id",
                "incident_type",
                "incident_number",
                
                // Severity and priority for smart chunking
                "severity_level",
                "priority",
                "risk_assessment",
                
                // Location context for dispatcher use cases
                "location",
                "beat",
                "district",
                
                // Timeline - essential for incident narrative
                "event_timeline",
                "dispatch_time",
                "arrival_time",
                "completion_time",
                
                // Units and personnel
                "assigned_units",
                "primary_unit",
                
                // Status and alerts
                "current_status",
                "hazmat_flag",
                "violence_flag",
                "compliance_flags"
            };

            // ================================================================
            // EXAMPLE 1: GENERATE SAMPLE INCIDENT DATA
            // ================================================================
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("SIMULATING LARGE INCIDENT DATASET");
            Console.WriteLine(new string('=', 70));

            var sampleIncidents = GenerateSampleIncidents(150);
            var apiResponse = new { records = sampleIncidents, metadata = new { total_count = sampleIncidents.Count } };

            Console.WriteLine($"Generated simulated incident dataset with {sampleIncidents.Count} incidents");
            Console.WriteLine($"Approximate original size: {JsonSerializer.Serialize(apiResponse).Length / 1024.0:F2} KB");

            // ================================================================
            // EXAMPLE 2: USE CASE - SUPERVISOR INCIDENT SUMMARY (Real-time)
            // ================================================================
            await RunSupervisorDashboardExample(
                azureEndpoint: AzureEndpoint,
                deploymentName: DeploymentName,
                relevantFields: relevantFields,
                incidents: sampleIncidents);

            // ================================================================
            // EXAMPLE 3: USE CASE - DISPATCHER HISTORICAL CONTEXT
            // ================================================================
            await RunDispatcherContextExample(
                azureEndpoint: AzureEndpoint,
                deploymentName: DeploymentName,
                relevantFields: relevantFields,
                incidents: sampleIncidents);

            // ================================================================
            // EXAMPLE 4: USE CASE - COMPLIANCE PATTERN DETECTION
            // ================================================================
            await RunComplianceAnalysisExample(
                azureEndpoint: AzureEndpoint,
                deploymentName: DeploymentName,
                relevantFields: relevantFields,
                incidents: sampleIncidents);
        }

        /// <summary>
        /// Use Case 1: Supervisor Dashboard
        /// Goal: Provide quick incident summary for district supervisor
        /// Focus: High-priority incidents, trends, recommended actions
        /// Speed: Near real-time (< 2 seconds for dashboard)
        /// </summary>
        static async Task RunSupervisorDashboardExample(
            string azureEndpoint,
            string deploymentName,
            string[] relevantFields,
            List<Dictionary<string, object>> allIncidents)
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("USE CASE 1: SUPERVISOR DASHBOARD (Real-Time Summary)");
            Console.WriteLine("Goal: Provide incident summary for supervisor dashboard");
            Console.WriteLine("Speed: < 2 seconds for real-time updates");
            Console.WriteLine(new string('=', 70));

            // Filter to HIGH severity incidents only for dashboard
            var dashboardIncidents = allIncidents
                .FindAll(i => i["severity_level"].ToString() == "HIGH")
                .Take(50)  // Limit for dashboard responsiveness
                .ToList();

            var orchestrator = new OversizedJsonOrchestrator(
                azureEndpoint: azureEndpoint,
                deploymentName: deploymentName,
                relevantFields: relevantFields);

            try
            {
                var report = await orchestrator.ProcessLargeApiResponseAsync(
                    rawData: dashboardIncidents,
                    sortKeyFunc: GetIncidentSortKey,
                    useContextVaryingPattern: false);  // Fast mode, no context-varying

                Console.WriteLine("\n📊 SUPERVISOR DASHBOARD READY");
                Console.WriteLine($"High Priority Issues: {report.HighPriorityIssues.Count}");
                Console.WriteLine($"Key Recommendations: {string.Join(", ", report.Recommendations.Take(3))}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Dashboard update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Use Case 2: Dispatcher Historical Context
        /// Goal: Provide incident history for a location during active dispatch
        /// Focus: Similar incidents, patterns, hazards, timing
        /// Speed: Fast (< 3 seconds), run in parallel with dispatch
        /// </summary>
        static async Task RunDispatcherContextExample(
            string azureEndpoint,
            string deploymentName,
            string[] relevantFields,
            List<Dictionary<string, object>> allIncidents)
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("USE CASE 2: DISPATCHER HISTORICAL CONTEXT");
            Console.WriteLine("Goal: Historical context during active dispatch");
            Console.WriteLine("Speed: < 3 seconds from query");
            Console.WriteLine(new string('=', 70));

            // Simulate: Dispatcher querying location "Downtown"
            var location = "Downtown";
            var dispatcherIncidents = allIncidents
                .FindAll(i => (i["location"].ToString().Contains(location) || 
                              (Math.Random() < 0.15)))  // Simulate location match
                .Take(100)
                .ToList();

            if (dispatcherIncidents.Count == 0)
            {
                Console.WriteLine($"No recent incidents at {location}");
                return;
            }

            var orchestrator = new OversizedJsonOrchestrator(
                azureEndpoint: azureEndpoint,
                deploymentName: deploymentName,
                relevantFields: relevantFields);

            try
            {
                var report = await orchestrator.ProcessLargeApiResponseAsync(
                    rawData: dispatcherIncidents,
                    sortKeyFunc: GetIncidentSortKey,
                    useContextVaryingPattern: false);  // Speed over context

                Console.WriteLine("\n📍 DISPATCHER CONTEXT PROVIDED");
                Console.WriteLine($"Location: {location}");
                Console.WriteLine($"Recent incidents: {report.TotalRecordsAnalyzed}");
                Console.WriteLine($"Critical Alerts: {report.HighPriorityIssues.Count}");
                if (report.Recommendations.Count > 0)
                {
                    Console.WriteLine($"Key Info: {report.Recommendations[0]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Historical context lookup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Use Case 3: Compliance Pattern Detection
        /// Goal: Detect patterns in incident data for compliance analysis
        /// Focus: Pattern detection, anomalies, relationships
        /// Speed: Batch processing (can take minutes)
        /// Strategy: Context-varying chunking for pattern continuity
        /// </summary>
        static async Task RunComplianceAnalysisExample(
            string azureEndpoint,
            string deploymentName,
            string[] relevantFields,
            List<Dictionary<string, object>> allIncidents)
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("USE CASE 3: COMPLIANCE PATTERN DETECTION");
            Console.WriteLine("Goal: Detect incident patterns for compliance analysis");
            Console.WriteLine("Strategy: Context-varying chunking for pattern preservation");
            Console.WriteLine(new string('=', 70));

            var orchestrator = new OversizedJsonOrchestrator(
                azureEndpoint: azureEndpoint,
                deploymentName: deploymentName,
                relevantFields: relevantFields);

            try
            {
                // Use context-varying pattern for pattern detection
                var report = await orchestrator.ProcessLargeApiResponseAsync(
                    rawData: allIncidents,
                    sortKeyFunc: GetIncidentSortKey,
                    useContextVaryingPattern: true);  // Enable context-varying for patterns!

                Console.WriteLine("\n✓ COMPLIANCE ANALYSIS COMPLETE");
                Console.WriteLine($"Total Incidents Analyzed: {report.TotalRecordsAnalyzed}");
                Console.WriteLine($"Chunks Processed: {report.ChunksProcessed}");
                Console.WriteLine($"Patterns Detected: {report.Recommendations.Count}");
                Console.WriteLine($"Context-Varying Pattern Used: {report.ProcessingMetadata.ContextVaryingPatternUsed}");

                // Display summary
                Console.WriteLine("\nTop Patterns/Issues:");
                foreach (var issue in report.HighPriorityIssues.Take(5))
                {
                    Console.WriteLine($"  • {issue.IssueType}: {issue.Description}");
                }

                Console.WriteLine("\nRecommendations:");
                foreach (var rec in report.Recommendations.Take(5))
                {
                    Console.WriteLine($"  → {rec}");
                }

                // Save report
                var reportJson = report.ToJsonString();
                var outputPath = "compliance_analysis_report.json";
                await File.WriteAllTextAsync(outputPath, reportJson);
                Console.WriteLine($"\n✓ Full report saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Compliance analysis failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generate sample incident records similar to what you'd get from a CAD API.
        /// </summary>
        static List<Dictionary<string, object>> GenerateSampleIncidents(int count)
        {
            var incidents = new List<Dictionary<string, object>>();
            var random = new Random(42);
            var locations = new[] { "Downtown", "North", "South", "East", "West", "Airport", "Highway 101" };
            var types = new[] { "Structure Fire", "Vehicle Accident", "Medical Emergency", "Traffic Hazard", "Welfare Check" };

            for (int i = 0; i < count; i++)
            {
                var eventCount = random.Next(2, 6);
                var events = new object[eventCount];
                for (int j = 0; j < eventCount; j++)
                {
                    events[j] = new
                    {
                        time = $"2024-11-19T{random.Next(0, 24):D2}:{random.Next(0, 60):D2}:00Z",
                        description = $"Event {j + 1}"
                    };
                }

                var incident = new Dictionary<string, object>
                {
                    { "incident_id", $"INC-2024-{2000 + i:D5}" },
                    { "incident_number", $"{2000 + i}" },
                    { "incident_type", types[i % types.Length] },
                    { "severity_level", new[] { "HIGH", "MEDIUM", "LOW" }[i % 3] },
                    { "priority", random.Next(1, 5) },
                    { "risk_assessment", random.NextDouble() },
                    { "location", locations[i % locations.Length] },
                    { "beat", $"BEAT-{(i % 20) + 1:D2}" },
                    { "district", $"District-{(i % 5) + 1}" },
                    { "dispatch_time", "2024-11-19T14:30:00Z" },
                    { "arrival_time", "2024-11-19T14:38:00Z" },
                    { "completion_time", i % 4 != 0 ? "2024-11-19T15:15:00Z" : null },
                    { "event_timeline", events },
                    { "assigned_units", new[] { $"Unit-{random.Next(1, 8)}", $"Unit-{random.Next(1, 8)}" } },
                    { "primary_unit", $"Unit-{random.Next(1, 8)}" },
                    { "current_status", new[] { "Closed", "Active", "Pending" }[i % 3] },
                    { "hazmat_flag", i % 15 == 0 },
                    { "violence_flag", i % 20 == 0 },
                    { "compliance_flags", i % 10 == 0 ? new[] { "review_needed" } : new string[0] },
                    
                    // Extra fields that will be filtered out (verbose data)
                    { "internal_notes", string.Concat(Enumerable.Repeat("Internal system note... ", 20)) },
                    { "full_history", new object[] { "hist1", "hist2", "hist3" } },
                    { "attachments", new[] { "photo1.jpg", "photo2.jpg", "report.pdf" } }
                };

                incidents.Add(incident);
            }

            return incidents;
        }

        /// <summary>
        /// Define how to extract sort keys for semantic chunking by severity.
        /// </summary>
        static (string priority, double riskScore) GetIncidentSortKey(Dictionary<string, object> incident)
        {
            var priority = incident.ContainsKey("severity_level") 
                ? incident["severity_level"].ToString() 
                : "MEDIUM";
            var riskScore = incident.ContainsKey("risk_assessment") 
                ? Convert.ToDouble(incident["risk_assessment"]) 
                : 0.5;
            return (priority, riskScore);
        }

        static void PrintHeader()
        {
            Console.WriteLine("""
            ╔════════════════════════════════════════════════════════════════════╗
            ║   LARGE INCIDENT JSON HANDLING WITH AZURE AI FOUNDRY              ║
            ║   Multi-Use Case Example                                           ║
            ╚════════════════════════════════════════════════════════════════════╝

            This example demonstrates the 5-step approach for handling large
            incident JSON responses, with multiple real-world use cases:

            Use Case 1: Supervisor Dashboard
              • Real-time incident summary for dashboard
              • Focus on high-priority incidents
              • Speed requirement: < 2 seconds
              • Strategy: Fast processing, no context-varying

            Use Case 2: Dispatcher Historical Context
              • Incident history lookup during active calls
              • Location-based queries
              • Speed requirement: < 3 seconds
              • Strategy: Fast parallel processing

            Use Case 3: Compliance Pattern Detection
              • Detect patterns across large datasets
              • Cross-chunk analysis with context preservation
              • Speed requirement: Batch processing (minutes)
              • Strategy: Context-varying chunking for patterns

            All use cases employ the 5-step approach:
            1. ✓ Preprocessing - Filter to relevant incident fields
            2. ✓ Semantic Chunking - Group by severity, location, time
            3. ✓ Token Budget - Validate before LLM submission
            4. ✓ Structured Outputs - Reliable JSON parsing
            5. ✓ Aggregation - Combine results while preserving patterns
            """);
        }
    }
}
