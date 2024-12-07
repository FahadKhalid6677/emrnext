#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "Starting monitoring infrastructure validation..."

# Check Prometheus connectivity
echo -n "Checking Prometheus connectivity... "
if curl -s "http://localhost:9090/-/healthy" > /dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
    echo "Error: Cannot connect to Prometheus"
    exit 1
fi

# Check Grafana connectivity
echo -n "Checking Grafana connectivity... "
if curl -s "http://localhost:3000/api/health" > /dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
    echo "Error: Cannot connect to Grafana"
    exit 1
fi

# Validate dashboard configurations
echo -n "Validating dashboard configurations... "
if [ -f "../monitoring/dashboards/system-metrics.json" ] && [ -f "../monitoring/dashboards/business-metrics.json" ]; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
    echo "Error: Dashboard configuration files not found"
    exit 1
fi

# Validate alert rules
echo -n "Validating alert rules... "
if [ -f "../monitoring/alert-rules.yml" ]; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
    echo "Error: Alert rules file not found"
    exit 1
fi

# Check metrics collection
echo "Testing metrics collection..."
metrics=(
    "process_cpu_seconds_total"
    "process_resident_memory_bytes"
    "http_request_duration_milliseconds"
    "appointment_queue_depth"
    "claims_processed_total"
)

for metric in "${metrics[@]}"; do
    echo -n "Checking metric $metric... "
    if curl -s "http://localhost:9090/api/v1/query?query=$metric" | grep -q "success"; then
        echo -e "${GREEN}OK${NC}"
    else
        echo -e "${YELLOW}WARNING${NC}"
        echo "Warning: Metric $metric not found"
    fi
done

# Validate notification channels
echo -n "Validating notification channels... "
if curl -s "http://localhost:3000/api/alert-notifications" -H "Authorization: Bearer $GRAFANA_API_KEY" > /dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${YELLOW}WARNING${NC}"
    echo "Warning: Could not validate notification channels"
fi

echo "Validation complete!"
