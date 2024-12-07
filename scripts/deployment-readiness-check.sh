#!/bin/bash

# EMRNext Deployment Readiness Verification Script

set -e

echo "🔍 Starting Deployment Readiness Check"
echo "======================================"

# Verify Railway Authentication
echo -n "🔐 Checking Railway Authentication... "
railway whoami || {
    echo "❌ Authentication Failed"
    exit 1
}

# Check Project Initialization
echo -n "🚀 Verifying Project Initialization... "
railway list | grep -q "emrnext" || {
    echo "❌ Project Not Found"
    exit 1
}

# Validate Dockerfiles
echo "🐳 Checking Dockerfile Configurations:"
DOCKERFILES=("Dockerfile.backend" "Dockerfile.frontend")
for dockerfile in "${DOCKERFILES[@]}"; do
    if [ ! -f "$dockerfile" ]; then
        echo "❌ $dockerfile is missing"
        exit 1
    fi
    docker build -t "emrnext-${dockerfile%.*}" -f "$dockerfile" . &> /dev/null && \
        echo "✅ $dockerfile build successful" || {
        echo "❌ $dockerfile build failed"
        exit 1
    }
done

# Verify Railway Configuration
echo -n "📋 Checking railway.json... "
if [ ! -f "railway.json" ]; then
    echo "❌ railway.json is missing"
    exit 1
fi

# Check Environment Variables
echo "🔑 Verifying Environment Variables:"
REQUIRED_VARS=(
    "ASPNETCORE_ENVIRONMENT"
    "DATABASE_URL"
    "JWT_SECRET"
    "NODE_ENV"
)

for var in "${REQUIRED_VARS[@]}"; do
    railway variables get "$var" &> /dev/null && \
        echo "✅ $var is set" || {
        echo "❌ $var is not configured"
        exit 1
    }
done

# Validate Service Connections
echo -n "🔗 Checking Service Configurations... "
railway service list | grep -E "backend|frontend|database" &> /dev/null || {
    echo "❌ Service configurations incomplete"
    exit 1
}

echo ""
echo "🎉 Deployment Readiness Check Complete!"
echo "EMRNext is ready for deployment on Railway!"
exit 0
