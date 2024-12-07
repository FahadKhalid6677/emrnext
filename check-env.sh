#!/bin/bash

echo "EMRNext Environment Check"
echo "======================="

# Check Docker
echo "1. Checking Docker..."
if ! docker info > /dev/null 2>&1; then
    echo "Error: Docker is not running"
    exit 1
fi
echo "Docker is running"

# List running containers
echo -e "\n2. Running Containers:"
docker ps

# Check ports
echo -e "\n3. Checking ports..."
echo "Frontend (3000):"
lsof -i :3000
echo "API (5000):"
lsof -i :5000
echo "Database (1433):"
lsof -i :1433

# Check API health
echo -e "\n4. Checking API health..."
curl -s http://localhost:5000/health

# Show container logs
echo -e "\n5. Recent container logs:"
echo "Frontend logs:"
docker logs emrnext-frontend 2>&1 | tail -n 10
echo -e "\nAPI logs:"
docker logs emrnext-api 2>&1 | tail -n 10
echo -e "\nDatabase logs:"
docker logs emrnext-db 2>&1 | tail -n 10
