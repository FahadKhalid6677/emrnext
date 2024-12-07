#!/bin/bash

# Final Deployment Orchestration Script for EMRNext

# Deployment Stages Logging
log_stage() {
    local stage="$1"
    local status="$2"
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $stage - $status" >> /var/log/emrnext/deployment_stages.log
}

# Comprehensive Deployment Workflow
main_deployment() {
    # Stage 1: Pre-Deployment Validation
    log_stage "Pre-Deployment Validation" "Starting"
    ./scripts/system-validation.sh
    if [ $? -ne 0 ]; then
        log_stage "Pre-Deployment Validation" "Failed"
        exit 1
    fi
    log_stage "Pre-Deployment Validation" "Completed"

    # Stage 2: Build and Push Docker Images
    log_stage "Docker Image Build" "Starting"
    docker build -t emrnext-backend:latest -f Dockerfile.backend .
    docker build -t emrnext-frontend:latest -f Dockerfile.frontend .
    railway login
    railway deploy
    if [ $? -ne 0 ]; then
        log_stage "Docker Image Build" "Failed"
        exit 1
    fi
    log_stage "Docker Image Build" "Completed"

    # Stage 3: Database Migration
    log_stage "Database Migration" "Starting"
    railway run dotnet ef database update
    if [ $? -ne 0 ]; then
        log_stage "Database Migration" "Failed"
        exit 1
    fi
    log_stage "Database Migration" "Completed"

    # Stage 4: Post-Deployment Validation
    log_stage "Post-Deployment Validation" "Starting"
    ./scripts/post-deployment-validation.sh
    if [ $? -ne 0 ]; then
        log_stage "Post-Deployment Validation" "Failed"
        exit 1
    fi
    log_stage "Post-Deployment Validation" "Completed"

    # Stage 5: Generate Deployment Report
    log_stage "Deployment Report Generation" "Starting"
    python3 ./scripts/deployment-readiness-report.py
    python3 ./scripts/deployment-log-analyzer.py
    log_stage "Deployment Report Generation" "Completed"

    # Final Success
    log_stage "Deployment" "SUCCESSFUL"
    echo "EMRNext Deployment Completed Successfully!"
}

# Error Handling
trap 'echo "Deployment Failed at $(date)"; exit 1' ERR

# Execute Main Deployment
main_deployment
