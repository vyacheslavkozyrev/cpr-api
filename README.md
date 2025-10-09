# CPR — Career & Performance Review API

A comprehensive API for managing career progression, performance reviews, skills assessment, and employee feedback.

## Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL Client** (optional, for manual DB management) - [Download](https://www.postgresql.org/download/)
- **PowerShell 5.1+** (Windows) or **PowerShell Core** (cross-platform)

## Technology Stack

### Backend
- **ASP.NET Core 9.0** - Web API framework
- **Entity Framework Core 9.0** - ORM for database access
- **Npgsql** - PostgreSQL provider for EF Core
- **Swashbuckle (Swagger)** - API documentation and testing UI

### Database
- **PostgreSQL 16** - Primary database

### Testing
- **xUnit** - Testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing
- **WebApplicationFactory** - Test host for API testing

### Infrastructure
- **Docker & Docker Compose** - Containerization for PostgreSQL
- **HMAC Authentication** - Development stub for user authentication

## Project Structure

```
CPR/
├── src/
│   ├── CPR.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/      # API endpoints
│   │   ├── Auth/            # Authentication handlers
│   │   ├── Services/        # Application services
│   │   └── Swagger/         # Swagger configuration & examples
│   ├── CPR.Application/      # Business logic layer
│   │   ├── Contracts/       # DTOs and interfaces
│   │   ├── Services/        # Business services
│   │   └── Repositories/    # Repository interfaces
│   ├── CPR.Domain/          # Domain entities
│   │   └── Entities/        # Core business entities
│   └── CPR.Infrastructure/   # Data access layer
│       ├── Data/            # DbContext and configurations
│       ├── Repositories/    # Repository implementations
│       ├── Services/        # Infrastructure services (seeding, etc.)
│       └── Migrations/      # EF Core migrations
├── tests/
│   ├── CPR.UnitTests/       # Unit tests
│   ├── CPR.IntegrationTests/ # Integration tests (API + DB)
│   └── CPR.ContractTests/   # API contract tests
├── scripts/
│   ├── run-api-as-employee.cmd  # Run API as Eve Adams (employee)
│   ├── run-api-as-manager.cmd   # Run API as Henry Wilson (manager)
│   ├── run-api-as-solution-owner.cmd  # Run API as John Doe (solution owner)
│   ├── generate-token.ps1   # Generate HMAC auth token
│   ├── run-tests.cmd        # Run all tests
│   └── run-docker-dev.cmd   # Start Docker containers
├── docker/
│   └── docker-compose.yml   # Both dev (5432) and test (5433) databases
├── documents/               # Documentation
│   ├── conventions.md       # Coding conventions
│   ├── endpoints.md         # API endpoint documentation
│   └── ...
├── .env.dev                 # Development environment variables
├── .env.test                # Test environment variables
└── README.md
```

## Environment Configuration

The project uses environment-specific `.env` files:

- **`.env.dev`** - Development database (cpr_dev on port 5432)
- **`.env.test`** - Test database (cpr_test on port 5433)

Variables used:
```env
POSTGRES_HOST=localhost
POSTGRES_PORT=5432         # 5433 for test
POSTGRES_DB=cpr_dev        # cpr_test for test
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
JWT_SIGNING_KEY=local-test-key
```

## Quick Start

### 1. Start Docker Containers

**Start both Dev and Test databases:**
```powershell
cd scripts
.\run-docker-dev.cmd
```
Or manually:
```powershell
docker-compose -f docker/docker-compose.yml up -d
```

This starts both PostgreSQL containers:
- **Dev Database**: `localhost:5432` (uses `.env.dev` → database `cpr_dev`)
- **Test Database**: `localhost:5433` (uses `.env.test` → database `cpr_test`)

Both containers run simultaneously from a single Docker Compose file.

### 2. Run the API

**Option A: Run as Employee (Eve Adams)**
```powershell
cd scripts
.\run-api-as-employee.cmd
```
- User: Eve Adams (Director of Security Engineering)
- User ID: `c7746e91-a5e8-4f8b-9f22-f48374ffa2a4`
- Token automatically copied to clipboard

**Option B: Run as Manager (Henry Wilson)**
```powershell
cd scripts
.\run-api-as-manager.cmd
```
- User: Henry Wilson (Director of Technical Support)
- User ID: `977f4f1f-b3ce-4244-98fc-2c0d0248de88`
- Token automatically copied to clipboard

**Option C: Run as Solution Owner (John Doe)**
```powershell
cd scripts
.\run-api-as-solution-owner.cmd
```
- User: John Doe (Solution Owner)
- User ID: `679add6e-6c29-4e00-b6a5-b69c8e0f3445`
- Token automatically copied to clipboard
- **Note**: Ensure john.doe has Solution Owner role assigned in database

**Option D: Manual run**
```powershell
dotnet run --project src\CPR.Api --urls "http://localhost:5000"
```

The API will:
1. Load environment variables from `.env.dev`
2. Run database migrations (create schema if needed)
3. Seed initial data (roles, users, skills, etc.)
4. Start on `http://localhost:5000`
5. Swagger UI available at `http://localhost:5000/swagger`

### 3. Access Swagger UI

1. Open `http://localhost:5000/swagger`
2. Click **Authorize** button
3. Paste the token (from clipboard if using run-api-as-*.cmd)
4. Click **Authorize** then **Close**
5. Test protected endpoints like `/api/me`

### 4. Run Tests

**Run all tests:**
```powershell
cd scripts
.\run-tests.cmd
```

**Run specific test suites:**
```powershell
# Unit tests only
dotnet test tests\CPR.UnitTests\CPR.UnitTests.csproj

# Integration tests only
dotnet test tests\CPR.IntegrationTests\CPR.IntegrationTests.csproj

# Contract tests only
dotnet test tests\CPR.ContractTests\CPR.ContractTests.csproj
```

**Test Summary:**
- **Unit Tests**: 84 tests - Business logic validation, services, repositories, and utilities
- **Integration Tests**: 100 tests - Full API + Database scenarios covering all endpoints
- **Contract Tests**: 16 tests - API contract validation & Swagger schema (11 Projects + 5 existing)
- **Total**: 200 tests - All passing ✅

All tests run from empty databases and handle their own setup/cleanup automatically.

## Authentication

### Development Authentication Stub

The API uses HMAC-signed tokens for development:

**Token Format:** `{userId}.{base64Signature}`

Where: `signature = HMACSHA256(JWT_SIGNING_KEY, userId)`

### Generate Token Manually

Use the provided script:
```powershell
.\scripts\generate-token.ps1 -UserId "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"
```

Or manually in PowerShell:
```powershell
$signingKey = 'local-test-key'
$userId = 'c7746e91-a5e8-4f8b-9f22-f48374ffa2a4'
$hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($signingKey))
$sig = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($userId))
$token = $userId + '.' + [Convert]::ToBase64String($sig)
Write-Host "Token: $token"
Set-Clipboard $token
```

### Pre-seeded Users

The database is automatically seeded with test users:

| Name | Role | User ID | Employee ID |
|------|------|---------|-------------|
| Ryan King | Administrator | `5950a2be-bdfb-4dcb-9913-1e3e0e022a5c` | `...001` |
| Sarah Johnson | Contributor | `f4c8e7a2-1b3d-4e6f-9a2c-8d7e6f5a4b3c` | `...002` |
| Eve Adams | Contributor (Security Dir) | `c7746e91-a5e8-4f8b-9f22-f48374ffa2a4` | `...007` |
| Henry Wilson | Contributor (Support Dir) | `977f4f1f-b3ce-4244-98fc-2c0d0248de88` | `...00a` |

## Database Management

### Run Migrations Manually
```powershell
# Apply all pending migrations
dotnet ef database update --project src\CPR.Infrastructure --startup-project src\CPR.Api

# Create a new migration
dotnet ef migrations add MigrationName --project src\CPR.Infrastructure --startup-project src\CPR.Api

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project src\CPR.Infrastructure --startup-project src\CPR.Api
```

### Reset Database
```powershell
# Drop and recreate (will lose all data)
dotnet ef database drop --project src\CPR.Infrastructure --startup-project src\CPR.Api --force
dotnet ef database update --project src\CPR.Infrastructure --startup-project src\CPR.Api
```

Or using Docker:
```powershell
docker-compose -f docker/docker-compose.dev.yml down -v
docker-compose -f docker/docker-compose.dev.yml up -d
```

## Common Tasks

### Build Solution
```powershell
dotnet build src\CPR.sln
```

### Clean Build
```powershell
dotnet clean src\CPR.sln
dotnet build src\CPR.sln
```

### Watch Mode (Auto-reload)
```powershell
dotnet watch --project src\CPR.Api
```

### View Logs
```powershell
# Docker logs
docker-compose -f docker/docker-compose.dev.yml logs -f

# Specific container
docker logs cpr-postgres-dev -f
```

## Troubleshooting

### API fails to start: "relation does not exist"
**Solution:** Drop and recreate the database:
```powershell
# Using psql
psql -h localhost -p 5432 -U postgres -c "DROP DATABASE IF EXISTS cpr_dev;"
# Then restart the API - it will recreate and seed automatically
```

### Tests fail after running ContractTests
**Solution:** Tests are now isolated - IntegrationTests automatically recreate their database. If issues persist:
```powershell
# Recreate test database
docker-compose -f docker/docker-compose.test.yml down -v
docker-compose -f docker/docker-compose.test.yml up -d
```

### Port already in use
**Solution:** Stop existing PostgreSQL instances:
```powershell
# Check what's using the port
netstat -ano | findstr :5432

# Stop Docker containers
docker-compose -f docker/docker-compose.dev.yml down
```

### Migration conflicts
**Solution:** Reset migration history:
```powershell
dotnet ef database drop --project src\CPR.Infrastructure --startup-project src\CPR.Api --force
dotnet ef database update --project src\CPR.Infrastructure --startup-project src\CPR.Api
```

## Additional Documentation

- **[Conventions](documents/conventions.md)** - Coding standards and best practices
- **[API Endpoints](documents/endpoints.md)** - Detailed endpoint documentation
- **[Data Model](documents/data.md)** - Database schema and relationships
- **[User Stories](documents/stories.md)** - Feature requirements

## License

Internal project - All rights reserved

4) Call `/me` from PowerShell (no Swagger):

```powershell
# assumes $token variable is available or re-run generation snippet
Invoke-RestMethod -Uri http://localhost:5000/me -Headers @{ Authorization = "Bearer $token" } -Method Get
```

## Running integration tests locally with Postgres

A minimal test Postgres instance is provided in `docker-compose.test.yml`.

Start the database:

```powershell
docker-compose -f docker-compose.test.yml up -d
```

Set the `DATABASE_URL` environment variable for the test run (PowerShell):

```powershell
$env:DATABASE_URL = "Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres"
```

Run the integration tests:

```powershell
dotnet test tests/CPR.IntegrationTests/CPR.IntegrationTests.csproj
```

When finished, bring down the test database:

```powershell
docker-compose -f docker-compose.test.yml down
```
OpenAPI & client generation
---------------------------
You can generate client SDKs from the API OpenAPI document exposed at `/swagger/v1/swagger.json`.

1) Run the API locally (PowerShell):

```powershell
$env:JWT_SIGNING_KEY = 'test-signing-key-12345'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src\CPR.Api --urls "http://localhost:5000"
```

2) Download the OpenAPI JSON (PowerShell):

```powershell
# fetch swagger.json
Invoke-WebRequest -Uri http://localhost:5000/swagger/v1/swagger.json -OutFile .\swagger.json
```

3) Generate a C# client using NSwag (recommended):

```powershell
# install once
dotnet tool install --global NSwag.ConsoleCore
# generate a single-file C# client
nswag openapi2csclient /input:swagger.json /output:src\clients\CprApiClient.cs /namespace:CprApi.Client
```

Usage (example after generating `CprApiClient.cs`):

```csharp
var http = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var client = new CprApi.Client.CprApiClient(http);
var positions = await client.GetPositionsAsync();
```

4) Generate TypeScript (optional) with OpenAPI Generator (requires Java):

```powershell
# generate TypeScript fetch client (openapi-generator-cli required)
java -jar openapi-generator-cli.jar generate -i swagger.json -g typescript-fetch -o sdk/typescript
```

Publishing / CI
- You can generate SDKs in CI and publish to an internal NuGet/NPM feed. Save `swagger.json` as a pipeline artifact or fetch `$GITHUB_SERVER_URL/$GITHUB_REPOSITORY/actions/runs/...` in GitHub Actions, then run the above generator commands.

Notes
- The Swagger UI in development will include example responses for taxonomy endpoints (career, career_track, positions).
- If you prefer a different generator (AutoRest, NSwag, openapi-generator), choose the language generator that fits your release workflow.

## API Security & Validation Features

This API includes comprehensive security and validation features to ensure data integrity and prevent common web vulnerabilities:

### Input Validation
- **Model Validation**: All endpoints use DataAnnotations for request validation
- **Custom Validators**: Business rule validation (e.g., preventing self-feedback)
- **Range Validation**: Numeric fields validated within acceptable ranges
- **String Length Validation**: Text fields validated for minimum/maximum lengths

### Input Sanitization
- **HTML Tag Removal**: Automatic stripping of HTML tags from user input
- **Script Filtering**: Prevention of JavaScript injection attacks
- **SQL Injection Prevention**: Filtering of suspicious SQL keywords
- **Content Validation**: Special character ratio validation to prevent spam/malicious content

### Error Handling
- **RFC7807 Compliance**: All errors follow Problem Details format for consistent API responses
- **Detailed Validation Messages**: Clear, actionable error messages for validation failures
- **Structured Error Responses**: Machine-readable error format for better client integration

### Security Best Practices
- **Authentication Required**: JWT Bearer token authentication for all protected endpoints
- **Authorization**: Role-based access control for different user types (Employee, Manager, HR, Admin)
- **Input Sanitization**: Automatic cleaning of user-generated content
- **Validation Layers**: Multiple validation layers (model, business logic, data integrity)

### Example Validation Error Response
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation failed",
  "detail": "One or more validation errors occurred",
  "status": 400,
  "instance": "/api/feedback",
  "errors": {
    "Content": ["Feedback content must be between 10 and 2000 characters"],
    "Rating": ["Rating must be between 1 and 5"]
  }
}
```

For detailed API endpoint documentation including validation requirements, see `endpoints.md`.

Notes
- The stub signing key stored in the `JWT_SIGNING_KEY` environment variable is treated as a plain UTF-8 string (do not base64-decode it).
- This auth stub is for local development and testing only. Do not use it in production.

Scripts
-------
This repo includes convenience scripts in the `scripts/` folder for running tests and starting the API with a generated token.

Windows (PowerShell):

```powershell
# run all tests (unit, contract, integration)
.\scripts\run-tests.cmd

# start API as employee (token copied to clipboard)
.\scripts\run-api-as-employee.cmd
```

Unix / CI (bash):

```bash
# make sure script is executable once on your machine or CI job
chmod +x ./scripts/run-tests.sh
./scripts/run-tests.sh    # default: runs unit, contract, integration
./scripts/run-tests.sh integration
```

If you need to override the test database name set `DATABASE_NAME` before invoking the scripts.

PowerShell helpers
------------------
Use the included PowerShell scripts on Windows rather than the shell scripts:

```powershell
# generate a token and copy to clipboard
.\scripts\generate-token.ps1 -SigningKey 'test-signing-key-12345' -UserId '00000000-0000-0000-0000-000000000123' -Copy

# run tests (unit, contract, integration)
.\scripts\run-tests.ps1
```

