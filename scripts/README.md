# CPR API Scripts

This folder contains various scripts to run the CPR API with different authentication modes and user contexts, as well as database management utilities.

## Database Management Scripts

### Regenerate Migrations (RECOMMENDED)

When entities change or migrations get out of sync:

```powershell
.\scripts\regenerate-migrations.ps1
```

**What it does:**
- Removes all existing migrations
- Generates fresh migration from current entity model
- Drops and recreates database
- Applies new migration
- ⚠️ **All data will be lost!**

**Use when:**
- Entity definitions have changed
- Model snapshot is out of sync
- Setting up clean development environment
- Migrations are corrupted

### Reset Database

Reset database without regenerating migrations:

```powershell
.\scripts\reset-database.ps1
```

**What it does:**
- Drops existing database
- Creates new database
- Applies existing migrations
- ⚠️ **All data will be lost!**

**Use when:**
- Database schema is corrupted
- Need clean slate with existing migrations
- Testing migration scripts

### Verify Database Schema

Check if database matches EF Core model:

```powershell
.\scripts\verify-database-schema.ps1
```

**What it does:**
- Lists columns in feedback_requests table
- Lists columns in feedback_request_recipients table
- Checks for incorrect columns (e.g., employee_id in feedback_requests)
- Shows applied migrations

**Use when:**
- Debugging schema mismatches
- After manual database changes
- Before regenerating migrations

---

## Authentication Modes & Environments

The CPR API supports multiple environments with different authentication modes:

### 1. **Test Environment** (Integration Testing)
- **Configuration**: `appsettings.Test.json`
- **Authentication**: Stub Mode (HMAC-signed tokens)
- **Database**: Local PostgreSQL on port 5433
- **Usage**: Automated tests, CI/CD pipelines
- **Environment Variable**: `ASPNETCORE_ENVIRONMENT=Test`

### 2. **Development Environment** (Local Development) 
- **Configuration**: `appsettings.Development.json`
- **Authentication**: EntraExternalId Mode (Real Azure AD)
- **Database**: Local PostgreSQL on port 5432
- **Usage**: Local development with real authentication
- **Environment Variable**: `ASPNETCORE_ENVIRONMENT=Development`

### 3. **QA Environment**
- **Configuration**: `appsettings.QA.json`
- **Authentication**: EntraExternalId Mode (QA tenant)
- **Database**: QA PostgreSQL server
- **Environment Variable**: `ASPNETCORE_ENVIRONMENT=QA`

### 4. **Production Environment**
- **Configuration**: `appsettings.Production.json`
- **Authentication**: EntraExternalId Mode (Production tenant)
- **Database**: Production PostgreSQL server
- **Environment Variable**: `ASPNETCORE_ENVIRONMENT=Production`

---

## Quick Start Scripts

### Run with Stub Authentication (Development)

Use these scripts for local development when you need quick access with predefined user roles:

```bash
# Run as Employee (Eve Adams - Director of Security Engineering)
scripts\run-api-as-employee.cmd

# Run as Manager (Frank Turner - VP of Cloud Services)
scripts\run-api-as-manager.cmd

# Run as Administrator (Alice Smith - CEO)
scripts\run-api-as-administrator.cmd

# Run as Solution Owner (Dan Martinez - Principal Software Architect)
scripts\run-api-as-solution-owner.cmd
```

**What these scripts do:**
1. Generate a stub JWT token for the specified user
2. Copy the token to clipboard
3. Set `ASPNETCORE_ENVIRONMENT=Test` to use Stub authentication
4. Start the API at `http://localhost:5000`

**How to use:**
1. Run the script
2. Token is automatically copied to your clipboard
3. Use the token in API requests: `Authorization: Bearer <token>`
4. Or test in Swagger UI at `http://localhost:5000/swagger`

---

### Run with Entra External ID Authentication (Production-like)

Use this script when testing integration with the UI application or production-like scenarios:

```bash
# Run with Microsoft Entra External ID authentication
scripts\run-api.cmd
```

**What this script does:**
1. Sets `ASPNETCORE_ENVIRONMENT=Development` to use EntraExternalId authentication
2. Configures API to validate real JWT tokens from Microsoft Entra
3. Starts the API at `http://localhost:5000`

**How to use:**
1. Run the script
2. UI application must authenticate users with Microsoft Entra
3. UI provides bearer token in API requests
4. API validates token signature using Entra's public keys

**Note:** This mode requires:
- Microsoft Entra External ID tenant configured
- UI application set up to authenticate users
- Valid JWT tokens from Entra (stub tokens will be rejected)

---

## Script Details

### Stub Authentication Scripts

| Script | User Role | User Name | User ID |
|--------|-----------|-----------|---------|
| `run-api-as-employee.cmd` | Employee | Eve Adams | `c7746e91-a5e8-4f8b-9f22-f48374ffa2a4` |
| `run-api-as-manager.cmd` | Manager | Frank Turner | `fb8e4e92-b6f9-4c9c-a033-062495ffc3a5` |
| `run-api-as-administrator.cmd` | Administrator | Alice Smith | `11111111-1111-1111-1111-111111111111` |
| `run-api-as-solution-owner.cmd` | Solution Owner | Dan Martinez | `d8a8f8a3-c7e6-4d9f-b1aa-273606ddc4a6` |

### Supporting Scripts

- **`run-api-with-token.ps1`**: PowerShell script that generates stub tokens and runs the API
  - Used internally by `run-api-as-*.cmd` scripts
  - Can be called directly: `powershell -File run-api-with-token.ps1 -UserId <guid>`

- **`generate-token.ps1`**: Generates a stub token without running the API
  - Usage: `powershell -File generate-token.ps1 -UserId <guid>`
  - Token is copied to clipboard

---

## Environment Variables

The scripts set the following environment variables:

### Stub Mode
```bash
AUTHENTICATION_MODE=Stub
JWT_SIGNING_KEY=test-signing-key-12345
ASPNETCORE_URLS=http://localhost:5000
```

### EntraExternalId Mode
```bash
AUTHENTICATION_MODE=EntraExternalId
ASPNETCORE_URLS=http://localhost:5000
```

---

## Switching Between Modes

You can easily switch between authentication modes:

1. **Stop the current API instance** (Ctrl+C in terminal)
2. **Run the desired script**:
   - Stub mode: `run-api-as-employee.cmd` (or other user roles)
   - Entra mode: `run-api.cmd`

**No code changes needed!** The API automatically detects the authentication mode from environment variables.

---

## Testing with Swagger UI

1. Start the API with your preferred script
2. Open `http://localhost:5000/swagger` in browser
3. Click **Authorize** button (top right)
4. Paste the token (from clipboard if using stub mode)
5. Click **Authorize** button in dialog
6. Test endpoints with the authenticated user

---

## Troubleshooting

### Stub Token Not Working
- Ensure you're using `run-api-as-*.cmd` scripts (they set `AUTHENTICATION_MODE=Stub`)
- Check that token is correctly copied to clipboard
- Verify token format: `<user-id>.<base64-signature>`

### Entra Token Not Working
- Ensure you're using `run-api.cmd` script (sets `AUTHENTICATION_MODE=EntraExternalId`)
- Verify UI application is providing valid Entra JWT tokens
- Check Azure AD configuration in `launchSettings.json`
- Check Entra tenant and application client ID are correct

### API Won't Start
- Ensure port 5000 is not already in use
- Check that .NET 9 SDK is installed
- Verify PostgreSQL database is running (see `docker-compose.dev.yml`)

---

## Related Documentation

- **Authentication Setup**: See `documents/iteration-16-azure-ad-b2c-auth-plan.md`
- **API Endpoints**: See `documents/endpoints.md`
- **Environment Configuration**: See `.env.dev` and `.env.test`

---

## Summary

**For quick local development**: Use `run-api-as-employee.cmd` (or other roles)  
**For production-like testing**: Use `run-api.cmd` with UI application

Both modes work seamlessly without code changes! 🚀
