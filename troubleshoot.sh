#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}EMRNext Environment Troubleshooter${NC}"
echo "=================================="

# Function to check if a port is in use
check_port() {
    local port=$1
    if lsof -i :$port > /dev/null; then
        echo -e "${RED}Port $port is already in use${NC}"
        echo "Process using port $port:"
        lsof -i :$port
        return 1
    else
        echo -e "${GREEN}Port $port is available${NC}"
        return 0
    }
}

# Function to check Docker service
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}Docker is not running${NC}"
        echo "Please start Docker Desktop and try again"
        return 1
    else
        echo -e "${GREEN}Docker is running${NC}"
        return 0
    }
}

# Function to check container status
check_container() {
    local container=$1
    if docker ps --format '{{.Names}}' | grep -q "^$container$"; then
        echo -e "${GREEN}Container $container is running${NC}"
        return 0
    else
        echo -e "${RED}Container $container is not running${NC}"
        echo "Container logs:"
        docker logs $container 2>&1 | tail -n 20
        return 1
    }
}

# Function to check API health
check_api_health() {
    if curl -s http://localhost:5000/health > /dev/null; then
        echo -e "${GREEN}API health check passed${NC}"
        return 0
    else
        echo -e "${RED}API health check failed${NC}"
        return 1
    }
}

# Main troubleshooting flow
echo "Step 1: Checking Docker service..."
check_docker || exit 1

echo -e "\nStep 2: Checking required ports..."
check_port 3000 # Frontend
check_port 5000 # API
check_port 1433 # SQL Server

echo -e "\nStep 3: Checking container status..."
containers=("emrnext-db" "emrnext-api" "emrnext-frontend" "emrnext-redis" "emrnext-seq")
for container in "${containers[@]}"; do
    check_container $container
done

echo -e "\nStep 4: Checking API health..."
check_api_health

echo -e "\nStep 5: Checking database connection..."
docker exec emrnext-db /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P YourStrong@Password \
    -Q "SELECT @@VERSION" > /dev/null 2>&1

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Database connection successful${NC}"
else
    echo -e "${RED}Database connection failed${NC}"
    echo "Attempting to view SQL Server logs:"
    docker logs emrnext-db 2>&1 | tail -n 20
fi

echo -e "\nStep 6: Environment Variables Check..."
echo "Frontend API URL:"
docker exec emrnext-frontend printenv REACT_APP_API_URL
echo "API Database Connection:"
docker exec emrnext-api printenv ConnectionStrings__DefaultConnection

echo -e "\nTroubleshooting Complete!"
echo "=================================="
echo -e "If issues persist, try the following:"
echo "1. Stop all containers:    docker-compose down"
echo "2. Remove all volumes:     docker-compose down -v"
echo "3. Rebuild all images:     docker-compose build"
echo "4. Start fresh:           docker-compose up -d"
echo "5. View detailed logs:    docker-compose logs -f"
