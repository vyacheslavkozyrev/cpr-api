# Iteration 16 — Microsoft Entra External ID Authentication Implementation Plan

**Status**: Planning Phase  
**Created**: October 13, 2025  
**Updated**: October 13, 2025 - Migrated from Azure AD B2C to Microsoft Entra External ID  
**Objective**: Replace mock HMAC token authentication with Microsoft Entra External ID for production-grade authentication

---

## Executive Summary

This iteration will replace the current HMAC-signed stub authentication system with **Microsoft Entra External ID** (formerly Azure AD B2C) identity service. The implementation will provide:

- Real user authentication with secure token management
- User account lifecycle management (sign-up, sign-in, sign-out, password reset)
- Integration with Microsoft Entra External ID for user identity storage
- Separation of identity management (Microsoft Entra External ID) from application data (PostgreSQL)
- JWT token validation with Microsoft Entra public keys
- Backward compatibility during migration period
- Modern identity platform with enhanced security features

---

## Current State Analysis

### Existing Authentication System

**Components**:
1. **JwtStubAuthenticationHandler.cs** - HMAC-signed token validation
2. **TokenGenerator.cs** - Generates tokens for development/testing
3. **User Entity** - Contains `PasswordHash` field (to be removed)
4. **No authentication endpoints** - No sign-in, sign-out, or account management APIs

**Token Format**: `{userId}.{base64Signature}`
- Signature: `HMACSHA256(JWT_SIGNING_KEY, userId)`
- Simple but not production-ready
- No expiration, no refresh, no revocation

**Database Schema** (users table):
```sql
- id (UUID)
- user_name (string)
- password_hash (string) -- TO BE REMOVED
- display_name (string)
- created_by, created_at, modified_by, modified_at, is_deleted, deleted_by, deleted_at
```

### Required Changes

**User Entity** (CPR.Domain.Entities.User):
```csharp
public class User : AuditableEntity
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? EntraExternalId { get; set; }  // NEW: Microsoft Entra External ID user object ID
    // REMOVE: public string PasswordHash { get; set; } = null!;
}
```

---

## Microsoft Entra External ID Overview

### What is Microsoft Entra External ID?

Microsoft Entra External ID (formerly Azure AD B2C) is Microsoft's modern cloud identity service for customer-facing applications that:
- Hosts external user accounts and authentication flows
- Provides customizable sign-up, sign-in, password reset, and profile editing experiences
- Issues secure JWT tokens signed with Microsoft's keys
- Supports social identity providers (Google, Facebook, Microsoft, etc.)
- Provides MFA, conditional access, password policies, and advanced security features
- Scales to millions of users with enterprise-grade reliability
- Part of the Microsoft Entra family (unified identity platform)
- Built on modern standards (OAuth 2.0, OpenID Connect, SAML)

### Authentication Flow

```
1. User clicks "Sign In" → Redirected to Microsoft Entra External ID login page
2. User enters credentials → Microsoft Entra validates
3. Microsoft Entra issues JWT token → Returns to application
4. Application validates JWT signature with Microsoft Entra public keys
5. Application extracts claims (user ID, email, roles)
6. Application looks up local user record by EntraExternalId
7. Application processes request with authenticated user context
```

---

## Implementation Phases

### Phase 1: Microsoft Entra External ID Setup & Configuration

**Duration**: 1 day  
**Owner**: DevOps/Infrastructure

#### Tasks

1. **Create Microsoft Entra External ID Tenant**
   - Navigate to Azure Portal → Microsoft Entra admin center
   - Create new External ID tenant (customer identity solution)
   - Name: `cpr-{environment}` (e.g., `cpr-dev`, `cpr-staging`, `cpr-prod`)
   - Select region (e.g., United States, Europe)
   - Choose tenant type: **External ID (CIAM)** for customer scenarios

2. **Register Application**
   - Create App Registration in Microsoft Entra External ID
   - Name: `CPR API`
   - Supported account types: Accounts in this organizational directory only
   - Platform: Web application
   - Redirect URIs: 
     - `http://localhost:5000/signin-oidc` (local dev)
     - `http://localhost:5000/api/auth/signin-oidc` (local dev)
     - `https://api-dev.cpr.company.com/signin-oidc` (dev)
     - `https://api-staging.cpr.company.com/signin-oidc` (staging)
     - `https://api.cpr.company.com/signin-oidc` (production)
   - Record: **Application (client) ID**, **Tenant ID**, **Tenant Name**, **Authority URL**

3. **Configure User Flows / Sign-up and Sign-in Experience**
   - Create custom branding (optional)
   - Configure sign-up attributes: Email, Display Name
   - Configure token claims: Email, Display Name, Object ID (oid)
   - Enable self-service sign-up
   - Configure password policy and complexity requirements
   - Enable password reset flow
   - Optional: Enable social identity providers (Google, Microsoft, Facebook)

4. **Configure API Permissions**
   - Expose API scope: `API.Access`
   - Grant admin consent for Microsoft Graph permissions:
     - `User.Read` - Read user profile
     - `openid` - OpenID Connect sign-in
     - `profile` - Read user's basic profile
     - `email` - Read user's email address
     - `offline_access` - Refresh token support (optional)

5. **Record Configuration Values**
   ```
   ENTRA_EXTERNAL_INSTANCE=https://{tenant-name}.ciamlogin.com
   ENTRA_EXTERNAL_TENANT_ID={tenant-guid}
   ENTRA_EXTERNAL_CLIENT_ID={application-client-id}
   ENTRA_EXTERNAL_DOMAIN={tenant-name}.onmicrosoft.com
   ENTRA_EXTERNAL_AUTHORITY=https://{tenant-name}.ciamlogin.com/{tenant-guid}
   ```

#### Deliverables
- Microsoft Entra External ID tenant created and configured
- Sign-up and sign-in flows tested manually
- Configuration values documented in secure vault
- Test users created for development

---

### Phase 2: Database Schema Migration

**Duration**: 0.5 days  
**Dependencies**: None (can run in parallel with Phase 1)

#### Tasks

1. **Create Migration: AddEntraExternalId**
   ```bash
   dotnet ef migrations add AddEntraExternalId --project src/CPR.Infrastructure --startup-project src/CPR.Api
   ```

2. **Migration Content**:
   ```csharp
   // Up
   migrationBuilder.AddColumn<string>(
       name: "entra_external_id",
       table: "users",
       type: "character varying(100)",
       maxLength: 100,
       nullable: true);

   migrationBuilder.CreateIndex(
       name: "ix_users_entra_external_id",
       table: "users",
       column: "entra_external_id",
       unique: true,
       filter: "entra_external_id IS NOT NULL");
   
   // Down
   migrationBuilder.DropIndex(name: "ix_users_entra_external_id", table: "users");
   migrationBuilder.DropColumn(name: "entra_external_id", table: "users");
   ```

3. **Update User Entity** (`CPR.Domain.Entities.User.cs`):
   ```csharp
   public string? EntraExternalId { get; set; }  // Microsoft Entra External ID user object ID (oid claim)
   ```

4. **Update DbContext Configuration**:
   ```csharp
   // In CprDbContext.OnModelCreating
   modelBuilder.Entity<User>(b =>
   {
       // Existing mappings...
       b.Property(u => u.EntraExternalId)
           .HasColumnName("entra_external_id")
           .HasMaxLength(100);
       
       b.HasIndex(u => u.EntraExternalId)
           .IsUnique()
           .HasFilter("entra_external_id IS NOT NULL");
   });
   ```

5. **Create Migration: RemovePasswordHash** (Separate migration for safety)
   ```bash
   dotnet ef migrations add RemovePasswordHash --project src/CPR.Infrastructure --startup-project src/CPR.Api
   ```

   **Note**: This migration will be applied AFTER Azure AD B2C authentication is fully implemented and tested.

   ```csharp
   // Up
   migrationBuilder.DropColumn(name: "password_hash", table: "users");
   
   // Down
   migrationBuilder.AddColumn<string>(
       name: "password_hash",
       table: "users",
       type: "text",
       nullable: false,
       defaultValue: "");
   ```

#### Deliverables
- Two migrations created (AddAzureAdB2CObjectId, RemovePasswordHash)
- User entity updated
- DbContext configuration updated
- Migration tested locally with up/down

---

### Phase 3: Authentication Infrastructure

**Duration**: 2 days  
**Dependencies**: Phase 1 (Azure AD B2C tenant), Phase 2 (schema migration)

#### Tasks

1. **Install NuGet Packages**
   ```bash
   # In CPR.Api project
   dotnet add package Microsoft.Identity.Web --version 2.15.0
   dotnet add package Microsoft.Identity.Web.MicrosoftGraph --version 2.15.0
   ```

2. **Update appsettings.json Structure**
   ```json
   {
     "AzureAdB2C": {
       "Instance": "https://{tenant-name}.b2clogin.com",
       "Domain": "{tenant-name}.onmicrosoft.com",
       "TenantId": "{tenant-guid}",
       "ClientId": "{application-client-id}",
       "SignUpSignInPolicyId": "B2C_1_signup_signin",
       "ResetPasswordPolicyId": "B2C_1_password_reset",
       "CallbackPath": "/signin-oidc"
     },
     "Logging": { ... }
   }
   ```

3. **Update Environment Variable Mappings**
   - `.env.dev`, `.env.test`, `.env.staging`, `.env.prod`:
   ```env
   # Azure AD B2C Configuration
   AZURE_AD_B2C_INSTANCE=https://{tenant-name}.b2clogin.com
   AZURE_AD_B2C_DOMAIN={tenant-name}.onmicrosoft.com
   AZURE_AD_B2C_TENANT_ID={tenant-guid}
   AZURE_AD_B2C_CLIENT_ID={application-client-id}
   AZURE_AD_B2C_SUSI_POLICY=B2C_1_signup_signin
   AZURE_AD_B2C_RESET_POLICY=B2C_1_password_reset
   
   # Keep for backward compatibility during migration
   JWT_SIGNING_KEY=local-test-key
   ```

4. **Create AzureAdB2CAuthenticationHandler.cs**
   ```csharp
   namespace CPR.Api.Auth
   {
       public class AzureAdB2CAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
       {
           // Validates JWT tokens issued by Azure AD B2C
           // Extracts claims: oid (object ID), email, name
           // Looks up local User by AzureAdB2CObjectId
           // Enriches ClaimsPrincipal with application-specific claims (employeeId, roles)
       }
   }
   ```

5. **Create UserSyncService.cs** (Infrastructure)
   ```csharp
   namespace CPR.Infrastructure.Services
   {
       public interface IUserSyncService
       {
           Task<User> SyncUserFromB2CAsync(string azureAdB2CObjectId, string email, string displayName);
       }
       
       public class UserSyncService : IUserSyncService
       {
           // Creates or updates local User record
           // Maps Azure AD B2C object ID to local User.Id
           // Updates DisplayName if changed in Azure AD B2C
       }
   }
   ```

6. **Update Program.cs - Replace Stub Authentication**
   ```csharp
   // Remove old stub authentication
   // builder.Services.AddAuthentication(options => { ... }).AddScheme<...JwtStubAuthenticationHandler>...
   
   // Add Azure AD B2C authentication
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
   
   builder.Services.AddAuthorization();
   
   // Register user sync service
   builder.Services.AddScoped<IUserSyncService, UserSyncService>();
   ```

7. **Update RoleAuthorizationHandler.cs**
   - Modify to work with Azure AD B2C claims (use `oid` claim for user identification)
   - Continue using local database for role assignments

8. **Create Feature Flag for Authentication Mode**
   ```env
   # Allow switching between stub and Azure AD B2C during migration
   AUTHENTICATION_MODE=AzureAdB2C  # or "Stub" for backward compatibility
   ```

   ```csharp
   // Program.cs
   var authMode = Environment.GetEnvironmentVariable("AUTHENTICATION_MODE") ?? "AzureAdB2C";
   
   if (authMode == "Stub")
   {
       // Use JwtStubAuthenticationHandler (for testing)
   }
   else
   {
       // Use Azure AD B2C authentication
   }
   ```

#### Deliverables
- Azure AD B2C authentication handler implemented
- User sync service implemented
- Program.cs updated with new authentication
- Feature flag for authentication mode
- Backward compatibility maintained

---

### Phase 4: Authentication Endpoints (Controllers)

**Duration**: 2 days  
**Dependencies**: Phase 3 (authentication infrastructure)

#### Endpoints to Implement

1. **POST /api/auth/signup**
   - Redirects to Azure AD B2C sign-up flow
   - Parameters: `returnUrl` (optional)
   - Returns: Redirect URL to Azure AD B2C SUSI policy

2. **POST /api/auth/signin**
   - Redirects to Azure AD B2C sign-in flow
   - Parameters: `returnUrl` (optional)
   - Returns: Redirect URL to Azure AD B2C SUSI policy

3. **GET /api/auth/signin-oidc** (Callback endpoint)
   - Receives authorization code from Azure AD B2C
   - Exchanges code for JWT token
   - Syncs user to local database
   - Returns JWT token to client
   - **Note**: Typically handled by Microsoft.Identity.Web middleware

4. **POST /api/auth/signout**
   - Signs out user from application session
   - Redirects to Azure AD B2C sign-out endpoint
   - Parameters: `returnUrl` (optional)
   - Returns: Success message

5. **POST /api/auth/reset-password**
   - Redirects to Azure AD B2C password reset flow
   - Parameters: `email`, `returnUrl` (optional)
   - Returns: Redirect URL to Azure AD B2C password reset policy

6. **GET /api/auth/me** (Enhanced existing /api/me endpoint)
   - Returns current user profile with Azure AD B2C information
   - Response:
     ```json
     {
       "id": "guid",
       "userName": "string",
       "displayName": "string",
       "email": "string",
       "azureAdB2CObjectId": "string",
       "employeeId": "guid",
       "roles": ["Employee", "Manager"]
     }
     ```

#### Tasks

1. **Create AuthController.cs**
   ```csharp
   namespace CPR.Api.Controllers
   {
       [ApiController]
       [Route("api/[controller]")]
       public class AuthController : ControllerBase
       {
           private readonly IUserSyncService _userSyncService;
           private readonly IConfiguration _configuration;
           
           // Implement signup, signin, signout, reset-password endpoints
       }
   }
   ```

2. **Create AuthDtos.cs** (CPR.Application.Contracts)
   ```csharp
   public class SignInRequestDto
   {
       public string? ReturnUrl { get; set; }
   }
   
   public class SignInResponseDto
   {
       public string RedirectUrl { get; set; } = null!;
   }
   
   public class SignOutRequestDto
   {
       public string? ReturnUrl { get; set; }
   }
   
   public class ResetPasswordRequestDto
   {
       [Required]
       [EmailAddress]
       public string Email { get; set; } = null!;
       public string? ReturnUrl { get; set; }
   }
   
   public class AuthenticatedUserDto
   {
       public Guid Id { get; set; }
       public string UserName { get; set; } = null!;
       public string? DisplayName { get; set; }
       public string? Email { get; set; }
       public string? AzureAdB2CObjectId { get; set; }
       public Guid? EmployeeId { get; set; }
       public List<string> Roles { get; set; } = new();
   }
   ```

3. **Update MeController.cs**
   - Add Azure AD B2C object ID to response
   - Include email from JWT claims

4. **Implement Token Refresh Logic** (Optional for Iteration 16)
   - Azure AD B2C tokens expire (typically 1 hour)
   - Refresh token flow for seamless re-authentication
   - **Defer to Iteration 17** if time-constrained

#### Deliverables
- AuthController with 5 endpoints implemented
- Auth DTOs created
- MeController updated with Azure AD B2C fields
- Swagger documentation updated

---

### Phase 5: Testing Strategy

**Duration**: 2 days  
**Dependencies**: Phase 4 (endpoints implemented)

#### Test Categories

1. **Unit Tests** (`CPR.UnitTests/AuthControllerTests.cs`)
   - Test redirect URL generation for sign-in, sign-up, password reset
   - Test user sync service (create new user, update existing user)
   - Test claim extraction from JWT tokens
   - Mock Azure AD B2C responses

2. **Integration Tests** (`CPR.IntegrationTests/AuthIntegrationTests.cs`)
   - Test sign-in flow with mock Azure AD B2C tokens
   - Test user creation/sync after successful authentication
   - Test role assignment after user sync
   - Test protected endpoints with Azure AD B2C tokens
   - **Note**: Use JWT token generation with Azure AD B2C claim structure

3. **Contract Tests** (`CPR.ContractTests/AuthContractTests.cs`)
   - Validate auth endpoint request/response schemas
   - Validate JWT token structure
   - Validate error responses (401, 403)

4. **Manual Testing Checklist**
   - [ ] Sign up with new user in Azure AD B2C
   - [ ] Sign in with existing user
   - [ ] Password reset flow
   - [ ] Sign out and confirm session cleared
   - [ ] Access protected endpoint with valid token
   - [ ] Access protected endpoint with expired token (401)
   - [ ] Access protected endpoint with invalid token (401)
   - [ ] Verify user synced to local database
   - [ ] Verify roles assigned correctly after sync

#### Test Data Setup

1. **Seed Azure AD B2C Test Users**
   - Create 5-10 test users in Azure AD B2C dev tenant
   - Assign to different roles (Employee, Manager, Administrator)
   - Document credentials in secure test vault

2. **Local Database Test Users**
   - Create corresponding User records with AzureAdB2CObjectId
   - Link to Employee records
   - Assign roles via UserToRole table

#### Deliverables
- 30+ unit tests for authentication logic
- 15+ integration tests for auth flows
- 5+ contract tests for auth endpoints
- Manual testing checklist completed
- Test data setup documented

---

### Phase 6: Migration & Deployment

**Duration**: 1 day  
**Dependencies**: Phase 5 (all tests passing)

#### Migration Tasks

1. **Migrate Existing Users to Azure AD B2C**
   
   **Option A: Manual Migration (Recommended for small user base)**
   - Export existing users from database (id, user_name, display_name)
   - Admins create accounts in Azure AD B2C manually
   - Run SQL update to link Azure AD B2C object IDs:
     ```sql
     UPDATE users 
     SET azure_ad_b2c_object_id = '{azure-object-id}' 
     WHERE id = '{local-user-id}';
     ```
   
   **Option B: Bulk Import (For large user base)**
   - Use Azure AD B2C Graph API to bulk create users
   - Set temporary passwords and send reset email to all users
   - Script to update local database with Azure AD B2C object IDs

2. **Update Seed Data Scripts**
   - Modify `DatabaseSeeder.cs` to create users without `PasswordHash`
   - Set `AzureAdB2CObjectId` to null for seed data
   - Document manual Azure AD B2C account creation for seed users

3. **Apply RemovePasswordHash Migration**
   ```bash
   # After all users have AzureAdB2CObjectId populated
   dotnet ef database update --project src/CPR.Infrastructure --startup-project src/CPR.Api
   ```

4. **Update Documentation**
   - README.md: Replace token generation instructions with Azure AD B2C sign-in
   - endpoints.md: Document new auth endpoints
   - conventions.md: Update authentication section

5. **Update Scripts**
   - Deprecate `run-api-as-employee.cmd`, `run-api-as-manager.cmd`, `run-api-as-administrator.cmd`
   - Create `run-api-with-azure-ad-b2c.cmd` (starts API with Azure AD B2C authentication)
   - Keep old scripts with `AUTHENTICATION_MODE=Stub` for testing

#### Deployment Checklist

- [ ] Azure AD B2C tenant configured in all environments (dev, staging, prod)
- [ ] Environment variables set in Azure App Service / deployment config
- [ ] Database migrations applied (AddAzureAdB2CObjectId)
- [ ] All tests passing (unit, integration, contract)
- [ ] Manual testing completed in dev environment
- [ ] User migration completed
- [ ] RemovePasswordHash migration applied
- [ ] Documentation updated
- [ ] Rollback plan documented

#### Rollback Plan

1. **If issues found in production**:
   - Set `AUTHENTICATION_MODE=Stub` environment variable
   - Redeploy previous version
   - Investigate issues in staging

2. **If database issues**:
   - `RemovePasswordHash` migration has a `Down()` method to restore column
   - Revert to previous code version using stub authentication

#### Deliverables
- Azure AD B2C authentication fully deployed
- All users migrated
- PasswordHash column removed from database
- Documentation updated
- Rollback plan tested

---

## Phase 7: Documentation & Training

**Duration**: 1 day  
**Dependencies**: Phase 6 (deployment complete)

#### Tasks

1. **Update Technical Documentation**
   - Architecture diagram showing Azure AD B2C integration
   - Sequence diagrams for sign-in, sign-up, password reset flows
   - Token validation process documentation
   - User sync service documentation

2. **Update API Documentation**
   - Swagger/OpenAPI specs for auth endpoints
   - Add Azure AD B2C authentication examples
   - Update authentication section in endpoints.md

3. **Create User Guides**
   - "How to Sign Up" guide
   - "How to Sign In" guide
   - "How to Reset Password" guide
   - "Troubleshooting Authentication Issues" guide

4. **Create Developer Guides**
   - "Setting up Azure AD B2C for Local Development"
   - "Testing with Azure AD B2C"
   - "Adding Custom Claims to JWT Tokens"
   - "Debugging Authentication Issues"

5. **Update README.md**
   - Remove HMAC token generation sections
   - Add Azure AD B2C setup instructions
   - Update "Quick Start" section with new authentication flow
   - Update environment variable documentation

#### Deliverables
- Technical documentation complete
- User guides published
- Developer guides published
- README.md updated

---

## Security Considerations

### Token Security

1. **JWT Validation**
   - Validate token signature using Azure AD B2C public keys
   - Validate issuer (Azure AD B2C tenant)
   - Validate audience (application client ID)
   - Validate expiration time (exp claim)
   - Validate not-before time (nbf claim)

2. **Token Storage**
   - Client-side: Store in HttpOnly cookie or secure local storage
   - Never log full JWT tokens (log only token ID or last 4 characters)

3. **Token Expiration**
   - Azure AD B2C default: 1 hour
   - Implement token refresh flow (Phase 4 optional task or Iteration 17)

### User Data Privacy

1. **PII in Azure AD B2C**
   - Email, display name stored in Azure AD B2C
   - Employee data, goals, feedback remain in local database
   - Azure AD B2C object ID is the only link between systems

2. **Audit Logging**
   - Log all authentication events (sign-in, sign-out, password reset)
   - Include timestamp, user ID, IP address, user agent
   - Store in audit_logs table

3. **GDPR Compliance**
   - User can delete account in Azure AD B2C
   - Application must soft-delete User record when Azure AD B2C account deleted
   - Implement webhook from Azure AD B2C for account deletion events (Iteration 17)

### Password Security

1. **Password Policy in Azure AD B2C**
   - Minimum 8 characters
   - Require uppercase, lowercase, number, special character
   - Password expiration: 90 days (configurable)
   - Account lockout after 5 failed attempts

2. **Password Reset**
   - Email verification required
   - Temporary password link expires in 24 hours
   - Cannot reuse last 5 passwords

### Network Security

1. **HTTPS Enforcement**
   - All authentication endpoints require HTTPS in production
   - Redirect HTTP to HTTPS automatically

2. **CORS Configuration**
   - Restrict origins to known client applications
   - No wildcard CORS in production

---

## Testing Scenarios

### Functional Testing

1. **Sign Up Flow**
   - User clicks "Sign Up"
   - Redirected to Azure AD B2C sign-up page
   - Enters email, display name, password
   - Receives verification email
   - Verifies email and completes sign-up
   - Redirected back to application with JWT token
   - User record created in local database with AzureAdB2CObjectId

2. **Sign In Flow**
   - User clicks "Sign In"
   - Redirected to Azure AD B2C sign-in page
   - Enters email and password
   - Successful authentication
   - Redirected back to application with JWT token
   - User record synced (display name updated if changed)

3. **Password Reset Flow**
   - User clicks "Forgot Password"
   - Enters email address
   - Redirected to Azure AD B2C password reset page
   - Receives password reset email
   - Clicks link and sets new password
   - Redirected back to application
   - Can sign in with new password

4. **Sign Out Flow**
   - User clicks "Sign Out"
   - Application clears session
   - User redirected to Azure AD B2C sign-out endpoint
   - Azure AD B2C clears session
   - User redirected back to application

### Security Testing

1. **Invalid Token Validation**
   - [ ] Expired token returns 401
   - [ ] Invalid signature returns 401
   - [ ] Wrong audience returns 401
   - [ ] Wrong issuer returns 401
   - [ ] Malformed token returns 401

2. **Authorization Testing**
   - [ ] Employee role can access employee endpoints
   - [ ] Manager role can access manager endpoints
   - [ ] Administrator role can access admin endpoints
   - [ ] Non-authenticated user gets 401 on protected endpoints
   - [ ] Wrong role gets 403 on restricted endpoints

3. **Session Security**
   - [ ] Multiple concurrent sessions handled correctly
   - [ ] Session timeout after inactivity (if implemented)
   - [ ] Sign out invalidates session

### Performance Testing

1. **Token Validation Performance**
   - Measure time to validate JWT token
   - Target: < 50ms per request
   - Use Azure AD B2C public key caching

2. **User Sync Performance**
   - Measure time to sync user on first sign-in
   - Target: < 200ms
   - Optimize database queries

---

## Risks & Mitigations

### Risk 1: Azure AD B2C Configuration Errors

**Impact**: High - Authentication will not work  
**Probability**: Medium

**Mitigation**:
- Follow Microsoft documentation step-by-step
- Test in dev environment before staging/production
- Use Infrastructure as Code (ARM templates or Terraform) for reproducible setup
- Document configuration values in secure vault

### Risk 2: User Migration Data Loss

**Impact**: High - Users cannot sign in  
**Probability**: Low

**Mitigation**:
- Backup database before migration
- Test migration process in dev environment
- Maintain PasswordHash column until migration complete (two-phase migration)
- Implement rollback plan

### Risk 3: Token Validation Performance Issues

**Impact**: Medium - Slow API response times  
**Probability**: Low

**Mitigation**:
- Cache Azure AD B2C public keys (Microsoft.Identity.Web does this automatically)
- Monitor token validation latency
- Implement circuit breaker for Azure AD B2C calls

### Risk 4: Breaking Changes for Existing Clients

**Impact**: High - Client applications stop working  
**Probability**: Medium

**Mitigation**:
- Feature flag for authentication mode (Stub vs Azure AD B2C)
- Maintain backward compatibility during migration
- Coordinate with client application teams
- Versioned API endpoints if needed

### Risk 5: Azure AD B2C Service Outage

**Impact**: High - No users can authenticate  
**Probability**: Very Low (Azure SLA: 99.9%)

**Mitigation**:
- Monitor Azure AD B2C service health
- Implement fallback authentication mode (if feasible)
- Cache valid tokens locally with longer TTL
- Document incident response plan

---

## Success Criteria

### Functional Success

- [ ] Users can sign up for new accounts via Azure AD B2C
- [ ] Users can sign in with email and password
- [ ] Users can reset forgotten passwords
- [ ] Users can sign out and session is cleared
- [ ] All protected endpoints validate Azure AD B2C JWT tokens
- [ ] User records synced to local database with AzureAdB2CObjectId
- [ ] Roles assigned correctly after user sync
- [ ] PasswordHash column removed from database

### Technical Success

- [ ] All unit tests passing (target: 30+ new tests)
- [ ] All integration tests passing (target: 15+ new tests)
- [ ] All contract tests passing (target: 5+ new tests)
- [ ] No regression in existing functionality
- [ ] API response time < 200ms (95th percentile)
- [ ] Token validation time < 50ms (95th percentile)
- [ ] Zero critical security vulnerabilities
- [ ] Code coverage > 80% for new authentication code

### Documentation Success

- [ ] Technical documentation complete and reviewed
- [ ] User guides published and accessible
- [ ] Developer guides published
- [ ] README.md updated with Azure AD B2C instructions
- [ ] API documentation (Swagger) updated
- [ ] Architecture diagrams created

### Operational Success

- [ ] Azure AD B2C configured in dev, staging, production
- [ ] All environment variables set correctly
- [ ] Monitoring and alerting configured
- [ ] Incident response plan documented
- [ ] Rollback plan tested
- [ ] User migration completed successfully

---

## Timeline & Resource Allocation

### Estimated Timeline: 8-10 working days

| Phase | Duration | Start Date | End Date | Owner |
|-------|----------|------------|----------|-------|
| Phase 1: Azure AD B2C Setup | 1 day | Day 1 | Day 1 | DevOps |
| Phase 2: Database Migration | 0.5 days | Day 1 | Day 1 | Backend Dev |
| Phase 3: Auth Infrastructure | 2 days | Day 2 | Day 3 | Backend Dev |
| Phase 4: Auth Endpoints | 2 days | Day 4 | Day 5 | Backend Dev |
| Phase 5: Testing | 2 days | Day 6 | Day 7 | QA/Backend Dev |
| Phase 6: Migration & Deployment | 1 day | Day 8 | Day 8 | DevOps/Backend Dev |
| Phase 7: Documentation | 1 day | Day 9 | Day 9 | Tech Writer/Backend Dev |
| Buffer for Issues | 0.5 days | Day 10 | Day 10 | All |

### Resource Requirements

- **Backend Developer**: 8 days full-time
- **DevOps Engineer**: 2 days (Phase 1, Phase 6)
- **QA Engineer**: 2 days (Phase 5 testing)
- **Technical Writer**: 1 day (Phase 7 documentation)

---

## Out of Scope (Deferred to Future Iterations)

The following features are NOT included in Iteration 16:

1. **Token Refresh Flow**
   - Automatic token refresh before expiration
   - Refresh token storage and management
   - **Defer to**: Iteration 17

2. **Multi-Factor Authentication (MFA)**
   - SMS or authenticator app verification
   - **Defer to**: Iteration 18

3. **Social Identity Providers**
   - Sign in with Google, Facebook, Microsoft Account
   - **Defer to**: Iteration 19

4. **Profile Editing Endpoint**
   - User can update display name, email
   - **Defer to**: Iteration 17

5. **Account Deletion**
   - User can delete their Azure AD B2C account
   - Webhook to soft-delete local User record
   - **Defer to**: Iteration 18

6. **Advanced Token Features**
   - Custom claims from local database in JWT
   - Token revocation list
   - **Defer to**: Iteration 19

7. **Passwordless Authentication**
   - Email magic links
   - WebAuthn/FIDO2
   - **Defer to**: Iteration 20

---

## Dependencies

### External Dependencies

1. **Azure Subscription**: Required to create Azure AD B2C tenant
2. **Azure AD B2C Tenant**: Must be provisioned before Phase 3
3. **NuGet Packages**: Microsoft.Identity.Web (stable version 2.15.0+)

### Internal Dependencies

1. **User Entity**: Must support AzureAdB2CObjectId field
2. **Database**: Must support unique index on AzureAdB2CObjectId
3. **RoleAuthorizationHandler**: Must work with Azure AD B2C claims
4. **Existing Tests**: Must pass after authentication changes

---

## Post-Implementation Monitoring

### Metrics to Track

1. **Authentication Success Rate**
   - Target: > 99%
   - Alert if < 95%

2. **Token Validation Latency**
   - Target: < 50ms (P95)
   - Alert if > 100ms

3. **User Sync Latency**
   - Target: < 200ms (P95)
   - Alert if > 500ms

4. **Failed Authentication Attempts**
   - Track count per user
   - Alert on suspicious patterns (brute force)

5. **Azure AD B2C API Call Volume**
   - Monitor for rate limiting issues
   - Track costs (Azure AD B2C charges per MAU)

### Logging

1. **Authentication Events**
   - Log all sign-in, sign-out, password reset events
   - Include: timestamp, user ID, IP address, user agent, result (success/failure)

2. **Error Logging**
   - Log all authentication failures with reason
   - Do NOT log sensitive data (passwords, full tokens)

3. **Audit Trail**
   - Log user creation/sync events
   - Log role assignments

---

## Appendix A: Environment Variables

### Required Environment Variables

```env
# Azure AD B2C Configuration
AZURE_AD_B2C_INSTANCE=https://{tenant-name}.b2clogin.com
AZURE_AD_B2C_DOMAIN={tenant-name}.onmicrosoft.com
AZURE_AD_B2C_TENANT_ID={tenant-guid}
AZURE_AD_B2C_CLIENT_ID={application-client-id}
AZURE_AD_B2C_SUSI_POLICY=B2C_1_signup_signin
AZURE_AD_B2C_RESET_POLICY=B2C_1_password_reset

# Feature Flag (optional, default: AzureAdB2C)
AUTHENTICATION_MODE=AzureAdB2C  # or "Stub" for testing

# Existing Variables (keep)
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=cpr_dev
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
```

---

## Appendix B: Useful Links

### Microsoft Documentation

- [Azure AD B2C Overview](https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview)
- [Azure AD B2C User Flows](https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview)
- [Microsoft.Identity.Web Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [JWT Token Validation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tokens-overview)

### Azure AD B2C Pricing

- [Azure AD B2C Pricing](https://azure.microsoft.com/en-us/pricing/details/active-directory-b2c/)
- First 50,000 MAU (Monthly Active Users) free
- $0.00325 per MAU above 50,000

### Tools

- [JWT.io](https://jwt.io/) - Decode and validate JWT tokens
- [Azure AD B2C User Journey Debugger](https://docs.microsoft.com/en-us/azure/active-directory-b2c/troubleshoot-custom-policies)

---

## Appendix C: Sample Code Snippets

### Sample: User Sync Service

```csharp
public class UserSyncService : IUserSyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<UserSyncService> _logger;

    public async Task<User> SyncUserFromB2CAsync(string azureAdB2CObjectId, string email, string displayName)
    {
        // Try to find existing user by Azure AD B2C object ID
        var user = await _userRepository.GetByAzureAdB2CObjectIdAsync(azureAdB2CObjectId);
        
        if (user != null)
        {
            // Update display name if changed
            if (user.DisplayName != displayName)
            {
                user.DisplayName = displayName;
                user.ModifiedAt = DateTimeOffset.UtcNow;
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Updated user {UserId} display name to {DisplayName}", user.Id, displayName);
            }
            return user;
        }
        
        // Create new user
        user = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            DisplayName = displayName,
            AzureAdB2CObjectId = azureAdB2CObjectId,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };
        
        await _userRepository.AddAsync(user);
        _logger.LogInformation("Created new user {UserId} from Azure AD B2C object ID {ObjectId}", user.Id, azureAdB2CObjectId);
        
        // Assign default "Employee" role
        await _userRoleRepository.AssignRoleAsync(user.Id, EmployeeRoleId);
        
        return user;
    }
}
```

### Sample: Auth Controller Sign-In Endpoint

```csharp
[HttpPost("signin")]
[AllowAnonymous]
public IActionResult SignIn([FromBody] SignInRequestDto request)
{
    var b2cConfig = _configuration.GetSection("AzureAdB2C");
    var instance = b2cConfig["Instance"];
    var domain = b2cConfig["Domain"];
    var policy = b2cConfig["SignUpSignInPolicyId"];
    var clientId = b2cConfig["ClientId"];
    
    var returnUrl = request.ReturnUrl ?? "/";
    var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/signin-oidc";
    
    var authUrl = $"{instance}/{domain}/{policy}/oauth2/v2.0/authorize?" +
                  $"client_id={clientId}&" +
                  $"response_type=code&" +
                  $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                  $"response_mode=form_post&" +
                  $"scope=openid&" +
                  $"state={Uri.EscapeDataString(returnUrl)}";
    
    return Ok(new SignInResponseDto { RedirectUrl = authUrl });
}
```

---

## Approval & Sign-Off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Product Owner | | | |
| Engineering Lead | | | |
| Security Reviewer | | | |
| DevOps Lead | | | |

---

**Document Version**: 1.0  
**Last Updated**: October 13, 2025  
**Next Review**: After Phase 3 completion
