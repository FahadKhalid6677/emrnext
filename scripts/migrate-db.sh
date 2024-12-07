#!/bin/bash

# Exit on any error
set -e

# Load environment variables
source .env.production

# Database migration script for EMRNext
echo "Starting database migration..."

# Perform database migration using Entity Framework Core
dotnet ef database update \
    --connection "Host=$DB_HOST;Port=$DB_PORT;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD" \
    --startup-project src/EMRNext.Web \
    --project src/EMRNext.Core

# Optional: Seed initial data if needed
dotnet run --project src/EMRNext.Web -- seeddata

echo "Database migration completed successfully."
