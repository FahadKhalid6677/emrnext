#!/bin/bash

# Exit on error
set -e

# Configuration
BACKUP_DIR="/var/backups/emrnext"
RETENTION_DAYS=30
DB_CONTAINER="emrnext-db"
SECONDARY_STORAGE="s3://emrnext-backups"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Load environment variables
if [ -f .env ]; then
    source .env
fi

usage() {
    echo "Usage: $0 [command] [options]"
    echo ""
    echo "Commands:"
    echo "  list                    List available backups"
    echo "  full-backup             Perform full backup"
    echo "  verify --id <backup-id> Verify backup integrity"
    echo "  test-restore --id <backup-id> Test restore to staging"
    echo "  verify-replication      Check geo-replication status"
    echo ""
    exit 1
}

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: $1${NC}" >&2
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING: $1${NC}"
}

perform_full_backup() {
    log "Starting full backup..."
    
    # Create backup directory
    BACKUP_ID="backup_$(date +%Y%m%d_%H%M%S)"
    BACKUP_PATH="${BACKUP_DIR}/${BACKUP_ID}"
    mkdir -p "${BACKUP_PATH}"

    # Backup database
    log "Backing up database..."
    docker exec ${DB_CONTAINER} /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "${DB_PASSWORD}" \
        -Q "BACKUP DATABASE EMRNext TO DISK = '/var/opt/mssql/backup/emrnext.bak' WITH FORMAT"

    # Copy database backup
    docker cp ${DB_CONTAINER}:/var/opt/mssql/backup/emrnext.bak "${BACKUP_PATH}/"

    # Backup file storage
    log "Backing up file storage..."
    tar -czf "${BACKUP_PATH}/files.tar.gz" /var/emrnext/files

    # Backup configuration
    log "Backing up configuration..."
    cp /etc/emrnext/config/* "${BACKUP_PATH}/"

    # Create metadata file
    cat > "${BACKUP_PATH}/metadata.json" << EOF
{
    "backup_id": "${BACKUP_ID}",
    "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
    "type": "full",
    "components": ["database", "files", "config"]
}
EOF

    # Calculate checksums
    log "Calculating checksums..."
    find "${BACKUP_PATH}" -type f -exec sha256sum {} \; > "${BACKUP_PATH}/checksums.txt"

    # Encrypt backup
    log "Encrypting backup..."
    tar -czf "${BACKUP_PATH}.tar.gz" "${BACKUP_PATH}"
    gpg --encrypt --recipient emrnext-backup "${BACKUP_PATH}.tar.gz"
    rm "${BACKUP_PATH}.tar.gz"

    # Upload to secondary storage
    log "Uploading to secondary storage..."
    aws s3 cp "${BACKUP_PATH}.tar.gz.gpg" "${SECONDARY_STORAGE}/${BACKUP_ID}/"

    log "Backup completed successfully: ${BACKUP_ID}"
}

verify_backup() {
    local backup_id=$1
    log "Verifying backup: ${backup_id}"

    # Download from secondary storage if needed
    if [ ! -f "${BACKUP_DIR}/${backup_id}.tar.gz.gpg" ]; then
        log "Downloading backup from secondary storage..."
        aws s3 cp "${SECONDARY_STORAGE}/${backup_id}/${backup_id}.tar.gz.gpg" "${BACKUP_DIR}/"
    }

    # Decrypt backup
    log "Decrypting backup..."
    gpg --decrypt "${BACKUP_DIR}/${backup_id}.tar.gz.gpg" > "${BACKUP_DIR}/${backup_id}.tar.gz"

    # Extract backup
    log "Extracting backup..."
    tar -xzf "${BACKUP_DIR}/${backup_id}.tar.gz" -C "${BACKUP_DIR}"

    # Verify checksums
    log "Verifying checksums..."
    cd "${BACKUP_DIR}/${backup_id}"
    sha256sum -c checksums.txt

    # Verify database backup
    log "Verifying database backup..."
    docker exec ${DB_CONTAINER} /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "${DB_PASSWORD}" \
        -Q "RESTORE VERIFYONLY FROM DISK = '/var/opt/mssql/backup/emrnext.bak'"

    log "Backup verification completed successfully"
}

test_restore() {
    local backup_id=$1
    log "Testing restore of backup: ${backup_id}"

    # Create staging database
    log "Creating staging database..."
    docker exec ${DB_CONTAINER} /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "${DB_PASSWORD}" \
        -Q "CREATE DATABASE EMRNext_Staging"

    # Restore database to staging
    log "Restoring database to staging..."
    docker exec ${DB_CONTAINER} /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "${DB_PASSWORD}" \
        -Q "RESTORE DATABASE EMRNext_Staging FROM DISK = '/var/opt/mssql/backup/emrnext.bak' WITH MOVE 'EMRNext' TO '/var/opt/mssql/data/EMRNext_Staging.mdf', MOVE 'EMRNext_log' TO '/var/opt/mssql/data/EMRNext_Staging_log.ldf'"

    # Verify data integrity
    log "Verifying data integrity..."
    ./verify-data.sh --database EMRNext_Staging

    log "Test restore completed successfully"
}

verify_replication() {
    log "Checking geo-replication status..."

    # Check secondary storage sync
    aws s3 ls "${SECONDARY_STORAGE}" --recursive --summarize

    # Check last successful replication
    local last_backup=$(aws s3 ls "${SECONDARY_STORAGE}" --recursive | sort | tail -n 1)
    echo "Last backup in secondary storage: ${last_backup}"

    log "Replication verification completed"
}

# Main script logic
case "$1" in
    "list")
        ls -l "${BACKUP_DIR}" | grep "backup_"
        ;;
    "full-backup")
        perform_full_backup
        ;;
    "verify")
        if [ -z "$3" ]; then
            error "Backup ID required"
            usage
        fi
        verify_backup "$3"
        ;;
    "test-restore")
        if [ -z "$3" ]; then
            error "Backup ID required"
            usage
        fi
        test_restore "$3"
        ;;
    "verify-replication")
        verify_replication
        ;;
    *)
        usage
        ;;
esac
