# EMRNext - Modern Electronic Medical Records System

## Project Overview
EMRNext is a modern, secure, and scalable Electronic Medical Records (EMR) system built with ASP.NET Core. It follows clean architecture principles and implements industry best practices for healthcare software development.

## Solution Structure

```
EMRNext/
├── src/
│   ├── EMRNext.Web/           # ASP.NET Core MVC Application
│   ├── EMRNext.API/           # RESTful API Services
│   ├── EMRNext.Core/          # Domain Models and Business Logic
│   ├── EMRNext.Infrastructure/# Data Access and External Services
│   └── EMRNext.Shared/        # Shared Components and Utilities
└── tests/
    ├── EMRNext.UnitTests/
    ├── EMRNext.IntegrationTests/
    └── EMRNext.FunctionalTests/
```

## Features
- Patient Management
- Clinical Documentation
- Appointment Scheduling
- Billing and Claims
- Prescription Management
- Laboratory Integration
- Secure Authentication
- Role-based Access Control
- HIPAA Compliance
- Audit Logging

## Technology Stack
- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- Identity Server
- MediatR
- AutoMapper
- FluentValidation
- xUnit
- Moq

## Local Deployment Instructions

### Prerequisites
- .NET 7.0 SDK
- Node.js 16+ 
- PostgreSQL 13+

### Backend Setup
1. Install .NET EF Core tools
```bash
dotnet tool install --global dotnet-ef
```

2. Configure Database Connection
- Open `src/EMRNext.Web/appsettings.Development.json`
- Update PostgreSQL connection string with your local credentials

3. Run Database Migrations
```bash
cd src/EMRNext.Web
dotnet ef database update
```

### Frontend Setup
1. Navigate to Frontend Directory
```bash
cd src/EMRNext.Web/ClientApp
```

2. Install Dependencies
```bash
npm install
```

### Running the Application

#### Option 1: Separate Terminal Windows
1. Start Backend (Port 5000)
```bash
cd src/EMRNext.Web
dotnet run
```

2. Start Frontend (Port 3000)
```bash
cd src/EMRNext.Web/ClientApp
npm start
```

#### Option 2: Combined Script
```bash
./deploy-local.sh
```

### Initial Login Credentials
- Email: admin@emrnext.com
- Password: EMRNext2024!

### Troubleshooting
- Ensure all prerequisites are installed
- Check connection strings in `appsettings.Development.json`
- Verify PostgreSQL is running
- Check console for specific error messages

## Development Notes
- Backend: ASP.NET Core 7.0
- Frontend: React with TypeScript
- State Management: Redux Toolkit
- Authentication: JWT
- Database: PostgreSQL

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 or VS Code

### Setup
1. Clone the repository
2. Navigate to the solution directory
3. Run `dotnet restore`
4. Update the connection string in appsettings.json
5. Run `dotnet ef database update`
6. Start the application with `dotnet run`

## Local Development Setup

### Quick Start
1. Ensure Docker Desktop is running
2. Run the setup script:
   ```bash
   chmod +x scripts/setup-local.sh
   ./scripts/setup-local.sh
   ```
3. Access the application:
   - API: https://localhost:5001
   - Seq (logging): http://localhost:5341

### Test Accounts
- Doctor: testdoctor / EMRNext2024!
- Nurse: testnurse / EMRNext2024!
- Admin: testadmin / EMRNext2024!

### Test Patients
- MRN: TEST001 (John Doe)
- MRN: TEST002 (Jane Smith)
- MRN: TEST003 (Robert Johnson)

### Available Features for Testing
1. Clinical Documentation
   - Progress notes
   - H&P documentation
   - Procedure notes
   - Discharge summaries

2. Order Management
   - Lab orders
   - Imaging orders
   - Medication orders
   - Order sets

3. Results Review
   - Lab results
   - Imaging reports
   - Critical values
   - Trending

4. Patient Management
   - Registration
   - Demographics
   - Medical history
   - Allergies

5. Work Queue
   - Task management
   - Result review
   - Document signing
   - Message handling

### Development Workflow
1. Start the environment:
   ```bash
   docker-compose up -d
   ```

2. Watch for changes:
   ```bash
   dotnet watch run --project src/EMRNext.Web
   ```

3. Run tests:
   ```bash
   dotnet test
   ```

4. View logs:
   - Access Seq at http://localhost:5341
   - Filter by application component
   - Monitor errors and performance

### Troubleshooting
1. Database Issues
   ```bash
   # Reset database
   docker-compose down
   docker volume rm emrnext_dbdata
   docker-compose up -d
   ```

2. Clean Rebuild
   ```bash
   # Clean solution
   dotnet clean
   # Rebuild all projects
   dotnet build
   ```

3. Reset Test Data
   ```bash
   # Re-run setup script
   ./scripts/setup-local.sh
   ```

## Development Guidelines
- Follow Clean Architecture principles
- Use CQRS pattern with MediatR
- Implement Domain-Driven Design
- Write unit tests for all business logic
- Follow SOLID principles
- Use async/await for I/O operations
- Implement proper error handling
- Add logging for all operations

## Security
- HIPAA compliance
- Data encryption at rest and in transit
- Secure authentication and authorization
- Regular security audits
- Protected health information (PHI) handling

## Contributing
Please read CONTRIBUTING.md for details on our code of conduct and the process for submitting pull requests.

## License
This project is licensed under the MIT License - see the LICENSE.md file for details
