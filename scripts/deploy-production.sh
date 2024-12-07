#!/bin/bash

# Production Deployment Script for EMRNext

# Color Codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Deployment Stages Logging
log_stage() {
    echo -e "${YELLOW}[DEPLOY] $1${NC}"
}

# Error Handling
handle_error() {
    echo -e "${RED}❌ Error: $1${NC}"
    # Trigger rollback or alert mechanism
    ./scripts/rollback.sh
    exit 1
}

# Pre-Deployment Validation
log_stage "Running Pre-Deployment System Validation"
./scripts/system-validation.sh || handle_error "System Validation Failed"

# Build Docker Images
log_stage "Building Production Docker Images"
docker build -t emrnext-backend:prod -f Dockerfile.backend .
docker build -t emrnext-frontend:prod -f Dockerfile.frontend .

# Push to Container Registry
log_stage "Pushing Images to Container Registry"
railway login
railway deploy || handle_error "Railway Deployment Failed"

# Database Migration
log_stage "Running Database Migrations"
railway run dotnet ef database update || handle_error "Database Migration Failed"

# Warm-up and Health Check
log_stage "Performing Deployment Health Checks"
./scripts/deployment-health-check.sh || handle_error "Deployment Health Check Failed"

# Final Success Notification
echo -e "${GREEN}✅ EMRNext Deployment Completed Successfully!${NC}"

# Start Continuous Monitoring
log_stage "Initiating Continuous Monitoring"
./scripts/continuous-monitoring.sh &

exit 0
