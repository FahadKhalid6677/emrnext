#!/bin/bash

# Exit on error
set -e

# Load environment variables
if [ -f .env ]; then
    source .env
fi

# Check required environment variables
if [ -z "$DB_PASSWORD" ]; then
    echo "Error: DB_PASSWORD environment variable is not set"
    exit 1
fi

# Function to display deployment status
function echo_status() {
    echo "----------------------------------------"
    echo "$1"
    echo "----------------------------------------"
}

# Build and push Docker images
echo_status "Building and pushing Docker images..."
docker-compose build
docker-compose push

# Apply database migrations
echo_status "Applying database migrations..."
dotnet ef database update --project src/EMRNext.Core --startup-project src/EMRNext.API

# Start the application
echo_status "Starting the application..."
docker-compose up -d

# Wait for services to be healthy
echo_status "Waiting for services to be healthy..."
sleep 30

# Run health check
echo_status "Running health check..."
curl -f http://localhost:5000/health || {
    echo "Health check failed"
    exit 1
}

echo_status "Deployment completed successfully!"
