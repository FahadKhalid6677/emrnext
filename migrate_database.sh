#!/bin/bash

# Navigate to the project directory
cd /Users/fahadkhalid/EMRNext/src/EMRNext.Web

# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Restore dependencies
dotnet restore

# Create migrations
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

echo "Database migration complete!"
