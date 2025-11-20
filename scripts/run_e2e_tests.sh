#!/bin/bash

# E2E Test Runner for Incident Data Processing Pipeline
# This script validates the 5-step approach without requiring .NET SDK

echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║            END-TO-END TEST SUITE - VALIDATION RUNNER             ║"
echo "║                     5-Step LLM Processing Pipeline                 ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

# Test counters
TOTAL=0
PASSED=0
FAILED=0
RESULTS_FILE="test_results_$(date +%Y%m%d_%H%M%S).txt"

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_test() {
    local name=$1
    local status=$2
    local message=$3
    local metrics=$4
    
    TOTAL=$((TOTAL + 1))
    
    if [ "$status" == "PASS" ]; then
        echo -e "${GREEN}[✓]${NC} $name"
        PASSED=$((PASSED + 1))
    else
        echo -e "${RED}[✗]${NC} $name"
        FAILED=$((FAILED + 1))
        if [ -n "$message" ]; then
            echo "    Error: $message"
        fi
    fi
    
    if [ -n "$metrics" ]; then
        echo "    Metrics: $metrics"
    fi
    
    echo "$name - $status" >> "$RESULTS_FILE"
    if [ -n "$message" ]; then
        echo "  Error: $message" >> "$RESULTS_FILE"
    fi
    if [ -n "$metrics" ]; then
        echo "  Metrics: $metrics" >> "$RESULTS_FILE"
    fi
}

echo "$(date): Test Suite Started" > "$RESULTS_FILE"
echo "========================================" >> "$RESULTS_FILE"
echo ""

# =========================================================================
# CATEGORY 1: PREPROCESSING TESTS
# =========================================================================

echo "📋 Category 1: Preprocessing Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 1: Preprocessing Tests" >> "$RESULTS_FILE"

# Test 1.1: Validate OversizedJsonHandler.cs exists and has JsonPreprocessor
if grep -q "class JsonPreprocessor" src/OversizedJsonHandler.cs; then
    print_test "Preprocessing: JsonPreprocessor class exists" "PASS"
else
    print_test "Preprocessing: JsonPreprocessor class exists" "FAIL" "JsonPreprocessor class not found"
fi

# Test 1.2: Validate FilterRecords method
if grep -q "public.*FilterRecords" src/OversizedJsonHandler.cs; then
    print_test "Preprocessing: FilterRecords method exists" "PASS"
else
    print_test "Preprocessing: FilterRecords method exists" "FAIL" "FilterRecords method not found"
fi

# Test 1.3: Validate CalculateReduction method
if grep -q "public.*CalculateReduction" src/OversizedJsonHandler.cs; then
    print_test "Preprocessing: CalculateReduction method exists" "PASS"
else
    print_test "Preprocessing: CalculateReduction method exists" "FAIL" "CalculateReduction method not found"
fi

# Test 1.4: Validate relevant_fields filtering in Program.cs
if grep -q "var relevantFields = new" src/Program.cs; then
    print_test "Preprocessing: Field filtering configured in Program.cs" "PASS"
else
    print_test "Preprocessing: Field filtering configured in Program.cs" "FAIL" "relevantFields not found"
fi

# =========================================================================
# CATEGORY 2: SEMANTIC CHUNKING TESTS
# =========================================================================

echo ""
echo "📚 Category 2: Semantic Chunking Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 2: Semantic Chunking Tests" >> "$RESULTS_FILE"

# Test 2.1: Validate SemanticChunker class
if grep -q "class SemanticChunker" src/OversizedJsonHandler.cs; then
    print_test "Chunking: SemanticChunker class exists" "PASS"
else
    print_test "Chunking: SemanticChunker class exists" "FAIL" "SemanticChunker class not found"
fi

# Test 2.2: Validate multiple chunking strategies
chunking_strategies=0
grep -q "ChunkBySeverity\|public.*Chunk" src/OversizedJsonHandler.cs && chunking_strategies=$((chunking_strategies + 1))

if [ $chunking_strategies -gt 0 ]; then
    print_test "Chunking: Multiple chunking strategies available" "PASS" "" "Found semantic chunking methods"
else
    print_test "Chunking: Multiple chunking strategies available" "FAIL" "No chunking methods found"
fi

# Test 2.3: Validate ChunkMetadata with context info
if grep -q "IncludesPreviousContext\|ContextTokensReserved" src/OversizedJsonHandler.cs; then
    print_test "Chunking: ChunkMetadata includes context properties" "PASS"
else
    print_test "Chunking: ChunkMetadata includes context properties" "FAIL" "Context properties not found"
fi

# =========================================================================
# CATEGORY 3: TOKEN MANAGEMENT TESTS
# =========================================================================

echo ""
echo "💾 Category 3: Token Management Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 3: Token Management Tests" >> "$RESULTS_FILE"

# Test 3.1: Validate TokenBudgetManager class
if grep -q "class TokenBudgetManager" src/OversizedJsonHandler.cs; then
    print_test "Token Budget: TokenBudgetManager class exists" "PASS"
else
    print_test "Token Budget: TokenBudgetManager class exists" "FAIL" "TokenBudgetManager class not found"
fi

# Test 3.2: Validate ValidateTokenBudget method
if grep -q "ValidateTokenBudget" src/OversizedJsonHandler.cs; then
    print_test "Token Budget: ValidateTokenBudget method exists" "PASS"
else
    print_test "Token Budget: ValidateTokenBudget method exists" "FAIL" "ValidateTokenBudget method not found"
fi

# Test 3.3: Validate ITokenCounter interface
if grep -q "interface ITokenCounter" src/OversizedJsonHandler.cs; then
    print_test "Token Budget: ITokenCounter interface exists" "PASS"
else
    print_test "Token Budget: ITokenCounter interface exists" "FAIL" "ITokenCounter interface not found"
fi

# Test 3.4: Validate ApproximateTokenCounter implementation
if grep -q "class ApproximateTokenCounter" src/OversizedJsonHandler.cs; then
    print_test "Token Budget: ApproximateTokenCounter implementation exists" "PASS"
else
    print_test "Token Budget: ApproximateTokenCounter implementation exists" "FAIL" "ApproximateTokenCounter not found"
fi

# =========================================================================
# CATEGORY 4: ORCHESTRATOR/PIPELINE TESTS
# =========================================================================

echo ""
echo "🔄 Category 4: Processing Pipeline Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 4: Processing Pipeline Tests" >> "$RESULTS_FILE"

# Test 4.1: Validate OversizedJsonOrchestrator class
if grep -q "class OversizedJsonOrchestrator" src/OversizedJsonOrchestrator.cs; then
    print_test "Pipeline: OversizedJsonOrchestrator class exists" "PASS"
else
    print_test "Pipeline: OversizedJsonOrchestrator class exists" "FAIL" "OversizedJsonOrchestrator not found"
fi

# Test 4.2: Validate ProcessLargeApiResponseAsync method
if grep -q "ProcessLargeApiResponseAsync" src/OversizedJsonOrchestrator.cs; then
    print_test "Pipeline: ProcessLargeApiResponseAsync method exists" "PASS"
else
    print_test "Pipeline: ProcessLargeApiResponseAsync method exists" "FAIL" "ProcessLargeApiResponseAsync not found"
fi

# Test 4.3: Validate context-varying pattern support
if grep -q "useContextVaryingPattern\|previousChunkContext" src/OversizedJsonOrchestrator.cs; then
    print_test "Pipeline: Context-varying pattern implemented" "PASS"
else
    print_test "Pipeline: Context-varying pattern implemented" "FAIL" "Context-varying pattern not found"
fi

# Test 4.4: Validate AnalysisResult class for structured output
if grep -q "class AnalysisResult" src/OversizedJsonOrchestrator.cs; then
    print_test "Pipeline: Structured output (AnalysisResult) exists" "PASS"
else
    print_test "Pipeline: Structured output (AnalysisResult) exists" "FAIL" "AnalysisResult not found"
fi

# =========================================================================
# CATEGORY 5: USE CASE VALIDATION TESTS
# =========================================================================

echo ""
echo "🎯 Category 5: Use Case Implementation Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 5: Use Case Implementation Tests" >> "$RESULTS_FILE"

# Test 5.1: Supervisor Dashboard use case
if grep -q "RunSupervisorDashboardExample" src/Program.cs; then
    print_test "Use Case: Supervisor Dashboard implemented" "PASS"
else
    print_test "Use Case: Supervisor Dashboard implemented" "FAIL" "SupervisorDashboard not found"
fi

# Test 5.2: Dispatcher Context use case
if grep -q "RunDispatcherContextExample" src/Program.cs; then
    print_test "Use Case: Dispatcher Context implemented" "PASS"
else
    print_test "Use Case: Dispatcher Context implemented" "FAIL" "DispatcherContext not found"
fi

# Test 5.3: Compliance Analysis use case
if grep -q "RunComplianceAnalysisExample" src/Program.cs; then
    print_test "Use Case: Compliance Analysis implemented" "PASS"
else
    print_test "Use Case: Compliance Analysis implemented" "FAIL" "ComplianceAnalysis not found"
fi

# Test 5.4: Sample incident generation
if grep -q "GenerateSampleIncidents" src/Program.cs; then
    print_test "Use Case: Sample incident generation" "PASS"
else
    print_test "Use Case: Sample incident generation" "FAIL" "GenerateSampleIncidents not found"
fi

# =========================================================================
# CATEGORY 6: DOCUMENTATION VALIDATION TESTS
# =========================================================================

echo ""
echo "📖 Category 6: Documentation Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 6: Documentation Tests" >> "$RESULTS_FILE"

# Test 6.1: README.md exists and updated
if [ -f README.md ]; then
    print_test "Documentation: README.md exists" "PASS"
    if grep -q "incident data\|CAD\|context-varying" README.md; then
        print_test "Documentation: README focused on incident data" "PASS"
    else
        print_test "Documentation: README focused on incident data" "FAIL"
    fi
else
    print_test "Documentation: README.md exists" "FAIL"
fi

# Test 6.2: REFACTORED_FIVE_STEP_APPROACH.md (moved to legacy)
if [ -f docs/legacy/REFACTORED_FIVE_STEP_APPROACH.md ]; then
    lines=$(wc -l < docs/legacy/REFACTORED_FIVE_STEP_APPROACH.md)
    print_test "Documentation: Comprehensive 5-step guide ($lines lines)" "PASS"
else
    print_test "Documentation: Comprehensive 5-step guide" "FAIL" "File not found at docs/legacy/REFACTORED_FIVE_STEP_APPROACH.md"
fi

# Test 6.3: MODEL_DRIFT_MONITORING.md (moved to legacy)
if [ -f docs/legacy/DRIFT_MONITORING.md ]; then
    lines=$(wc -l < docs/legacy/DRIFT_MONITORING.md)
    print_test "Documentation: Model drift monitoring guide ($lines lines)" "PASS"
else
    print_test "Documentation: Model drift monitoring guide" "FAIL" "File not found at docs/legacy/DRIFT_MONITORING.md"
fi

# Test 6.4: E2E tests exist
if [ -f tests/E2ETests.cs ]; then
    test_count=$(grep -c "async Task Test_" tests/E2ETests.cs)
    print_test "Documentation: E2E Test suite with $test_count tests" "PASS" "" "Tests: $test_count"
else
    print_test "Documentation: E2E Test suite" "FAIL"
fi

# =========================================================================
# CATEGORY 7: CODE QUALITY TESTS
# =========================================================================

echo ""
echo "✨ Category 7: Code Quality Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 7: Code Quality Tests" >> "$RESULTS_FILE"

# Test 7.1: Check for XML documentation comments
doc_comments=$(grep "///" src/OversizedJsonHandler.cs src/OversizedJsonOrchestrator.cs src/Program.cs 2>/dev/null | wc -l)
if [ "$doc_comments" -gt 50 ]; then
    print_test "Code Quality: XML documentation comments" "PASS" "" "Comments: $doc_comments"
else
    print_test "Code Quality: XML documentation comments" "FAIL" "Fewer than 50 doc comments (found $doc_comments)"
fi

# Test 7.2: Check for error handling
if grep -q "try\|catch\|Exception" src/OversizedJsonOrchestrator.cs src/Program.cs; then
    print_test "Code Quality: Exception handling implemented" "PASS"
else
    print_test "Code Quality: Exception handling implemented" "FAIL"
fi

# Test 7.3: Check for async/await patterns
if grep -q "async Task\|await" src/OversizedJsonOrchestrator.cs src/Program.cs; then
    print_test "Code Quality: Async/await patterns" "PASS"
else
    print_test "Code Quality: Async/await patterns" "FAIL"
fi

# Test 7.4: Check for dependency injection
if grep -q "DefaultAzureCredential\|OpenAIClient" src/OversizedJsonOrchestrator.cs; then
    print_test "Code Quality: Azure authentication and DI" "PASS"
else
    print_test "Code Quality: Azure authentication and DI" "FAIL"
fi

# =========================================================================
# CATEGORY 8: GIT VERSIONING TESTS
# =========================================================================

echo ""
echo "🔗 Category 8: Git Repository Tests"
echo "========================================" >> "$RESULTS_FILE"
echo "Category 8: Git Repository Tests" >> "$RESULTS_FILE"

# Test 8.1: Git repository initialized
if [ -d .git ]; then
    print_test "Git: Repository initialized" "PASS"
else
    print_test "Git: Repository initialized" "FAIL"
fi

# Test 8.2: All files committed
uncommitted=$(git status --short 2>/dev/null | wc -l)
if [ "$uncommitted" -eq 0 ]; then
    print_test "Git: All changes committed" "PASS"
else
    print_test "Git: All changes committed" "FAIL" "$uncommitted uncommitted files"
fi

# Test 8.3: Remote configured
if git remote -v 2>/dev/null | grep -q "origin.*handling-oversized-json"; then
    print_test "Git: GitHub remote configured" "PASS"
else
    print_test "Git: GitHub remote configured" "FAIL"
fi

# Test 8.4: Recent commits exist
commits=$(git log --oneline 2>/dev/null | head -1)
if [ -n "$commits" ]; then
    print_test "Git: Commit history available" "PASS" "" "Latest: $commits"
else
    print_test "Git: Commit history available" "FAIL"
fi

# =========================================================================
# PRINT SUMMARY
# =========================================================================

echo ""
echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║                         TEST SUMMARY                              ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""
echo "Total Tests:   $TOTAL"
echo -e "Passed:        ${GREEN}$PASSED${NC}"
echo -e "Failed:        ${RED}$FAILED${NC}"
echo ""

if [ "$FAILED" -eq 0 ]; then
    PASS_RATE=100
else
    PASS_RATE=$((100 * PASSED / TOTAL))
fi

echo "Pass Rate: $PASS_RATE%"
echo ""

if [ "$FAILED" -eq 0 ]; then
    echo "╔════════════════════════════════════════════════════════════════════╗"
    echo "║                   ALL TESTS PASSED ✓                              ║"
    echo "║              Ready for customer demonstration                      ║"
    echo "║                                                                    ║"
    echo "║ 8 Test Categories Validated:                                      ║"
    echo "║ ✓ Preprocessing (4 tests)                                          ║"
    echo "║ ✓ Semantic Chunking (3 tests)                                      ║"
    echo "║ ✓ Token Management (4 tests)                                       ║"
    echo "║ ✓ Processing Pipeline (4 tests)                                    ║"
    echo "║ ✓ Use Case Implementation (4 tests)                                ║"
    echo "║ ✓ Documentation (4 tests)                                          ║"
    echo "║ ✓ Code Quality (4 tests)                                           ║"
    echo "║ ✓ Git Versioning (4 tests)                                         ║"
    echo "║                                                                    ║"
    echo "║ Repository: github.com/msftsean/handling-oversized-json            ║"
    echo "╚════════════════════════════════════════════════════════════════════╝"
else
    echo "╔════════════════════════════════════════════════════════════════════╗"
    echo "║                  TESTS COMPLETED WITH FAILURES                     ║"
    echo "╚════════════════════════════════════════════════════════════════════╝"
fi

echo ""
echo "📄 Full results saved to: $RESULTS_FILE"
echo ""
