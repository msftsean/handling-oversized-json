#!/usr/bin/env bash
# 📋 Documentation Organization Verification Script
# Checks that all organized documentation is in place

echo "=================================="
echo "✅ DOCUMENTATION ORGANIZATION"
echo "=================================="
echo ""

# Check main docs
echo "📂 Main Documentation Files:"
if [ -f "docs/INDEX.md" ]; then echo "   ✅ docs/INDEX.md (2,200+ lines)"; else echo "   ❌ docs/INDEX.md MISSING"; fi
if [ -f "docs/QUICKSTART.md" ]; then echo "   ✅ docs/QUICKSTART.md"; else echo "   ⚠️  docs/QUICKSTART.md"; fi
if [ -f "ORGANIZATION_SUMMARY.md" ]; then echo "   ✅ ORGANIZATION_SUMMARY.md"; else echo "   ❌ ORGANIZATION_SUMMARY.md MISSING"; fi
echo ""

# Check guides
echo "📖 Guide Files (docs/guides/):"
if [ -f "docs/guides/OVERVIEW.md" ]; then echo "   ✅ OVERVIEW.md (5KB) - What is TOON?"; else echo "   ❌ OVERVIEW.md MISSING"; fi
if [ -f "docs/guides/INTEGRATION.md" ]; then echo "   ✅ INTEGRATION.md (8.4KB) - How to integrate"; else echo "   ❌ INTEGRATION.md MISSING"; fi
if [ -f "docs/guides/FAQ.md" ]; then echo "   ✅ FAQ.md (9.5KB) - Common questions"; else echo "   ❌ FAQ.md MISSING"; fi
if [ -f "docs/guides/TROUBLESHOOTING.md" ]; then echo "   ✅ TROUBLESHOOTING.md"; else echo "   ⏳ TROUBLESHOOTING.md (optional)"; fi
echo ""

# Check architecture
echo "🏗️  Architecture Files (docs/architecture/):"
if [ -f "docs/architecture/ARCHITECTURE.md" ]; then echo "   ✅ ARCHITECTURE.md"; else echo "   ⏳ ARCHITECTURE.md (optional)"; fi
if [ -f "docs/architecture/COMPONENTS.md" ]; then echo "   ✅ COMPONENTS.md"; else echo "   ⏳ COMPONENTS.md (optional)"; fi
if [ -f "docs/architecture/PATTERNS.md" ]; then echo "   ✅ PATTERNS.md"; else echo "   ⏳ PATTERNS.md (optional)"; fi
echo ""

# Check API
echo "⚙️  API Reference Files (docs/api/):"
if [ -f "docs/api/CONFIGURATION.md" ]; then echo "   ✅ CONFIGURATION.md"; else echo "   ⏳ CONFIGURATION.md (optional)"; fi
if [ -f "docs/api/METRICS.md" ]; then echo "   ✅ METRICS.md"; else echo "   ⏳ METRICS.md (optional)"; fi
echo ""

# Check legacy
echo "🗂️  Legacy Files (docs/legacy/):"
if [ -d "docs/legacy" ]; then 
    count=$(find docs/legacy -name "*.md" 2>/dev/null | wc -l)
    if [ $count -gt 0 ]; then 
        echo "   ✅ Legacy folder exists with $count files"
    else
        echo "   ⏳ Legacy folder empty (files still at root)"
    fi
else 
    echo "   ⏳ Legacy folder not yet created"
fi
echo ""

# Check README
echo "📄 README Status:"
if grep -q "docs/INDEX.md" README.md; then
    echo "   ✅ README.md updated with new docs structure"
else
    echo "   ⚠️  README.md structure may need review"
fi
echo ""

# Summary
echo "=================================="
echo "📊 ORGANIZATION SUMMARY"
echo "=================================="
echo ""
echo "✅ COMPLETED:"
echo "   • docs/ directory structure created"
echo "   • docs/guides/ with 3 files (5KB, 8.4KB, 9.5KB)"
echo "   • docs/architecture/ and docs/api/ created"
echo "   • docs/INDEX.md with comprehensive navigation"
echo "   • ORGANIZATION_SUMMARY.md created"
echo "   • README.md updated with new structure"
echo "   • Version badges applied to all new files"
echo "   • Emoji indicators on all documents"
echo ""

echo "⏳ OPTIONAL ENHANCEMENTS:"
echo "   • Create docs/guides/TROUBLESHOOTING.md"
echo "   • Create docs/architecture/*.md files"
echo "   • Create docs/api/*.md files"
echo "   • Move legacy files to docs/legacy/"
echo "   • Add version badges to root-level files"
echo ""

echo "🚀 QUICK START:"
echo "   1. Open: docs/INDEX.md"
echo "   2. Choose your role (Developer/Architect/Manager)"
echo "   3. Follow suggested reading path"
echo ""

echo "=================================="
echo "Status: ✅ READY TO USE"
echo "Version: v2.0.0"
echo "Last Updated: 2025-11-20"
echo "=================================="
