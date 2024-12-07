#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check prerequisites
command -v docker >/dev/null 2>&1 || { echo -e "${RED}Error: Docker is not installed${NC}" >&2; exit 1; }
command -v docker-compose >/dev/null 2>&1 || { echo -e "${RED}Error: Docker Compose is not installed${NC}" >&2; exit 1; }

# Check required environment variables
required_vars=("GRAFANA_ADMIN_PASSWORD" "SMTP_USER" "SMTP_PASSWORD" "PAGERDUTY_SERVICE_KEY")
for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        echo -e "${RED}Error: $var is not set${NC}"
        exit 1
    fi
done

echo "Starting monitoring deployment..."

# Create necessary directories
echo "Creating directories..."
mkdir -p ../monitoring/{dashboards,grafana-provisioning}

# Deploy monitoring stack
echo "Deploying monitoring stack..."
cd ../monitoring
docker-compose down
docker-compose up -d

# Wait for services to be ready
echo "Waiting for services to be ready..."
sleep 30

# Validate services
echo "Validating services..."

# Check Prometheus
echo -n "Checking Prometheus... "
if curl -s "http://localhost:9090/-/healthy" > /dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
    echo "Error: Prometheus is not healthy"
    exit 1
fi

# Check Grafana
echo -n "Checking Grafana... "
if curl -s "http://localhost:3000/api/health" > /dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
    echo "Error: Grafana is not healthy"
    exit 1
fi

# Check Alertmanager
echo -n "Checking Alertmanager... "
if curl -s "http://localhost:9093/-/healthy" > /dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAILED${NC}"
    echo "Error: Alertmanager is not healthy"
    exit 1
fi

# Import dashboards
echo "Importing dashboards..."
for dashboard in dashboards/*.json; do
    echo -n "Importing $(basename $dashboard)... "
    if curl -s -X POST -H "Content-Type: application/json" -d @$dashboard \
        "http://admin:${GRAFANA_ADMIN_PASSWORD}@localhost:3000/api/dashboards/db" > /dev/null; then
        echo -e "${GREEN}OK${NC}"
    else
        echo -e "${RED}FAILED${NC}"
        echo "Error: Failed to import dashboard"
        exit 1
    fi
done

echo -e "\n${GREEN}Monitoring deployment completed successfully!${NC}"
echo "Access Grafana at http://localhost:3000"
echo "Access Prometheus at http://localhost:9090"
echo "Access Alertmanager at http://localhost:9093"
