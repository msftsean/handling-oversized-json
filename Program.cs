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
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            PrintHeader();

            // Configuration - Replace with your Azure values
            const string AzureEndpoint = "https://your-aoai-resource.openai.azure.com/";
            const string DeploymentName = "gpt-4o";  // Your deployment name

            // Define which fields are relevant for your use case
            var relevantFields = new[]
            {
                "record_id",
                "status",
                "priority_level",
                "created_date",
                "last_updated",
                "assigned_to",
                "compliance_flags",
                "risk_score",
                "required_actions",
                "last_review_date",
                "next_review_date",
                "documentation_status",
                "service_plan_current"
            };

            // ================================================================
            // STEP 1: GENERATE SAMPLE DATA
            // ================================================================
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("SIMULATING LARGE API RESPONSE");
            Console.WriteLine(new string('=', 70));

            var sampleRecords = GenerateSampleRecords(150);
            var apiResponse = new { records = sampleRecords, metadata = new { total_count = sampleRecords.Count } };

            Console.WriteLine($"Generated simulated response with {sampleRecords.Count} records");
            Console.WriteLine($"Approximate size: {JsonSerializer.Serialize(apiResponse).Length / 1024.0:F2} KB");

            // ================================================================
            // STEP 2: INITIALIZE ORCHESTRATOR
            // ================================================================
            var orchestrator = new OversizedJsonOrchestrator(
                azureEndpoint: AzureEndpoint,
                deploymentName: DeploymentName,
                relevantFields: relevantFields);

            // ================================================================
            // STEP 3: PROCESS THE LARGE RESPONSE
            // ================================================================
            try
            {
                var auditReport = await orchestrator.ProcessLargeApiResponseAsync(
                    rawData: sampleRecords,
                    sortKeyFunc: GetSortKey);

                // ================================================================
                // STEP 4: SAVE REPORT
                // ================================================================
                var reportJson = auditReport.ToJsonString();
                var outputPath = "audit_report.json";
                
                await File.WriteAllTextAsync(outputPath, reportJson);
                Console.WriteLine($"\n✓ Report saved to: {outputPath}");

                // Display summary
                Console.WriteLine("\n" + new string('=', 70));
                Console.WriteLine("AUDIT REPORT SUMMARY");
                Console.WriteLine(new string('=', 70));
                Console.WriteLine($"Total Records Analyzed: {auditReport.TotalRecordsAnalyzed}");
                Console.WriteLine($"Chunks Processed: {auditReport.ChunksProcessed}");
                Console.WriteLine($"High Priority Issues: {auditReport.HighPriorityIssues.Count}");
                Console.WriteLine($"Medium Priority Issues: {auditReport.MediumPriorityIssues.Count}");
                Console.WriteLine($"Recommendations: {auditReport.Recommendations.Count}");
                Console.WriteLine($"\nPayload Reduction:");
                Console.WriteLine($"  Original: {auditReport.ProcessingMetadata.OriginalPayloadSizeKb:F2} KB");
                Console.WriteLine($"  Filtered: {auditReport.ProcessingMetadata.FilteredPayloadSizeKb:F2} KB");
                Console.WriteLine($"  Reduction: {auditReport.ProcessingMetadata.ReductionPercent:F1}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error processing: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generate sample records similar to what you'd get from an API.
        /// In production, replace this with actual API calls.
        /// </summary>
        static List<Dictionary<string, object>> GenerateSampleRecords(int count)
        {
            var records = new List<Dictionary<string, object>>();
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                var record = new Dictionary<string, object>
                {
                    { "record_id", $"REC-{2024000 + i}" },
                    { "status", i % 3 != 0 ? "ACTIVE" : "PENDING" },
                    { "priority_level", new[] { "HIGH", "MEDIUM", "LOW" }[i % 3] },
                    { "created_date", $"2024-{(i % 12) + 1:D2}-01T00:00:00Z" },
                    { "last_updated", "2024-11-10T12:00:00Z" },
                    { "assigned_to", $"USER-{(i % 20) + 1:D3}" },
                    { "compliance_flags", i % 5 == 0 ? new[] { "overdue_action" } : new string[0] },
                    { "risk_score", (i % 10) / 10.0 },
                    { "required_actions", new[] { "review", "document" } },
                    { "last_review_date", i % 7 != 0 ? "2024-10-15T00:00:00Z" : null },
                    { "next_review_date", "2024-11-20T00:00:00Z" },
                    { "documentation_status", i % 4 == 0 ? "COMPLETE" : "INCOMPLETE" },
                    { "service_plan_current", i % 3 != 0 },

                    // Extra fields that will be filtered out (simulating bloated API response)
                    { "internal_notes", string.Concat(Enumerable.Repeat("Lorem ipsum dolor sit amet... ", 10)) },
                    { "history", new object[] { "event1", "event2", "event3" } },
                    { "attachments", new[] { "doc1.pdf", "doc2.pdf", "doc3.pdf", "doc4.pdf" } }
                };

                records.Add(record);
            }

            return records;
        }

        /// <summary>
        /// Define how to extract sort keys for semantic chunking.
        /// This groups related records together while respecting token limits.
        /// </summary>
        static (string priority, double riskScore) GetSortKey(Dictionary<string, object> record)
        {
            var priority = record.ContainsKey("priority_level") ? record["priority_level"].ToString() : "MEDIUM";
            var riskScore = record.ContainsKey("risk_score") ? Convert.ToDouble(record["risk_score"]) : 0.5;
            return (priority, riskScore);
        }

        static void PrintHeader()
        {
            Console.WriteLine("""
            ╔════════════════════════════════════════════════════════════════════╗
            ║   LARGE JSON HANDLING WITH AZURE AI FOUNDRY                        ║
            ║   Contoso Example                                                  ║
            ╚════════════════════════════════════════════════════════════════════╝

            This example demonstrates the 5-step approach for handling large JSON
            responses that exceed LLM token limits:

            1. ✓ Preprocessing Layer - Filter JSON to relevant fields only
            2. ✓ Semantic Chunking - Group data intelligently by context
            3. ✓ Token Budget Management - Validate before sending to LLM
            4. ✓ Structured Outputs - Use gpt-4o JSON schema for reliable parsing
            5. ✓ Aggregation & Reporting - Combine results from all chunks
            """);
        }
    }
}
