# Phase 1: Microsoft Entra External ID Setup & Configuration Guide

**Iteration**: 16  
**Phase**: 1 of 7  
**Duration**: 1 day  
**Status**: Ready to Start  
**Date Started**: October 13, 2025

---

## Overview

This guide walks you through setting up **Microsoft Entra External ID** (formerly Azure AD B2C) for the CPR API. Microsoft Entra External ID is part of the Microsoft Entra family and provides customer identity and access management (CIAM) capabilities.

**What's Changed**: Azure AD B2C has been rebranded and evolved into Microsoft Entra External ID, offering enhanced features and better integration with the Microsoft Entra ecosystem.

---

## Prerequisites

- [ ] Azure subscription (free tier is sufficient for development)
- [ ] Azure Portal access: https://portal.azure.com
- [ ] Microsoft Entra admin center access: https://entra.microsoft.com
- [ ] Permissions to create Microsoft Entra resources
- [ ] Notepad or text editor to record configuration values

---

## Step 1: Create Microsoft Entra External ID Tenant

### 1.1 Navigate to Microsoft Entra Admin Center

1. Go to https://entra.microsoft.com (or https://portal.azure.com)
2. Sign in with your Azure credentials
3. If using Azure Portal, search for **"Microsoft Entra ID"** in the top search bar
4. Select **Microsoft Entra ID** from the results

### 1.2 Create External ID Tenant (CIAM)

**Option A: Via Azure Portal (Recommended - Most Reliable)**

1. Go to https://portal.azure.com
2. In the top search bar, type: **"Microsoft Entra ID"** or **"Tenant"**
3. Click **"+ Create a tenant"** or look for **"Manage tenants"** then **"+ Create"**
4. Select **"Microsoft Entra ID"** (not External ID directly - we'll configure it for External ID use)
5. Or search for **"Azure AD B2C"** in the search bar and create from there
   - Note: The UI may still show "Azure AD B2C" even though it's now External ID

**Option B: Direct Marketplace Link**

1. Go to Azure Portal
2. Navigate to **Marketplace** or search for **"Azure Active Directory B2C"** or **"External ID"**
3. Click **Create**
4. Follow the creation wizard

**Option C: Via Microsoft Entra Admin Center (if available)**

1. Go to https://entra.microsoft.com
2. Click on your tenant name/icon in the top-right corner
3. Select **"Manage tenants"** or **"Switch tenant"**
4. Click **"+ Create a tenant"**
5. Choose **"Microsoft Entra ID"** or **"External ID for customers"** if available

> 💡 **Note**: The exact navigation may vary depending on your subscription and portal version. If you don't see "External Identities" in the navigation, use Option A (Azure Portal) which is most reliable.

### 1.3 Configure Tenant Settings

Fill in the tenant configuration:

**Basics**:
- **Tenant type**: Select **External ID - Customers (CIAM)** (if available) or **Microsoft Entra ID**
- **Organization name**: `CPR Development` (or `CPR Staging`, `CPR Production`)
- **Initial domain name**: `cpr-dev` 
  - This creates: `cpr-dev.ciamlogin.com` OR `cpr-dev.onmicrosoft.com`
  - Domain suffix depends on tenant type selected, but **both work the same way**
  - Must be globally unique
- **Country/Region**: Select your region (e.g., United States, Europe)
  - **Important**: Cannot be changed after creation
  - Determines data residency

> 💡 **Note**: If your domain ends with `.onmicrosoft.com` but the tenant type is "External", that's perfectly fine! The domain suffix doesn't affect External ID functionality.

**Configuration**:
- **Subscription**: Select your Azure subscription
- **Resource group**: 
  - Create new → `rg-cpr-dev-external-id`
  - Or use existing resource group

3. Click **Review + create**
4. Review the settings carefully
5. Click **Create**

> ⏱️ **Wait Time**: Tenant creation takes 2-5 minutes

### 1.4 Access Your External ID Tenant

After creation:

1. You'll see a notification: **"Your tenant has been created"**
2. Click **Go to tenant** or navigate manually:
   - Go to https://entra.microsoft.com
   - Click on your profile/tenant switcher (top-right)
   - Select your new tenant: `cpr-dev`

> ✅ **Checkpoint**: You should see your tenant name in the top bar and "External ID" in the navigation

### 1.5 Record Tenant Information

Open a text editor and record these values:

```
TENANT_NAME=cpr-dev
TENANT_DOMAIN=cpr-dev.ciamlogin.com
TENANT_ID=<copy from Overview page>
AUTHORITY_URL=https://cpr-dev.ciamlogin.com/<tenant-id>
```

**To find Tenant ID**:
1. In Microsoft Entra admin center, go to **Overview**
2. Look for **Tenant ID** or **Directory (tenant) ID**
3. It's a GUID like: `12345678-1234-1234-1234-123456789abc`
4. Click the copy icon

---

## Step 2: Register CPR API Application

### 2.1 Navigate to App Registrations

1. In Microsoft Entra admin center (https://entra.microsoft.com)
2. Go to **Applications** → **App registrations**
3. Click **+ New registration**

### 2.2 Configure Application Registration

Fill in the registration form:

**Name**:
```
CPR API
```

**Supported account types**:
- Select: **Accounts in this organizational directory only (cpr-dev only - Single tenant)**

**Redirect URI**:
- Platform: **Web**
- For now, add the local development URI:
  ```
  http://localhost:5000/login
  ```
  
  > We'll add more redirect URIs later

3. Click **Register**

> ⏱️ **Wait Time**: Registration is instant

### 2.3 Record Application (Client) ID

After registration, you'll be on the application's **Overview** page.

**Record these values**:
```
APPLICATION_CLIENT_ID=<copy Application (client) ID>
APPLICATION_OBJECT_ID=<copy Object ID>
```

> 💡 **Tip**: Keep this browser tab open - you'll need to configure more settings

### 2.4 Add Additional Redirect URIs

1. Go to **Manage** → **Authentication**
2. Under **Web** redirect URIs, click **+ Add URI**
3. Add these redirect URIs:

   **For Development**:
   ```
   http://localhost:5000/signin-oidc
   http://localhost:5000/api/auth/signin-oidc
   http://localhost:5000/signout-callback-oidc
   ```

   **For Staging** (update with your actual domain):
   ```
   https://api-staging.cpr.yourcompany.com/signin-oidc
   https://api-staging.cpr.yourcompany.com/api/auth/signin-oidc
   https://api-staging.cpr.yourcompany.com/signout-callback-oidc
   ```

   **For Production** (update with your actual domain):
   ```
   https://api.cpr.yourcompany.com/signin-oidc
   https://api.cpr.yourcompany.com/api/auth/signin-oidc
   https://api.cpr.yourcompany.com/signout-callback-oidc
   ```

4. Click **Save** (bottom of page)

### 2.5 Configure Token Configuration

1. Stay in **Authentication** section
2. Under **Implicit grant and hybrid flows**:
   - ☑ Check **ID tokens** (used for user authentication)
   - ☐ Leave **Access tokens** unchecked (we'll use authorization code flow)

3. Under **Advanced settings**:
   - **Allow public client flows**: Select **No**
   - **Enable the following mobile and desktop flows**: **No**

4. Click **Save**

### 2.6 Configure API Permissions

1. Go to **Manage** → **API permissions**
2. You'll see **User.Read** (Microsoft Graph) is already added
3. Click **+ Add a permission**
4. Select **Microsoft Graph**
5. Select **Delegated permissions**
6. Search for and check these permissions:
   - ☑ `openid` (OpenID Connect authentication)
   - ☑ `profile` (Read user's basic profile)
   - ☑ `email` (Read user's email address)
   - ☑ `offline_access` (Maintain access to data) - Optional, for refresh tokens

7. Click **Add permissions**
8. Click **✓ Grant admin consent for cpr-dev** button
9. Click **Yes** to confirm

> ✅ **Checkpoint**: All permissions should show "Granted for cpr-dev" with green checkmarks

### 2.7 Expose an API (Create API Scope)

This allows your API to define permissions that clients can request.

1. Go to **Manage** → **Expose an API**
2. Click **+ Add a scope**
3. You'll be prompted to set an **Application ID URI**:
   
   **Option A - Use default**:
   ```
   api://<your-client-id>
   ```
   
   **Option B - Use custom** (recommended):
   ```
   api://cpr-api
   ```
   
   Click **Save and continue**

4. Configure the scope:
   - **Scope name**: `API.Access`
   - **Who can consent**: **Admins and users**
   - **Admin consent display name**: `Access CPR API`
   - **Admin consent description**: `Allows the application to access CPR API on behalf of the signed-in user`
   - **User consent display name**: `Access CPR data`
   - **User consent description**: `Allows the application to access your career and performance review data`
   - **State**: **Enabled**

5. Click **Add scope**

### 2.8 Record API Scope Information

```
API_SCOPE=api://cpr-api/API.Access
APPLICATION_ID_URI=api://cpr-api
```

---

## Step 3: Configure Sign-up and Sign-in Experience

**✅ Good News**: If you see "This feature is unavailable or doesn't apply to the current tenant configuration" in various settings, this confirms you have an **External ID customer tenant**, which is exactly what we want!

Microsoft Entra External ID (for customers/CIAM) has a simplified, pre-configured setup compared to standard Entra ID.

### 3.1 Understanding External ID Customer Tenants

**What's Different**:
- **Authentication is pre-configured**: Email/password authentication is enabled by default
- **Simplified navigation**: Many standard Entra ID menus are hidden or unavailable
- **No manual configuration needed**: Sign-up and sign-in work out of the box
- **Message you'll see**: "This feature is unavailable or doesn't apply to the current tenant configuration"

**What This Means**:
- ✅ Your tenant is correctly configured for customer authentication
- ✅ No need to configure "Protection → Authentication methods"
- ✅ No need to configure "External Identities" settings
- ✅ Users can sign up and sign in immediately using OAuth 2.0 endpoints

### 3.2 Verify Your Tenant Type

To confirm you have the correct tenant:

1. Go to Microsoft Entra admin center **Overview** page
2. Look for **Tenant type** - it should show **"External"** or **"External ID"**
3. Check your **Primary domain**

**Important**: The domain suffix (`.ciamlogin.com` vs `.onmicrosoft.com`) varies depending on how the tenant was created, but **both are valid** for External ID tenants!

**Valid External ID tenant domain examples**:
- ✅ **External ID with .ciamlogin.com**: `cpr-dev.ciamlogin.com` (created via External ID wizard)
- ✅ **External ID with .onmicrosoft.com**: `cpr-dev.onmicrosoft.com` (created via standard wizard, configured as External)
- ✅ **Both work identically** - the domain suffix doesn't affect functionality

**What matters**: 
- ✅ **Tenant type** = "External" or "External ID" (check in Overview)
- ✅ Seeing "This feature is unavailable..." in various settings
- ✅ Simplified navigation menu

**Not critical**: 
- ❓ Domain ending (`.ciamlogin.com` or `.onmicrosoft.com`) - both are fine!

### 3.3 Skip Manual Configuration

**You can skip these common configuration steps** (they're not available or not needed in External ID customer tenants):

- ❌ **Protection → Authentication methods** - Not available in customer tenants
- ❌ **Identity → External Identities → User flows** - Not needed, authentication works automatically
- ❌ **Users → User settings** - Most settings show "feature unavailable" message
- ❌ **External Identities → All identity providers** - Pre-configured for email/password

**What IS configured automatically**:
- ✅ Email + password authentication
- ✅ User sign-up flow
- ✅ Email verification (one-time passcode)
- ✅ Password complexity requirements
- ✅ OAuth 2.0 / OpenID Connect endpoints

### 3.4 User Attributes - Already Configured! ✅

**No action required**. External ID customer tenants automatically support these standard attributes:

- ✅ **Email** (required, used for sign-in)
- ✅ **Display Name** (collected during sign-up)
- ✅ **Given Name** (first name)
- ✅ **Surname** (last name)

These attributes are automatically available in ID tokens and can be accessed by your application.

**If you need custom attributes later** (e.g., "Department", "Job Title"):
- Custom attributes can be added via Microsoft Graph API
- Not needed for Phase 1 - we'll use the built-in attributes

### 3.5 Company Branding (Optional - Can Skip for Development)

**This step is optional** and can be configured later for production.

**To add company branding** (if available):
1. Try searching for **"Company branding"** in the Entra admin center search bar
2. Or navigate to **Settings** → **Company branding** (if visible)
3. Upload logo, colors, background image

**If not available**:
- Company branding may require a premium subscription
- For development, the default Microsoft sign-in page is perfectly fine
- Users will see a standard blue Microsoft sign-in page

> 💡 **Recommendation**: Skip this for now and focus on getting authentication working. You can add branding later before production deployment.

### 3.6 Step 3 Summary - You're All Set! ✅

**What you've learned**:
- ✅ External ID customer tenants are **pre-configured** for authentication
- ✅ Seeing "This feature is unavailable..." messages is **normal and expected**
- ✅ Email/password authentication is **already enabled**
- ✅ User attributes are **already available**
- ✅ No manual configuration needed for basic authentication

**What you can skip**:
- ❌ Protection → Authentication methods (not available)
- ❌ External Identities configuration (not needed)
- ❌ User Settings configuration (shows "unavailable")
- ❌ Custom user attributes (built-in ones work fine)
- ❌ Company branding (optional, can do later)

**Ready to proceed?** 
Move on to **Step 4: Configure Token Settings** - this is where you'll configure what user information is included in authentication tokens.

---

## Step 4: Configure Token Settings

### 4.1 Configure Token Lifetime

1. Go to **Applications** → **App registrations** → **CPR API**
2. Go to **Manage** → **Token configuration**
3. Click **+ Add optional claim**

**For ID Token**:
- Select **ID** token type
- Check these claims:
  - ☑ `email` - User's email address
  - ☑ `family_name` - User's last name
  - ☑ `given_name` - User's first name
  - ☑ `preferred_username` - User's username/email
  - ☑ `upn` - User principal name

4. Click **Add**

**For Access Token** (if needed):
- Select **Access** token type
- Add same claims as ID token

5. Click **Add**

### 4.2 Configure Token Lifetimes (Advanced - Optional)

**You can skip this step** - token lifetimes are already configured with secure defaults.

**Default lifetimes in Microsoft Entra External ID**:
- **Access token**: 60-75 minutes (randomized for security)
- **Refresh token**: 90 days (sliding window)
- **ID token**: 60-75 minutes

**Note about Conditional Access**:
- **Conditional Access** menu is typically not available in External ID customer tenants
- It may require Azure AD Premium P1/P2 licenses
- Token lifetime customization is not needed for most applications
- Default lifetimes are secure and appropriate for development and production

**If you need custom token lifetimes** (rare):
- Token lifetime policies can be configured via Microsoft Graph API
- Requires PowerShell or custom scripts
- Not recommended unless you have specific compliance requirements

> 💡 **Recommendation**: Use the default token lifetimes - they're secure and work well for most applications. Skip this configuration.

---

## Step 5: Test Sign-up and Sign-in

### 5.1 Get Sign-in URL

Microsoft Entra External ID uses standard OAuth 2.0 / OpenID Connect endpoints.

**Important**: The endpoint format differs based on your tenant domain type!

**Option A: For `.onmicrosoft.com` domains** (most common):

```
https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/authorize?
  client_id={client-id}
  &response_type=code
  &redirect_uri={redirect-uri}
  &response_mode=form_post
  &scope=openid profile email
  &state=12345
  &nonce=67890
```

**Option B: For `.ciamlogin.com` domains** (newer External ID tenants):

```
https://{tenant-name}.ciamlogin.com/{tenant-id}/oauth2/v2.0/authorize?
  client_id={client-id}
  &response_type=code
  &redirect_uri={redirect-uri}
  &response_mode=form_post
  &scope=openid profile email
  &state=12345
  &nonce=67890
```

**Replace placeholders**:
- `{tenant-id}`: Your Tenant ID GUID (e.g., `c77e5d0f-a26e-42e1-be72-fa5a9efd180c`)
- `{tenant-name}`: Your tenant name without domain suffix (e.g., `cpr-dev`)
- `{client-id}`: Your Application (client) ID
- `{redirect-uri}`: Your redirect URI (URL-encoded, e.g., `http%3A%2F%2Flocalhost%3A5000%2Fsignin-oidc`)

**Example for .onmicrosoft.com tenant**:
```
https://login.microsoftonline.com/c77e5d0f-a26e-42e1-be72-fa5a9efd180c/oauth2/v2.0/authorize?client_id=4d440622-bc73-44b7-a13c-9160a21d87f2&response_type=code&redirect_uri=https%3A%2F%2Flocalhost%3A3000&response_mode=form_post&scope=openid%20profile%20email&state=12345&nonce=67890
```

**Example for .ciamlogin.com tenant**:
```
https://cpr-dev.ciamlogin.com/12345678-1234-1234-1234-123456789abc/oauth2/v2.0/authorize?client_id=87654321-4321-4321-4321-210987654321&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A5000%2Fsignin-oidc&response_mode=form_post&scope=openid%20profile%20email&state=12345&nonce=67890
```

> 💡 **Key Difference**: `.onmicrosoft.com` tenants use `login.microsoftonline.com/{tenant-id}`, while `.ciamlogin.com` tenants use `{tenant-name}.ciamlogin.com/{tenant-id}`

### 5.2 Test in Browser

1. Copy the authorization URL (with your actual values)
2. Open a browser (incognito/private mode recommended)
3. Paste the URL and press Enter
4. You should see the Microsoft Entra sign-in page
5. Click **Sign up now** or **Create one** to create a new account
6. Enter your email address
7. You'll receive a verification code via email
8. Enter the code to verify your email
9. Set a password (must meet complexity requirements)
10. Enter Display Name
11. Click **Create**

> ✅ **Checkpoint**: You should be redirected to `http://localhost:5000/signin-oidc` with an authorization code

> Note: The redirect will show an error (localhost:5000 not running), but that's expected. The important part is the sign-up flow worked.

### 5.3 Verify User Creation

1. Go back to Microsoft Entra admin center
2. Navigate to **Users** → **All users**
3. You should see your newly created user
4. Click on the user to see details
5. Copy the **Object ID** - this is the user's unique identifier

```
TEST_USER_OBJECT_ID=<paste Object ID>
```

---

## Step 6: Create Additional Test Users

### 6.1 Create Test Users Manually

Create test users for different roles:

1. Go to **Users** → **All users**
2. Click **+ New user** → **Create new user**

**User 1: Test Employee**
- **User principal name**: `test.employee@cpr-dev.ciamlogin.com`
- **Display name**: `Test Employee`
- **Password**: Click **Auto-generate password** or set manually
- Check **Show password** and copy it
- Click **Create**

**User 2: Test Manager**
- **User principal name**: `test.manager@cpr-dev.ciamlogin.com`
- **Display name**: `Test Manager`
- Create and copy password

**User 3: Test Administrator**
- **User principal name**: `test.admin@cpr-dev.ciamlogin.com`
- **Display name**: `Test Administrator`
- Create and copy password

### 6.2 Record Test User Information

For each user, record:
```
# Test Employee
TEST_EMPLOYEE_EMAIL=test.employee@cpr-dev.ciamlogin.com
TEST_EMPLOYEE_OBJECT_ID=<object ID>
TEST_EMPLOYEE_PASSWORD=<initial password>

# Test Manager
TEST_MANAGER_EMAIL=test.manager@cpr-dev.ciamlogin.com
TEST_MANAGER_OBJECT_ID=<object ID>
TEST_MANAGER_PASSWORD=<initial password>

# Test Administrator
TEST_ADMIN_EMAIL=test.admin@cpr-dev.ciamlogin.com
TEST_ADMIN_OBJECT_ID=<object ID>
TEST_ADMIN_PASSWORD=<initial password>
```

> 🔒 **Security**: Store these credentials securely (password manager, Azure Key Vault)

---

## Step 7: Configure Password Reset Flow

Microsoft Entra External ID includes built-in password reset.

### 7.1 Verify Password Reset is Enabled

1. Go to **Users** → **Password reset**
2. Under **Properties**:
   - **Self-service password reset enabled**: Select **All** or **Selected**
   - If **Selected**, add a group containing your test users

3. Under **Authentication methods**:
   - **Number of methods required to reset**: `1`
   - Enable: ☑ **Email**
   - Enable: ☑ **Mobile phone** (optional)

4. Under **Registration**:
   - **Require users to register when signing in**: **No** (for development)
   - **Number of days before users are asked to reconfirm**: `180`

5. Click **Save**

### 7.2 Test Password Reset

1. Open sign-in URL in browser (from Step 5.1)
2. Click **Forgot my password**
3. Enter email address of a test user
4. Complete the verification (email code)
5. Set a new password
6. Sign in with new password

> ✅ **Checkpoint**: Password reset should work successfully

---

## Step 8: Record All Configuration Values

Create a file named `.env.external-id` in your project root (DO NOT COMMIT TO GIT):

```env
# Microsoft Entra External ID Configuration for CPR API
# Development Environment
# Created: October 13, 2025
# DO NOT COMMIT THIS FILE TO VERSION CONTROL

# ========================================
# Tenant Information
# ========================================
ENTRA_EXTERNAL_TENANT_NAME=cpr-dev
ENTRA_EXTERNAL_TENANT_DOMAIN=cpr-dev.ciamlogin.com
ENTRA_EXTERNAL_TENANT_ID=<paste Tenant ID GUID>

# ========================================
# Instance and Authority URLs
# ========================================
ENTRA_EXTERNAL_INSTANCE=https://cpr-dev.ciamlogin.com
ENTRA_EXTERNAL_AUTHORITY=https://cpr-dev.ciamlogin.com/<tenant-id>

# ========================================
# Application Registration
# ========================================
ENTRA_EXTERNAL_CLIENT_ID=<paste Application (client) ID>
ENTRA_EXTERNAL_OBJECT_ID=<paste Application Object ID>

# ========================================
# API Configuration
# ========================================
ENTRA_EXTERNAL_API_SCOPE=api://cpr-api/API.Access
ENTRA_EXTERNAL_APP_ID_URI=api://cpr-api

# ========================================
# Endpoints
# ========================================
ENTRA_EXTERNAL_AUTHORIZE_ENDPOINT=https://cpr-dev.ciamlogin.com/<tenant-id>/oauth2/v2.0/authorize
ENTRA_EXTERNAL_TOKEN_ENDPOINT=https://cpr-dev.ciamlogin.com/<tenant-id>/oauth2/v2.0/token
ENTRA_EXTERNAL_JWKS_URI=https://cpr-dev.ciamlogin.com/<tenant-id>/discovery/v2.0/keys
ENTRA_EXTERNAL_ISSUER=https://cpr-dev.ciamlogin.com/<tenant-id>/v2.0

# ========================================
# Callback/Redirect URLs
# ========================================
ENTRA_EXTERNAL_CALLBACK_PATH=/signin-oidc
ENTRA_EXTERNAL_SIGNOUT_CALLBACK_PATH=/signout-callback-oidc

# ========================================
# Authentication Mode
# ========================================
AUTHENTICATION_MODE=EntraExternalID

# ========================================
# Test Users (for development only)
# ========================================
TEST_EMPLOYEE_EMAIL=test.employee@cpr-dev.ciamlogin.com
TEST_EMPLOYEE_OBJECT_ID=<object ID>

TEST_MANAGER_EMAIL=test.manager@cpr-dev.ciamlogin.com
TEST_MANAGER_OBJECT_ID=<object ID>

TEST_ADMIN_EMAIL=test.admin@cpr-dev.ciamlogin.com
TEST_ADMIN_OBJECT_ID=<object ID>
```

> 🔒 **Important**: Add `.env.external-id` to your `.gitignore` file

---

## Step 9: Configure CORS (For SPA Clients)

If you'll have a frontend application (React, Angular, Vue):

1. Go to **Applications** → **App registrations** → **CPR API**
2. Go to **Manage** → **Authentication**
3. Scroll to **Single-page application**
4. Click **+ Add a platform** → Select **Single-page application**
5. Add frontend URLs:
   ```
   http://localhost:3000
   http://localhost:4200
   https://app-dev.cpr.yourcompany.com
   ```
6. Click **Configure**

---

## Step 10: Verify OpenID Connect Discovery

Microsoft Entra External ID provides an OpenID Connect discovery endpoint.

### 10.1 Access Discovery Document

1. Open a browser
2. Navigate to the OpenID Connect discovery endpoint based on your domain type:

   **For .onmicrosoft.com domains**:
   ```
   https://login.microsoftonline.com/{tenant-id}/v2.0/.well-known/openid-configuration
   ```
   
   Example:
   ```
   https://login.microsoftonline.com/c77e5d0f-a26e-42e1-be72-fa5a9efd180c/v2.0/.well-known/openid-configuration
   ```

   **For .ciamlogin.com domains**:
   ```
   https://{tenant-name}.ciamlogin.com/{tenant-id}/v2.0/.well-known/openid-configuration
   ```
   
   Example:
   ```
   https://cpr-dev.ciamlogin.com/12345678-1234-1234-1234-123456789abc/v2.0/.well-known/openid-configuration
   ```

3. You should see a JSON document with endpoints:
   ```json
   {
     "issuer": "https://login.microsoftonline.com/{tenant-id}/v2.0",
     "authorization_endpoint": "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/authorize",
     "token_endpoint": "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token",
     "jwks_uri": "https://login.microsoftonline.com/{tenant-id}/discovery/v2.0/keys",
     ...
   }
   ```

> ✅ **Checkpoint**: Discovery document should load successfully and show your actual tenant endpoints

### 10.2 Record Discovery URL

Record the correct URL based on your tenant domain:

**For .onmicrosoft.com domains**:
```
ENTRA_EXTERNAL_DISCOVERY_URL=https://login.microsoftonline.com/<tenant-id>/v2.0/.well-known/openid-configuration
```

**For .ciamlogin.com domains**:
```
ENTRA_EXTERNAL_DISCOVERY_URL=https://<tenant-name>.ciamlogin.com/<tenant-id>/v2.0/.well-known/openid-configuration
```

---

## Phase 1 Checklist

Before proceeding to Phase 2, ensure you have completed:

- [ ] Microsoft Entra External ID tenant created (e.g., `cpr-dev.ciamlogin.com`)
- [ ] Tenant ID recorded
- [ ] CPR API application registered in Microsoft Entra External ID
- [ ] Application (client) ID recorded
- [ ] Redirect URIs configured (localhost:5000/signin-oidc, etc.)
- [ ] API permissions configured and admin consent granted (openid, profile, email)
- [ ] API scope exposed (api://cpr-api/API.Access)
- [ ] Token configuration added (email, given_name, family_name claims)
- [ ] Self-service sign-up enabled and tested
- [ ] Password reset flow enabled and tested
- [ ] At least one user created via sign-up flow
- [ ] Test users created manually (employee, manager, admin)
- [ ] Test user Object IDs recorded
- [ ] All configuration values recorded in `.env.external-id`
- [ ] `.env.external-id` added to `.gitignore`
- [ ] OpenID Connect discovery document accessible
- [ ] Configuration values backed up securely

---

## Troubleshooting

### Issue: "This feature is unavailable or doesn't apply to the current tenant configuration"

**Solution**: 
- **This is NORMAL and EXPECTED for External ID customer tenants!** ✅
- You're seeing this message because you correctly created an External ID customer tenant
- This message appears in: User Settings, Authentication Methods, External Identities, and other areas
- **No action needed** - this is not an error

**Why you see this**:
- External ID customer tenants are designed for customer-facing applications
- Many enterprise/workforce features are intentionally disabled
- Authentication is pre-configured and works automatically
- Simplified configuration reduces complexity

**What's already working**:
- ✅ Email/password authentication
- ✅ User sign-up and sign-in
- ✅ Email verification
- ✅ Password reset
- ✅ OAuth 2.0 / OpenID Connect endpoints

**Verification you have the right tenant**:
- **Tenant type** in Overview shows "External" or "External ID" ✅ (most important!)
- You see "This feature is unavailable..." in multiple settings ✅
- Navigation menu is simplified compared to standard Entra ID ✅
- Domain ends in `.ciamlogin.com` OR `.onmicrosoft.com` (both are valid for External ID)

**Action**: Proceed to Step 4 - your tenant is correctly configured! ✅

### Issue: Cannot find "Protection → Authentication methods" or "External Identities" menus

**Solution**: 
- **This is normal for External ID (customer) tenants!**
- These menus don't exist in External ID customer tenants
- Authentication is pre-configured and enabled by default
- No manual configuration needed

**Key Differences**:
- **Standard Entra ID tenant**: Has "Protection", "Authentication methods", full "External Identities" menu
- **External ID customer tenant**: Simplified navigation, pre-configured authentication, many features "unavailable"

**Verification**: 
- **Tenant type** shows "External" or "External ID" in Overview → ✅ Correct tenant type
- Domain ends in `.ciamlogin.com` OR `.onmicrosoft.com` → ✅ Both are valid
- Selected "External ID - Customers (CIAM)" during creation → ✅ Correct tenant type
- Seeing "This feature is unavailable..." messages → ✅ Correct tenant type
- Proceed directly to Step 4 (Configure Token Settings)

### Issue: Cannot find "External ID" or "External Identities" in Entra Admin Center

**Solution**: 
- The navigation structure varies by subscription and portal version
- **Best workaround**: Use Azure Portal (https://portal.azure.com) instead:
  1. Search for "Tenant" or "Microsoft Entra ID" in the top search bar
  2. Look for "+ Create a tenant" or "Manage tenants" → "+ Create"
  3. You can create a standard Microsoft Entra ID tenant and configure it for customer/external authentication
- **Alternative**: Search directly for "Azure AD B2C" in Azure Portal search - the service works the same way as External ID
- **Azure CLI option** (see below): Create tenant via command line if portal navigation is difficult
- Microsoft Entra External ID is essentially Azure AD B2C with rebranding and enhanced features
- Both approaches result in the same authentication capabilities we need

**Azure CLI Alternative** (if portal is problematic):
```powershell
# Install Azure CLI if not already installed
# Download from: https://aka.ms/installazurecliwindows

# Login
az login

# Create Azure AD B2C (External ID) tenant
az resource create `
  --resource-type "Microsoft.AzureActiveDirectory/b2cDirectories" `
  --name "cpr-dev" `
  --resource-group "rg-cpr-dev-external-id" `
  --location "United States" `
  --properties '{\"createTenantProperties\":{\"displayName\":\"CPR Development\",\"countryCode\":\"US\"}}'
```

After creation via CLI, manage the tenant through Azure Portal or Entra Admin Center.

### Issue: "Tenant name already exists"

**Solution**:
- Tenant names must be globally unique
- Try a different name (e.g., `cpr-dev-yourcompany`)
- The `.ciamlogin.com` domain is reserved for External ID tenants

### Issue: "Redirect URI mismatch" error

**Solution**:
- Go to **App registrations** → **CPR API** → **Authentication**
- Verify redirect URI matches exactly (including protocol, port, path)
- URLs are case-sensitive
- Ensure no trailing slashes unless specified

### Issue: Email verification code not received

**Solution**:
- Check spam/junk folder
- Verify email address is correct
- Email delivery can take 1-2 minutes
- For production, configure custom email provider (SendGrid, etc.)

### Issue: Sign-in page not loading or shows error

**Solution**:
- Verify the authorization URL is correctly formatted
- Check that tenant ID and client ID are correct
- Ensure redirect URI is registered in the app
- Try incognito/private browsing mode
- Clear browser cache and cookies

### Issue: "AADSTS50011: The redirect URI specified in the request does not match"

**Solution**:
- The redirect URI in your request must exactly match one configured in the app
- Check for typos, extra spaces, wrong protocol (http vs https)
- URL-encode the redirect URI in the authorization request

---

## Next Steps

Congratulations! Phase 1 is complete. 🎉

**Next: Phase 2 - Database Schema Migration**

Phase 2 tasks:
- Create migration to add `entra_external_id` column to users table
- Update User entity with `EntraExternalId` property
- Update CprDbContext configuration
- Create index on `entra_external_id`
- Test migration up/down
- Prepare migration to remove `password_hash` column (apply later)

---

## Additional Resources

### Microsoft Entra External ID Documentation
- [Microsoft Entra External ID Overview](https://learn.microsoft.com/en-us/entra/external-id/)
- [Quickstart: Set up sign-in for a web app](https://learn.microsoft.com/en-us/entra/external-id/customers/quickstart-web-app-dotnet-sign-in)
- [Authentication flows and application scenarios](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-authentication-methods)

### Microsoft Identity Platform
- [Microsoft Identity Platform documentation](https://learn.microsoft.com/en-us/entra/identity-platform/)
- [Microsoft.Identity.Web library](https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/)
- [OAuth 2.0 and OpenID Connect protocols](https://learn.microsoft.com/en-us/entra/identity-platform/v2-protocols)

### Best Practices
- [Security best practices for External ID](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-security-customers)
- [Token lifetime policies](https://learn.microsoft.com/en-us/entra/identity-platform/configurable-token-lifetimes)

### Migration from Azure AD B2C
- [Migrate from Azure AD B2C to External ID](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-migrate-customers-from-azure-ad-b2c)

---

**Phase 1 Status**: ✅ Complete  
**Started**: October 13, 2025  
**Completed**: _____________  
**Completed By**: _____________  
**Configuration File**: `.env.external-id` (stored securely, not in git)
