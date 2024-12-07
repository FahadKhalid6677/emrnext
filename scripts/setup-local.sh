#!/bin/bash

# Exit on error
set -e

echo "Setting up EMRNext local development environment..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "Docker is not running. Please start Docker and try again."
    exit 1
fi

# Create .env file if it doesn't exist
if [ ! -f .env ]; then
    echo "Creating .env file..."
    cat > .env << EOF
DB_PASSWORD=EMRNext_LocalDev2024!
ASPNETCORE_ENVIRONMENT=Development
EOF
fi

# Build and start containers
echo "Starting Docker containers..."
docker-compose down
docker-compose up -d

# Wait for database to be ready
echo "Waiting for database to be ready..."
sleep 30

# Run database migrations
echo "Running database migrations..."
dotnet ef database update --project src/EMRNext.Infrastructure --startup-project src/EMRNext.Web

# Seed essential data
echo "Seeding reference data..."
dotnet run --project src/EMRNext.Web/EMRNext.Web.csproj -- --seed-data

# Create test accounts
echo "Creating test accounts..."
cat > src/EMRNext.Infrastructure/Data/SeedData/TestUsers.sql << EOF
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'testdoctor')
BEGIN
    INSERT INTO Users (Username, Email, FirstName, LastName, Role)
    VALUES 
    ('testdoctor', 'doctor@test.com', 'Test', 'Doctor', 'Physician'),
    ('testnurse', 'nurse@test.com', 'Test', 'Nurse', 'Nurse'),
    ('testadmin', 'admin@test.com', 'Test', 'Admin', 'Administrator')
END
EOF

# Import test patient data
echo "Importing test patient data..."
cat > src/EMRNext.Infrastructure/Data/SeedData/TestPatients.sql << EOF
IF NOT EXISTS (SELECT * FROM Patients WHERE MRN = 'TEST001')
BEGIN
    INSERT INTO Patients (MRN, FirstName, LastName, DateOfBirth, Gender)
    VALUES 
    ('TEST001', 'John', 'Doe', '1980-01-01', 'M'),
    ('TEST002', 'Jane', 'Smith', '1990-02-15', 'F'),
    ('TEST003', 'Robert', 'Johnson', '1975-05-20', 'M')
END
EOF

# Run SQL scripts
docker exec emrnext_db /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P $DB_PASSWORD \
    -d EMRNext -i /src/EMRNext.Infrastructure/Data/SeedData/TestUsers.sql

docker exec emrnext_db /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P $DB_PASSWORD \
    -d EMRNext -i /src/EMRNext.Infrastructure/Data/SeedData/TestPatients.sql

echo "Setup complete! The system is ready for local testing."
echo ""
echo "Access points:"
echo "- API: https://localhost:5001"
echo "- Seq (logging): http://localhost:5341"
echo ""
echo "Test accounts:"
echo "- Doctor: testdoctor / EMRNext2024!"
echo "- Nurse: testnurse / EMRNext2024!"
echo "- Admin: testadmin / EMRNext2024!"
echo ""
echo "Test patients:"
echo "- MRN: TEST001 (John Doe)"
echo "- MRN: TEST002 (Jane Smith)"
echo "- MRN: TEST003 (Robert Johnson)"
