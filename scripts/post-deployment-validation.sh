#!/bin/bash

# Post-Deployment Validation Script for EMRNext

# Color Codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Validation Configuration
DEPLOYMENT_URL="https://emrnext.railway.app"
MAX_RETRIES=3
RETRY_DELAY=10

# Logging
LOG_FILE="/var/log/emrnext/post_deployment_validation.log"
mkdir -p "$(dirname "$LOG_FILE")"

log_message() {
    local message="$1"
    local status="$2"
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $status: $message" | tee -a "$LOG_FILE"
}

# Health Check Functions
check_backend_health() {
    local retry=0
    while [ $retry -lt $MAX_RETRIES ]; do
        local response=$(curl -s -o /dev/null -w "%{http_code}" "$DEPLOYMENT_URL/api/health")
        if [ "$response" == "200" ]; then
            log_message "Backend Health Check Passed" "${GREEN}SUCCESS${NC}"
            return 0
        fi
        
        log_message "Backend Health Check Failed (Attempt $((retry+1)))" "${YELLOW}WARNING${NC}"
        sleep $RETRY_DELAY
        ((retry++))
    done
    
    log_message "Backend Health Check Failed After $MAX_RETRIES Attempts" "${RED}CRITICAL${NC}"
    return 1
}

check_frontend_availability() {
    local retry=0
    while [ $retry -lt $MAX_RETRIES ]; do
        local response=$(curl -s -o /dev/null -w "%{http_code}" "$DEPLOYMENT_URL")
        if [ "$response" == "200" ]; then
            log_message "Frontend Availability Check Passed" "${GREEN}SUCCESS${NC}"
            return 0
        fi
        
        log_message "Frontend Availability Check Failed (Attempt $((retry+1)))" "${YELLOW}WARNING${NC}"
        sleep $RETRY_DELAY
        ((retry++))
    done
    
    log_message "Frontend Availability Check Failed After $MAX_RETRIES Attempts" "${RED}CRITICAL${NC}"
    return 1
}

run_smoke_tests() {
    log_message "Running Smoke Tests" "${YELLOW}PROGRESS${NC}"
    
    # Sample smoke test scenarios
    local test_scenarios=(
        "/api/auth/validate-token"
        "/api/patients/count"
        "/api/appointments/today"
    )
    
    for scenario in "${test_scenarios[@]}"; do
        local response=$(curl -s -o /dev/null -w "%{http_code}" "$DEPLOYMENT_URL$scenario")
        if [ "$response" != "200" ]; then
            log_message "Smoke Test Failed for $scenario" "${RED}CRITICAL${NC}"
            return 1
        fi
    done
    
    log_message "All Smoke Tests Passed" "${GREEN}SUCCESS${NC}"
    return 0
}

# Main Validation Function
main() {
    log_message "Starting Post-Deployment Validation" "${YELLOW}PROGRESS${NC}"
    
    check_backend_health || exit 1
    check_frontend_availability || exit 1
    run_smoke_tests || exit 1
    
    # Run log analyzer
    python3 /Users/fahadkhalid/EMRNext/scripts/deployment-log-analyzer.py
    
    log_message "Post-Deployment Validation Completed Successfully" "${GREEN}SUCCESS${NC}"
}

# Execute Main Validation
main
