#!/bin/bash

# Simple push script for VS Code with existing GitHub authentication
# No PAT needed - uses your existing VS Code authentication

set -e

echo "========================================================================"
echo "Pushing to GitHub: msftsean/handling-oversized-json"
echo "========================================================================"
echo ""

# Check if we're in a git repo
if [ ! -d ".git" ]; then
    echo "❌ This script must be run from the root of the git repository"
    echo ""
    echo "Usage:"
    echo "  1. Extract outputs.zip"
    echo "  2. cd outputs"
    echo "  3. bash push-to-github.sh"
    exit 1
fi

echo "✓ Git repository detected"
echo ""

# Show current status
echo "Current branch:"
git branch

echo ""
echo "Remote:"
git remote -v

echo ""
echo "========================================================================"
echo "Setting up GitHub remote..."
echo "========================================================================"
echo ""

# Update or add the remote (using HTTPS without credentials - VS Code will handle auth)
git remote remove origin 2>/dev/null || true
git remote add origin https://github.com/msftsean/handling-oversized-json.git

echo "✓ Remote set to: https://github.com/msftsean/handling-oversized-json.git"
echo ""

# Show what we're about to push
echo "========================================================================"
echo "Ready to push:"
echo "========================================================================"
echo ""
git log --oneline -5
echo ""
echo "Branch: $(git rev-parse --abbrev-ref HEAD)"
echo ""

# Push to GitHub
echo "========================================================================"
echo "Pushing to GitHub..."
echo "========================================================================"
echo ""

if git push -u origin main; then
    echo ""
    echo "========================================================================"
    echo "✅ SUCCESS! Repository pushed to GitHub"
    echo "========================================================================"
    echo ""
    echo "Your repository is now live:"
    echo "🔗 https://github.com/msftsean/handling-oversized-json"
    echo ""
    echo "Next steps:"
    echo "1. Visit the repository on GitHub"
    echo "2. Share the link with your customer"
    echo "3. Customer should start with QUICKSTART.md"
    echo ""
else
    echo ""
    echo "========================================================================"
    echo "❌ Push failed"
    echo "========================================================================"
    echo ""
    echo "Troubleshooting:"
    echo "1. Make sure you're authenticated with GitHub in VS Code"
    echo "2. Check that the remote URL is correct"
    echo "3. Try pushing manually:"
    echo ""
    echo "   git push -u origin main"
    echo ""
    exit 1
fi

echo "All done! 🎉"
