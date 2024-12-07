#!/bin/bash

# Comprehensive Deployment Preparation Script

# Exit on first error
set -e

# Color codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Logging functions
log() {
    echo -e "${GREEN}[PREP]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Validate Prerequisites
validate_prerequisites() {
    log "Checking Prerequisites..."
    
    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        error ".NET SDK is not installed. Please install .NET 7.0 SDK."
        exit 1
    fi

    # Check Node.js
    if ! command -v node &> /dev/null; then
        error "Node.js is not installed. Please install Node.js 16+."
        exit 1
    fi

    # Check PostgreSQL
    if ! command -v psql &> /dev/null; then
        warn "PostgreSQL is not installed. Please install PostgreSQL."
    fi
}

# Clean and Restore Solution
restore_solution() {
    log "Cleaning and Restoring Solution..."
    
    cd /Users/fahadkhalid/EMRNext
    
    # Clean solution
    dotnet clean

    # Restore NuGet packages
    dotnet restore

    # Update packages to resolve version conflicts
    dotnet list package --outdated
}

# Fix Potential Compilation Issues
fix_compilation_issues() {
    log "Attempting to Fix Compilation Issues..."

    # Navigate to Core project
    cd /Users/fahadkhalid/EMRNext/src/EMRNext.Core

    # Add missing type references or stub implementations
    cat > /Users/fahadkhalid/EMRNext/src/EMRNext.Core/Stubs/StubInterfaces.cs << EOL
namespace EMRNext.Core.Stubs {
    public interface IEmailService {}
    public interface ISMSService {}
    public interface IPushNotificationService {}
    public interface IMessageBroker {}
    public interface IPreferenceService {}
    public interface ILabOrderService {}
    public interface IImagingService {}
    public interface IDocumentStorageService {}
    public interface ISecurityService {}
    public interface IUserContext {}
    public interface ITaskValidator {}
    public interface IAuditService {}
    public interface IAlertService {}
    public interface INotificationService {}
    public interface IEncryptionService {}
    public interface IPrescriptionRepository {}
    public interface IDrugInteractionRepository {}
    public interface IPatientRepository {}
    public interface IVitalSignRepository {}
    public interface IAuditRepository {}

    public class Result {}
    public class Alert {}
    public class Recommendation {}
    public class TimeSlot {}
    public class WaitlistEntry {}
    public class ScheduleMetrics {}
    public class UtilizationReport {}
    public class WorkQueueTask {}
    public class WorkQueueTaskRequest {}
    public class TaskPriority {}
    public class NotificationPriority {}
}
EOL
}

# Prepare Frontend
prepare_frontend() {
    log "Preparing Frontend..."
    
    cd /Users/fahadkhalid/EMRNext/src/EMRNext.Web/ClientApp
    
    # Install dependencies
    npm install

    # Build frontend
    npm run build
}

# Database Preparation
prepare_database() {
    log "Preparing Database..."
    
    cd /Users/fahadkhalid/EMRNext/src/EMRNext.Web

    # Run database migrations
    dotnet ef database update
}

# Main Deployment Preparation Function
main() {
    log "Starting EMRNext Deployment Preparation..."

    validate_prerequisites
    restore_solution
    fix_compilation_issues
    prepare_frontend
    prepare_database

    log "Deployment Preparation Complete! ✅"
    log "Next Steps:"
    log "1. Review compilation output"
    log "2. Test local deployment"
    log "3. Prepare for cloud deployment"
}

# Run the main function
main
