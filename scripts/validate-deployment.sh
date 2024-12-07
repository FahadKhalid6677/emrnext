#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_file="validation_results.log"
error_file="validation_errors.log"

# Function to log messages
log_message() {
    echo -e "$1" | tee -a "$log_file"
}

# Function to log errors
log_error() {
    echo -e "${RED}ERROR: $1${NC}" | tee -a "$error_file"
}

# Function to check service health
check_service() {
    local service=$1
    local url=$2
    log_message "\nChecking $service health..."
    
    if curl -s "$url" > /dev/null; then
        log_message "${GREEN}✓ $service is healthy${NC}"
        return 0
    else
        log_error "$service is not responding"
        return 1
    fi
}

# Clear previous logs
> "$log_file"
> "$error_file"

log_message "Starting EMRNext Monitoring Validation\n================================="
log_message "Timestamp: $(date)"

# Step 1: Infrastructure Tests
log_message "\n1. Infrastructure Validation"
log_message "-------------------------"

# Check Docker
if command -v docker >/dev/null 2>&1; then
    log_message "${GREEN}✓ Docker installed${NC}"
else
    log_error "Docker not found"
    exit 1
fi

# Check required ports
for port in 3000 9090 9093; do
    if ! lsof -i :$port > /dev/null 2>&1; then
        log_message "${GREEN}✓ Port $port available${NC}"
    else
        log_error "Port $port is already in use"
        exit 1
    fi
done

# Check services
check_service "Prometheus" "http://localhost:9090/-/healthy"
check_service "Grafana" "http://localhost:3000/api/health"
check_service "Alertmanager" "http://localhost:9093/-/healthy"

# Step 2: Configuration Validation
log_message "\n2. Configuration Validation"
log_message "---------------------------"

config_files=(
    "../monitoring/prometheus.yml"
    "../monitoring/alertmanager.yml"
    "../monitoring/dashboards/system-metrics.json"
    "../monitoring/dashboards/business-metrics.json"
)

for file in "${config_files[@]}"; do
    if [ -f "$file" ]; then
        log_message "${GREEN}✓ Found $file${NC}"
    else
        log_error "Missing configuration file: $file"
        exit 1
    fi
done

# Step 3: Metrics Validation
log_message "\n3. Metrics Validation"
log_message "--------------------"

metrics=(
    "process_cpu_seconds_total"
    "process_resident_memory_bytes"
    "http_request_duration_milliseconds"
    "appointment_queue_depth"
    "claims_processed_total"
)

for metric in "${metrics[@]}"; do
    if curl -s "http://localhost:9090/api/v1/query?query=$metric" | grep -q "success"; then
        log_message "${GREEN}✓ Metric $metric is collecting data${NC}"
    else
        log_error "Metric $metric is not collecting data"
    fi
done

# Step 4: Alert System Validation
log_message "\n4. Alert System Validation"
log_message "-------------------------"

# Check alert rules
if curl -s "http://localhost:9090/api/v1/rules" | grep -q "groups"; then
    log_message "${GREEN}✓ Alert rules loaded${NC}"
else
    log_error "Alert rules not loaded properly"
fi

# Check notification channels
if curl -s "http://localhost:3000/api/alert-notifications" -H "Authorization: Bearer ${GRAFANA_API_KEY}" | grep -q "id"; then
    log_message "${GREEN}✓ Notification channels configured${NC}"
else
    log_error "Notification channels not configured"
fi

# Step 5: Security Validation
log_message "\n5. Security Validation"
log_message "---------------------"

# Check Grafana authentication
if curl -s "http://localhost:3000/api/auth/keys" -H "Authorization: Bearer ${GRAFANA_API_KEY}" | grep -q "id"; then
    log_message "${GREEN}✓ Grafana authentication working${NC}"
else
    log_error "Grafana authentication issues"
fi

# Check HTTPS
if curl -sk "https://localhost:3000" 2>&1 | grep -q "SSL"; then
    log_message "${GREEN}✓ HTTPS enabled${NC}"
else
    log_message "${YELLOW}! HTTPS not enabled${NC}"
fi

# Final Results
log_message "\nValidation Summary"
log_message "=================="

error_count=$(wc -l < "$error_file")
if [ $error_count -eq 0 ]; then
    log_message "${GREEN}✓ All validation tests passed successfully!${NC}"
else
    log_message "${RED}✗ Found $error_count validation errors. Check validation_errors.log for details.${NC}"
fi

log_message "\nValidation completed at $(date)"
log_message "Detailed results available in: $log_file"
if [ $error_count -gt 0 ]; then
    log_message "Error details available in: $error_file"
fi
