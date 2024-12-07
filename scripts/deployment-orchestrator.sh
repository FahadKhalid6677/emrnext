#!/bin/bash

# EMRNext Deployment Orchestrator
# Comprehensive deployment management script

set -e

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Deployment Stages Logging
log_stage() {
    echo -e "${YELLOW}[STAGE] $1${NC}"
}

# Success Logging
log_success() {
    echo -e "${GREEN}[SUCCESS] $1${NC}"
}

# Error Logging
log_error() {
    echo -e "${RED}[ERROR] $1${NC}"
    exit 1
}

# Deployment Main Function
deploy_emrnext() {
    # Stage 1: Pre-Deployment Validation
    log_stage "Pre-Deployment Validation"
    ./scripts/pre-deployment-validation.sh || log_error "Pre-deployment validation failed"
    log_success "Pre-deployment validation complete"

    # Stage 2: Railway Deployment Readiness Check
    log_stage "Deployment Readiness Check"
    ./scripts/deployment-readiness-check.sh || log_error "Deployment readiness check failed"
    log_success "Deployment readiness verified"

    # Stage 3: Railway Project Deployment
    log_stage "Railway Project Deployment"
    railway up || log_error "Railway deployment failed"
    log_success "Railway deployment initiated"

    # Stage 4: Deployment Verification
    log_stage "Deployment Verification"
    verify_deployment || log_error "Deployment verification failed"
    log_success "Deployment successfully verified"

    # Stage 5: Post-Deployment Monitoring Setup
    log_stage "Post-Deployment Monitoring"
    setup_monitoring || log_error "Monitoring setup failed"
    log_success "Monitoring configured"
}

# Deployment Verification Function
verify_deployment() {
    local FRONTEND_URL=$(railway open frontend)
    local BACKEND_URL=$(railway open backend)

    # Check Frontend Accessibility
    curl -f "$FRONTEND_URL" || log_error "Frontend not accessible"

    # Check Backend API
    curl -f "$BACKEND_URL/health" || log_error "Backend health check failed"

    # Additional Verification Steps
    railway logs frontend
    railway logs backend
}

# Monitoring Setup Function
setup_monitoring() {
    # Configure Railway monitoring
    railway monitor set \
        --cpu-threshold 80 \
        --memory-threshold 85 \
        --error-tracking true \
        --performance-monitoring true
}

# Error Handling
trap 'log_error "Deployment failed at stage: $BASH_COMMAND"' ERR

# Main Execution
main() {
    echo -e "${GREEN}🚀 EMRNext Deployment Orchestrator${NC}"
    echo "Starting comprehensive deployment process..."
    
    deploy_emrnext
    
    echo -e "${GREEN}🎉 Deployment Completed Successfully!${NC}"
}

# Run Main Function
main
