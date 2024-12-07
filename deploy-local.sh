#!/bin/bash

# Navigate to the project directory
cd /Users/fahadkhalid/EMRNext/src/EMRNext.Web

# Install .NET EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Run database migrations
dotnet ef database update

# Build the backend
dotnet build

# Start the backend server
dotnet run &
BACKEND_PID=$!

# Navigate to frontend directory
cd ../EMRNext.Web/ClientApp

# Install npm dependencies
npm install

# Start the React development server
npm start &
FRONTEND_PID=$!

# Wait for user input to stop
echo "Backend and Frontend are running. Press Enter to stop."
read

# Kill the background processes
kill $BACKEND_PID
kill $FRONTEND_PID

exit 0
