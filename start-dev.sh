#!/bin/bash

# Function to check if a service is healthy
check_service() {
    local service=$1
    local max_attempts=$2
    local attempt=1

    echo "Checking $service health..."
    while [ $attempt -le $max_attempts ]; do
        if curl -s "http://localhost:$3/health" > /dev/null; then
            echo "$service is healthy!"
            return 0
        fi
        echo "Attempt $attempt/$max_attempts: $service not ready yet..."
        sleep 5
        attempt=$((attempt + 1))
    done
    echo "$service health check failed after $max_attempts attempts"
    return 1
}

# Stop any running containers
echo "Stopping any existing containers..."
docker-compose down

# Start the services
echo "Starting services..."
docker-compose up -d

# Wait for database to be ready
echo "Waiting for database to be ready..."
sleep 30

# Initialize the database with test data
echo "Initializing database with test data..."
docker-compose exec db /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P YourStrong@Password \
    -i /var/opt/mssql/InitialSetup.sql

# Check API health
check_service "API" 12 5000
api_status=$?

# Check Frontend health
check_service "Frontend" 6 3000
frontend_status=$?

if [ $api_status -eq 0 ] && [ $frontend_status -eq 0 ]; then
    echo "
=================================
Development environment is ready!
=================================

Services:
- Frontend: http://localhost:3000
- API: http://localhost:5000

Test Accounts:
- Provider: john.doe@test.com / test123
- Provider: jane.smith@test.com / test123

Test Patients:
- MRN001: Alice Johnson
- MRN002: Bob Williams
- MRN003: Carol Davis

To stop the environment:
$ docker-compose down

To view logs:
$ docker-compose logs -f [service_name]

Happy coding! 🚀
"
else
    echo "Failed to start development environment. Please check the logs:"
    docker-compose logs
fi
