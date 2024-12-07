#!/bin/bash

# EMRNext Pre-Deployment Validation Script

set -e

echo "🚦 Pre-Deployment Validation Starting"
echo "===================================="

# Validate .NET Project
echo -n "🔧 Checking .NET Project... "
dotnet restore && dotnet build && echo "✅ Build Successful" || {
    echo "❌ .NET Project Build Failed"
    exit 1
}

# Validate Frontend
echo -n "🌐 Checking Frontend... "
cd src/EMRNext.Web
npm install && npm run build && echo "✅ Frontend Build Successful" || {
    echo "❌ Frontend Build Failed"
    exit 1
}
cd ../..

# Run Unit Tests
echo "🧪 Running Unit Tests:"
dotnet test || {
    echo "❌ Unit Tests Failed"
    exit 1
}

# Security Scan
echo -n "🔒 Running Security Scan... "
dotnet tool install -g security-scan &> /dev/null
security-scan . || {
    echo "⚠️ Security Scan Detected Potential Issues"
}

# Performance Profiling
echo -n "📊 Running Performance Check... "
dotnet tool install -g dotnet-trace &> /dev/null
dotnet trace collect --providers Microsoft-Windows-DotNetRuntime || {
    echo "⚠️ Performance Profiling Encountered Issues"
}

echo ""
echo "🎉 Pre-Deployment Validation Complete!"
echo "EMRNext is ready for Railway deployment!"
exit 0
