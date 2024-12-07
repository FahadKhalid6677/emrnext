#!/bin/bash

# System-Wide Validation Script for EMRNext

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Validation Stages
echo -e "${YELLOW}🔍 Starting Comprehensive System Validation ${NC}"

# 1. Dependency Check
echo -e "${YELLOW}Checking System Dependencies...${NC}"
./scripts/check-dependencies.sh
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Dependency Check Failed${NC}"
    exit 1
fi

# 2. Database Integrity Check
echo -e "${YELLOW}Verifying Database Integrity...${NC}"
dotnet run --project src/EMRNext.Core/DatabaseIntegrityCheck
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Database Integrity Check Failed${NC}"
    exit 1
fi

# 3. Full Test Suite
echo -e "${YELLOW}Running Comprehensive Test Suite...${NC}"
./scripts/run-tests.sh
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Test Suite Validation Failed${NC}"
    exit 1
fi

# 4. Performance Benchmark
echo -e "${YELLOW}Running Performance Benchmarks...${NC}"
./scripts/generate-performance-report.sh
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Performance Benchmarking Failed${NC}"
    exit 1
fi

# 5. Security Scan
echo -e "${YELLOW}Performing Security Vulnerability Scan...${NC}"
dotnet tool run security-scan src/
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Security Scan Failed${NC}"
    exit 1
fi

# 6. Configuration Validation
echo -e "${YELLOW}Validating Environment Configurations...${NC}"
dotnet run --project src/EMRNext.Core/ConfigurationValidator
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Configuration Validation Failed${NC}"
    exit 1
fi

# Final Success
echo -e "${GREEN}✅ System Validation Completed Successfully!${NC}"
exit 0
