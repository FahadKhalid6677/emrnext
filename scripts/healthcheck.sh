#!/bin/bash

# Exit on error
set -e

# Configuration
API_URL="https://localhost:5001"
TIMEOUT=5
MAX_RETRIES=3

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "Starting health check..."

# Function to check endpoint
check_endpoint() {
    local endpoint=$1
    local description=$2
    local retry=0

    while [ $retry -lt $MAX_RETRIES ]; do
        if curl -f -s -k "${API_URL}${endpoint}" -m $TIMEOUT > /dev/null; then
            echo -e "${GREEN}✓${NC} $description"
            return 0
        else
            retry=$((retry+1))
            if [ $retry -lt $MAX_RETRIES ]; then
                echo -e "${YELLOW}⚠ Retrying $description (attempt $retry of $MAX_RETRIES)${NC}"
                sleep 2
            fi
        fi
    done

    echo -e "${RED}✗${NC} $description failed after $MAX_RETRIES attempts"
    return 1
}

# Check core services
echo "Checking core services..."
check_endpoint "/health" "API Health Check" || exit 1
check_endpoint "/health/database" "Database Connection" || exit 1
check_endpoint "/health/redis" "Redis Connection" || exit 1

# Check business services
echo "Checking business services..."
check_endpoint "/api/clinical/health" "Clinical Service" || exit 1
check_endpoint "/api/scheduling/health" "Scheduling Service" || exit 1
check_endpoint "/api/billing/health" "Billing Service" || exit 1

# Check monitoring
echo "Checking monitoring services..."
check_endpoint "/metrics" "Metrics Endpoint" || exit 1
check_endpoint "/health/external" "External Services" || exit 1

# Check system resources
echo "Checking system resources..."
docker stats --no-stream || exit 1

echo -e "${GREEN}All health checks passed!${NC}"
