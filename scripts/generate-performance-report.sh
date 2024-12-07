#!/bin/bash

# Performance Report Generator for EMRNext

# Create output directory
mkdir -p ./performance-reports

# Run BenchmarkDotNet
dotnet run -c Release --project tests/EMRNext.PerformanceTests \
    --benchmark-summary ./performance-reports/benchmark-summary.md

# Generate Load Testing Report
artillery run \
    --output ./performance-reports/load-test-report.json \
    ./load-testing-config.yml

# Convert Artillery JSON to Markdown
artillery report \
    ./performance-reports/load-test-report.json \
    > ./performance-reports/load-test-report.md

# Combine Reports
echo "# EMRNext Performance Report" > ./performance-reports/comprehensive-report.md
echo "## Benchmark Summary" >> ./performance-reports/comprehensive-report.md
cat ./performance-reports/benchmark-summary.md >> ./performance-reports/comprehensive-report.md
echo -e "\n## Load Testing Results" >> ./performance-reports/comprehensive-report.md
cat ./performance-reports/load-test-report.md >> ./performance-reports/comprehensive-report.md

# Display report
cat ./performance-reports/comprehensive-report.md
