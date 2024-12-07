#!/bin/bash

# Install PostgreSQL if not already installed
brew install postgresql

# Start PostgreSQL service
brew services start postgresql

# Wait for PostgreSQL to start
sleep 5

# Create databases
psql postgres <<EOF
CREATE DATABASE emrnext_dev;
CREATE DATABASE emrnext_identity_dev;
CREATE USER emrnext_user WITH PASSWORD 'emrnext_dev_password';
GRANT ALL PRIVILEGES ON DATABASE emrnext_dev TO emrnext_user;
GRANT ALL PRIVILEGES ON DATABASE emrnext_identity_dev TO emrnext_user;
EOF

echo "PostgreSQL setup complete!"
