#!/bin/bash

# macOS Development Setup for EMRNext

# Check Homebrew installation
if ! command -v brew &> /dev/null; then
    echo "Installing Homebrew..."
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

# Update Homebrew
brew update

# Install required dependencies
brew install postgresql
brew install node
brew install --cask docker

# Start PostgreSQL
brew services start postgresql

# Create PostgreSQL user and database
psql postgres <<EOF
CREATE DATABASE emrnext_dev;
CREATE USER emrnext_user WITH PASSWORD 'emrnext_dev_password';
GRANT ALL PRIVILEGES ON DATABASE emrnext_dev TO emrnext_user;
EOF

# Install .NET SDK (if not already installed)
brew install --cask dotnet-sdk

# Install global .NET tools
dotnet tool install --global dotnet-ef

# Set executable permissions
chmod +x /Users/fahadkhalid/EMRNext/deploy.sh

echo "macOS Development Setup Complete!"
echo "Next steps:"
echo "1. Configure connection strings in appsettings.Development.json"
echo "2. Run database migrations"
echo "3. Start the application using ./deploy.sh"
