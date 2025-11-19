#!/bin/bash

# GitHub Push Script for handling-oversized-json
# Run this script to push all files to your GitHub repo

set -e

GITHUB_REPO="https://github.com/msftsean/handling-oversized-json.git"
GITHUB_USER="msftsean"
SOURCE_DIR="/mnt/user-data/outputs"

echo "========================================================================"
echo "Pushing to GitHub: msftsean/handling-oversized-json"
echo "========================================================================"
echo ""

# Check if git is installed
if ! command -v git &> /dev/null; then
    echo "❌ git is not installed. Please install git first."
    exit 1
fi

# Check if we have the source files
if [ ! -d "$SOURCE_DIR" ]; then
    echo "❌ Source directory not found: $SOURCE_DIR"
    exit 1
fi

# Check for required files
REQUIRED_FILES=(
    "README.md"
    "QUICKSTART.md"
    "FIVE_STEP_APPROACH.md"
    "MODEL_DRIFT_MITIGATION_GUIDE.md"
    "OversizedJsonHandler.cs"
    "OversizedJsonOrchestrator.cs"
    "Program.cs"
    "OversizedJsonHandler.csproj"
)

echo "Checking for required files..."
for file in "${REQUIRED_FILES[@]}"; do
    if [ ! -f "$SOURCE_DIR/$file" ]; then
        echo "❌ Missing: $file"
        exit 1
    fi
    echo "  ✓ $file"
done
echo ""

# Create temporary git directory
TEMP_REPO=$(mktemp -d)
echo "Using temporary directory: $TEMP_REPO"
cd "$TEMP_REPO"

# Initialize git repo
echo ""
echo "Initializing git repository..."
git init
git config user.name "GitHub Automation"
git config user.email "automation@github.com"

# Copy all files from source
echo "Copying files..."
cp -r "$SOURCE_DIR"/* .

# Add all files
echo "Adding files to git..."
git add .

# Create commit
echo "Creating commit..."
git commit -m "Add C# implementation of 5-step JSON handling approach

- Refactored from Python to C# for Microsoft Foundry
- Generic Contoso branding (no entity-specific references)
- Production-ready implementation with error handling
- Comprehensive 5-step approach documentation
- Model drift mitigation guide for production monitoring
- Quick start guide for immediate adoption
- Real-world results: 98.8% payload reduction"

# Create/update branch
echo "Setting branch to main..."
git branch -M main

# Add remote
echo "Adding GitHub remote..."
git remote add origin "$GITHUB_REPO"

# Push to GitHub
echo ""
echo "========================================================================"
echo "Ready to push! You have two options:"
echo "========================================================================"
echo ""
echo "OPTION 1: Use HTTPS (requires GitHub token)"
echo "  git push -u origin main"
echo ""
echo "OPTION 2: Use SSH (requires SSH key setup)"
echo "  git remote set-url origin git@github.com:msftsean/handling-oversized-json.git"
echo "  git push -u origin main"
echo ""
echo "IMPORTANT: You must authenticate with GitHub first!"
echo "  - GitHub token: https://github.com/settings/tokens"
echo "  - SSH keys: https://github.com/settings/keys"
echo ""
echo "Continuing with HTTPS (using stored credentials or prompting)..."
echo ""

# Attempt push
if git push -u origin main 2>&1; then
    echo ""
    echo "========================================================================"
    echo "✅ SUCCESS! Files pushed to GitHub"
    echo "========================================================================"
    echo ""
    echo "Repository: https://github.com/msftsean/handling-oversized-json"
    echo ""
    echo "Next steps:"
    echo "1. Visit https://github.com/msftsean/handling-oversized-json"
    echo "2. Share the repo link with your customer"
    echo "3. They should start with QUICKSTART.md"
    echo ""
else
    echo ""
    echo "========================================================================"
    echo "⚠️  Push failed. Likely authentication issue."
    echo "========================================================================"
    echo ""
    echo "Troubleshooting:"
    echo "1. Check your GitHub credentials"
    echo "2. Ensure you have push access to the repository"
    echo "3. Try manually with one of these commands:"
    echo ""
    echo "   HTTPS:"
    echo "   cd $TEMP_REPO"
    echo "   git push -u origin main"
    echo ""
    echo "   SSH:"
    echo "   git remote set-url origin git@github.com:msftsean/handling-oversized-json.git"
    echo "   git push -u origin main"
    echo ""
    exit 1
fi

# Cleanup
cd /
rm -rf "$TEMP_REPO"

echo "All done! 🎉"
