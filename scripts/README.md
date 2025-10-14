# CPR API Scripts

This folder contains various scripts to run the CPR API with different authentication modes and user contexts.

## Authentication Modes

The CPR API supports two authentication modes:

### 1. **Stub Mode** (Development/Testing)
- Uses HMAC-signed tokens for quick local development
- No external dependencies (Microsoft Entra not required)
- Fast iteration during development
- Perfect for automated testing and CI/CD

### 2. **EntraExternalId Mode** (Production)
- Uses real JWT tokens from Microsoft Entra External ID
- Production-like authentication
- Requires UI application to provide bearer tokens
- Recommended for integration testing with UI

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
3. Set `AUTHENTICATION_MODE=Stub` environment variable
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
1. Sets `AUTHENTICATION_MODE=EntraExternalId` environment variable
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
