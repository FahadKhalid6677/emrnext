#!/bin/bash

# Deployment Preparation Script for EMRNext

# Exit on any error
set -e

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to log messages
log() {
    echo -e "${GREEN}[DEPLOYMENT PREP]${NC} $1"
}

# Function to log warnings
warn() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Validate Prerequisites
validate_prerequisites() {
    log "Checking Prerequisites..."
    
    # Check Node.js
    if ! command -v node &> /dev/null; then
        warn "Node.js is not installed. Please install Node.js 16+ first."
        exit 1
    fi

    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        warn ".NET SDK is not installed. Please install .NET 7.0 SDK first."
        exit 1
    fi

    # Check Git
    if ! command -v git &> /dev/null; then
        warn "Git is not installed. Please install Git first."
        exit 1
    fi
}

# Prepare Backend
prepare_backend() {
    log "Preparing Backend..."
    
    cd /Users/fahadkhalid/EMRNext/src/EMRNext.Web

    # Restore .NET dependencies
    dotnet restore

    # Build the project
    dotnet build

    # Optional: Run backend tests
    # dotnet test
}

# Prepare Frontend
prepare_frontend() {
    log "Preparing Frontend..."
    
    cd /Users/fahadkhalid/EMRNext/src/EMRNext.Web/ClientApp

    # Install npm dependencies
    npm install

    # Build the frontend
    npm run build
}

# Create Deployment Dockerfiles
create_dockerfiles() {
    log "Creating Deployment Dockerfiles..."

    # Backend Dockerfile
    cat > /Users/fahadkhalid/EMRNext/Dockerfile.backend << EOL
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY src/EMRNext.Web/*.csproj ./
RUN dotnet restore

COPY src/EMRNext.Web/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "EMRNext.Web.dll"]
EOL

    # Frontend Dockerfile
    cat > /Users/fahadkhalid/EMRNext/Dockerfile.frontend << EOL
FROM node:18-alpine AS build
WORKDIR /app

COPY src/EMRNext.Web/ClientApp/package*.json ./
RUN npm install

COPY src/EMRNext.Web/ClientApp/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
EOL
}

# Create Railway Configuration
create_railway_config() {
    log "Creating Railway Configuration..."

    cat > /Users/fahadkhalid/EMRNext/railway.json << EOL
{
  "version": 2,
  "name": "emrnext",
  "services": [
    {
      "name": "backend",
      "dockerfile": "./Dockerfile.backend",
      "port": 5000,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ASPNETCORE_URLS": "http://+:5000"
      }
    },
    {
      "name": "frontend",
      "dockerfile": "./Dockerfile.frontend",
      "port": 3000,
      "env": {
        "NODE_ENV": "production",
        "VITE_API_URL": "\${backend.RAILWAY_PUBLIC_URL}/api"
      }
    },
    {
      "name": "database",
      "image": "postgres:15",
      "port": 5432,
      "env": {
        "POSTGRES_DB": "emrnext",
        "POSTGRES_USER": "emrnext_user"
      }
    }
  ]
}
EOL
}

# Create Nginx Configuration
create_nginx_config() {
    log "Creating Nginx Configuration..."

    cat > /Users/fahadkhalid/EMRNext/nginx.conf << EOL
server {
    listen 3000;
    server_name localhost;

    location / {
        root /usr/share/nginx/html;
        index index.html index.htm;
        try_files \$uri \$uri/ /index.html;
    }

    location /api {
        proxy_pass http://backend:5000/api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
    }

    error_page 500 502 503 504 /50x.html;
    location = /50x.html {
        root /usr/share/nginx/html;
    }
}
EOL
}

# Main Deployment Preparation Function
main() {
    log "Starting EMRNext Deployment Preparation..."

    validate_prerequisites
    prepare_backend
    prepare_frontend
    create_dockerfiles
    create_railway_config
    create_nginx_config

    log "Deployment Preparation Complete! ✅"
    log "Next Steps:"
    log "1. Review created configuration files"
    log "2. Commit changes to GitHub"
    log "3. Connect to Railway.app"
}

# Run the main function
main
