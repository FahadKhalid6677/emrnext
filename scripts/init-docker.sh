#!/bin/bash

# EMRNext Docker Environment Initialization Script

# Configuration
PROJECT_ROOT=$(pwd)
DATA_DIR="${PROJECT_ROOT}/data"
LOG_FILE="${PROJECT_ROOT}/logs/docker-init.log"

# Ensure required directories exist
mkdir -p "${DATA_DIR}/"{db,redis,logs,monitoring,backups}
mkdir -p "$(dirname "${LOG_FILE}")"

# Logging function
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "${LOG_FILE}"
}

# Create Docker networks
create_networks() {
    log "Creating Docker networks..."
    
    # Frontend network
    docker network create \
        --driver bridge \
        --subnet 172.20.0.0/16 \
        --label com.emrnext.network.type=frontend \
        frontend_network || log "Frontend network already exists"
    
    # Backend network
    docker network create \
        --driver bridge \
        --subnet 172.21.0.0/16 \
        --internal \
        --label com.emrnext.network.type=backend \
        backend_network || log "Backend network already exists"
    
    # Database network
    docker network create \
        --driver bridge \
        --subnet 172.22.0.0/16 \
        --internal \
        --label com.emrnext.network.type=database \
        database_network || log "Database network already exists"
}

# Set up Docker volumes
setup_volumes() {
    log "Setting up Docker volumes..."
    
    # Create directories with proper permissions
    chmod 755 "${DATA_DIR}"
    chmod 700 "${DATA_DIR}/"{db,redis}
    chmod 755 "${DATA_DIR}/"{logs,monitoring,backups}
    
    # Create Docker volumes
    docker volume create \
        --driver local \
        --opt type=none \
        --opt device="${DATA_DIR}/db" \
        --opt o=bind \
        db_data
    
    docker volume create \
        --driver local \
        --opt type=none \
        --opt device="${DATA_DIR}/redis" \
        --opt o=bind \
        redis_data
    
    docker volume create \
        --driver local \
        --opt type=none \
        --opt device="${DATA_DIR}/logs" \
        --opt o=bind \
        log_data
}

# Initialize monitoring
init_monitoring() {
    log "Initializing monitoring..."
    
    # Create monitoring directories
    mkdir -p "${DATA_DIR}/monitoring/"{prometheus,grafana,alertmanager}
    
    # Set proper permissions
    chmod 755 "${DATA_DIR}/monitoring"
    chmod 755 "${DATA_DIR}/monitoring/"{prometheus,grafana,alertmanager}
}

# Main initialization process
main() {
    log "Starting Docker environment initialization..."
    
    # Create networks
    create_networks
    
    # Set up volumes
    setup_volumes
    
    # Initialize monitoring
    init_monitoring
    
    log "Docker environment initialization completed successfully"
}

# Run main function
main

exit 0
