#!/bin/bash

# Exit on error
set -e

# Configuration
DB_CONTAINER="emrnext-db"
DEFAULT_DATABASE="EMRNext"

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
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  --database <name>    Database to verify (default: EMRNext)"
    echo "  --verbose           Show detailed output"
    echo ""
    exit 1
}

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: $1${NC}" >&2
}

# Parse command line arguments
DATABASE=${DEFAULT_DATABASE}
VERBOSE=0

while [[ $# -gt 0 ]]; do
    case $1 in
        --database)
            DATABASE="$2"
            shift 2
            ;;
        --verbose)
            VERBOSE=1
            shift
            ;;
        *)
            usage
            ;;
    esac
done

# SQL queries for data verification
read -r -d '' VERIFY_QUERIES << EOM
-- Check database consistency
DBCC CHECKDB ('${DATABASE}') WITH NO_INFOMSGS;

-- Check for orphaned records
SELECT 'Encounters without Patients' as Check_Type, COUNT(*) as Issue_Count
FROM Encounters e
LEFT JOIN Patients p ON e.PatientId = p.Id
WHERE p.Id IS NULL
UNION ALL
SELECT 'Orders without Encounters', COUNT(*)
FROM Orders o
LEFT JOIN Encounters e ON o.EncounterId = e.Id
WHERE e.Id IS NULL
UNION ALL
SELECT 'Clinical Notes without Encounters', COUNT(*)
FROM ClinicalNotes n
LEFT JOIN Encounters e ON n.EncounterId = e.Id
WHERE e.Id IS NULL;

-- Check for data integrity issues
SELECT 'Future Dates' as Check_Type, COUNT(*) as Issue_Count
FROM Encounters
WHERE EncounterDate > GETDATE()
UNION ALL
SELECT 'Invalid Birth Dates', COUNT(*)
FROM Patients
WHERE DateOfBirth > GETDATE() OR DateOfBirth < '1900-01-01';

-- Check for missing required data
SELECT 'Missing Patient Info' as Check_Type, COUNT(*) as Issue_Count
FROM Patients
WHERE FirstName IS NULL OR LastName IS NULL OR DateOfBirth IS NULL
UNION ALL
SELECT 'Incomplete Encounters', COUNT(*)
FROM Encounters
WHERE EncounterDate IS NULL OR ProviderId IS NULL;

-- Check for duplicate records
SELECT 'Duplicate Patients' as Check_Type, COUNT(*) as Issue_Count
FROM (
    SELECT FirstName, LastName, DateOfBirth, COUNT(*) as cnt
    FROM Patients
    GROUP BY FirstName, LastName, DateOfBirth
    HAVING COUNT(*) > 1
) t;

-- Check for referential integrity
SELECT 'Invalid Provider References' as Check_Type, COUNT(*) as Issue_Count
FROM Encounters e
LEFT JOIN Providers p ON e.ProviderId = p.Id
WHERE p.Id IS NULL;

-- Check for security issues
SELECT 'Inactive Users with Access' as Check_Type, COUNT(*) as Issue_Count
FROM Users
WHERE IsActive = 0 AND LastLoginDate > DATEADD(day, -30, GETDATE());
EOM

log "Starting data verification for database: ${DATABASE}"

# Run verification queries
echo "${VERIFY_QUERIES}" | docker exec -i ${DB_CONTAINER} /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "${DB_PASSWORD}" \
    -d "${DATABASE}" \
    -h -1 \
    -W \
    -s "," \
    > verification_results.csv

# Process results
TOTAL_ISSUES=0
while IFS=, read -r check_type issue_count; do
    if [ "${issue_count}" -gt 0 ]; then
        error "${check_type}: ${issue_count} issues found"
        TOTAL_ISSUES=$((TOTAL_ISSUES + issue_count))
    elif [ ${VERBOSE} -eq 1 ]; then
        log "${check_type}: No issues found"
    fi
done < verification_results.csv

# Cleanup
rm verification_results.csv

# Final report
if [ ${TOTAL_ISSUES} -eq 0 ]; then
    log "Data verification completed successfully. No issues found."
    exit 0
else
    error "Data verification completed with ${TOTAL_ISSUES} total issues found."
    exit 1
fi
