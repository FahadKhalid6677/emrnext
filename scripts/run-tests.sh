#!/bin/bash

# Exit on first error
set -e

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Test runner script for EMRNext
echo "🚀 Starting Comprehensive Test Suite for EMRNext"

# Unit Tests
echo -e "${GREEN}Running Unit Tests...${NC}"
dotnet test tests/EMRNext.UnitTests

# Integration Tests
echo -e "${GREEN}Running Integration Tests...${NC}"
dotnet test tests/EMRNext.IntegrationTests

# Performance Tests
echo -e "${GREEN}Running Performance Tests...${NC}"
dotnet run --project tests/EMRNext.PerformanceTests

# Security Scan
echo -e "${GREEN}Running Security Vulnerability Scan...${NC}"
dotnet tool install -g security-scan
security-scan src/

# Generate Test Report
echo -e "${GREEN}Generating Test Report...${NC}"
dotnet test --logger:"html;LogFileName=TestReport.html"

echo -e "${GREEN}✅ All Tests Completed Successfully!${NC}"
