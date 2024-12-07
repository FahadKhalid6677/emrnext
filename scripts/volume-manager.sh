#!/bin/bash

# EMRNext Volume Management Script

# Configuration
BACKUP_DIR="/data/backups"
RETENTION_DAYS=7
LOG_FILE="/var/log/emrnext/volume-manager.log"

# Ensure required directories exist
mkdir -p "${BACKUP_DIR}"
mkdir -p "$(dirname "${LOG_FILE}")"

# Logging function
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "${LOG_FILE}"
}

# Create backup of a volume
backup_volume() {
    local volume_name="$1"
    local backup_path="${BACKUP_DIR}/${volume_name}_$(date +%Y%m%d_%H%M%S).tar.gz"
    
    log "Starting backup of volume: ${volume_name}"
    docker run --rm \
        -v "${volume_name}:/source:ro" \
        -v "${BACKUP_DIR}:/backup" \
        alpine tar czf "/backup/$(basename ${backup_path})" -C /source .
    
    if [ $? -eq 0 ]; then
        log "Backup completed successfully: ${backup_path}"
    else
        log "ERROR: Backup failed for volume: ${volume_name}"
        return 1
    fi
}

# Restore volume from backup
restore_volume() {
    local volume_name="$1"
    local backup_file="$2"
    
    if [ ! -f "${backup_file}" ]; then
        log "ERROR: Backup file not found: ${backup_file}"
        return 1
    fi
    
    log "Starting restore of volume: ${volume_name}"
    docker run --rm \
        -v "${volume_name}:/target" \
        -v "${backup_file}:/backup.tar.gz:ro" \
        alpine sh -c "cd /target && tar xzf /backup.tar.gz"
    
    if [ $? -eq 0 ]; then
        log "Restore completed successfully for volume: ${volume_name}"
    else
        log "ERROR: Restore failed for volume: ${volume_name}"
        return 1
    fi
}

# Cleanup old backups
cleanup_old_backups() {
    log "Starting cleanup of old backups"
    find "${BACKUP_DIR}" -type f -name "*.tar.gz" -mtime +${RETENTION_DAYS} -delete
    log "Cleanup completed"
}

# Main script logic
case "$1" in
    backup)
        if [ -z "$2" ]; then
            log "ERROR: Volume name required for backup"
            exit 1
        fi
        backup_volume "$2"
        ;;
    restore)
        if [ -z "$2" ] || [ -z "$3" ]; then
            log "ERROR: Volume name and backup file required for restore"
            exit 1
        fi
        restore_volume "$2" "$3"
        ;;
    cleanup)
        cleanup_old_backups
        ;;
    *)
        echo "Usage: $0 {backup|restore|cleanup} [volume_name] [backup_file]"
        exit 1
        ;;
esac

exit 0
