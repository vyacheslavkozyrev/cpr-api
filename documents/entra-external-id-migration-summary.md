# Microsoft Entra External ID Migration - Summary of Changes

**Date**: October 13, 2025  
**Iteration**: 16  
**Change Type**: Documentation Update (Azure AD B2C → Microsoft Entra External ID)

---

## What Changed?

**Azure AD B2C** has been rebranded and evolved into **Microsoft Entra External ID**, which is part of the Microsoft Entra family of identity products.

---

## Key Differences

| Aspect | Azure AD B2C (Old) | Microsoft Entra External ID (New) |
|--------|-------------------|----------------------------------|
| **Product Name** | Azure Active Directory B2C | Microsoft Entra External ID |
| **Domain** | `{tenant}.b2clogin.com` | `{tenant}.ciamlogin.com` |
| **Portal** | Azure Portal | Microsoft Entra admin center (entra.microsoft.com) |
| **Tenant Type** | B2C Tenant | External ID - Customers (CIAM) |
| **User Flows** | Separate user flows (B2C_1_*) | Integrated authentication experience |
| **Focus** | Consumer identity | Customer identity and access management (CIAM) |
| **Integration** | Standalone | Part of Microsoft Entra ecosystem |

---

## Documentation Updates

### Files Updated:

1. **`iteration-16-azure-ad-b2c-auth-plan.md`**
   - Title updated to "Microsoft Entra External ID Authentication Implementation Plan"
   - All references changed from Azure AD B2C to Microsoft Entra External ID
   - Domain changed from `.b2clogin.com` to `.ciamlogin.com`
   - User flows replaced with integrated authentication experience

2. **`phase-1-entra-external-id-setup-guide.md`** (NEW)
   - Complete rewrite for Microsoft Entra External ID
   - Step-by-step guide updated with current portal UI
   - Modern CIAM tenant creation process
   - Updated authentication configuration

3. **`phase-1-azure-ad-b2c-setup-guide.md`** (DELETED)
   - Old guide removed, replaced with Entra External ID version

---

## Configuration Changes

### Environment Variables (Updated)

**Old (Azure AD B2C)**:
```env
AZURE_AD_B2C_INSTANCE=https://cpr-dev.b2clogin.com
AZURE_AD_B2C_TENANT_ID={guid}
AZURE_AD_B2C_CLIENT_ID={guid}
AZURE_AD_B2C_DOMAIN=cpr-dev.onmicrosoft.com
AZURE_AD_B2C_SUSI_POLICY=B2C_1_signup_signin
AZURE_AD_B2C_RESET_POLICY=B2C_1_password_reset
```

**New (Microsoft Entra External ID)**:
```env
ENTRA_EXTERNAL_INSTANCE=https://cpr-dev.ciamlogin.com
ENTRA_EXTERNAL_TENANT_ID={guid}
ENTRA_EXTERNAL_CLIENT_ID={guid}
ENTRA_EXTERNAL_DOMAIN=cpr-dev.ciamlogin.com
ENTRA_EXTERNAL_AUTHORITY=https://cpr-dev.ciamlogin.com/{tenant-id}
```

### Database Schema Changes

**Old**:
```sql
azure_ad_b2c_object_id VARCHAR(100)
```

**New**:
```sql
entra_external_id VARCHAR(100)
```

### Code Changes

**Old User Entity**:
```csharp
public string? AzureAdB2CObjectId { get; set; }
```

**New User Entity**:
```csharp
public string? EntraExternalId { get; set; }
```

---

## Implementation Plan Changes

### Phase 1: Setup & Configuration
- **Old**: Create Azure AD B2C tenant with user flows
- **New**: Create Microsoft Entra External ID tenant with integrated authentication

### Phase 2: Database Migration
- **Old**: Add `azure_ad_b2c_object_id` column
- **New**: Add `entra_external_id` column

### Phase 3: Authentication Infrastructure
- **Old**: Validate tokens from `*.b2clogin.com`
- **New**: Validate tokens from `*.ciamlogin.com`

### Phase 4-7: No Major Changes
- Endpoints, testing, migration, and documentation phases remain largely the same
- Only naming conventions updated

---

## Why This Change?

1. **Microsoft's Rebranding**: Azure AD B2C is now part of the Microsoft Entra family
2. **Better Integration**: Entra External ID integrates better with other Microsoft identity products
3. **Modern UI**: New admin center (entra.microsoft.com) provides better user experience
4. **Future-Proof**: Microsoft is investing in Entra External ID as the future CIAM solution
5. **Enhanced Features**: External ID includes new features and improvements over B2C

---

## What Stays the Same?

✅ **OAuth 2.0 / OpenID Connect protocols** - Standard authentication flows  
✅ **JWT tokens** - Token format and validation process  
✅ **User attributes** - Email, display name, object ID  
✅ **Password policies** - Security requirements  
✅ **Self-service sign-up** - User registration flows  
✅ **Password reset** - Self-service password reset  
✅ **Microsoft.Identity.Web** - Same NuGet package works with both  

---

## Migration Path

If you already have Azure AD B2C:
1. **No immediate action required** - Azure AD B2C continues to work
2. **Microsoft provides migration tools** - When you're ready to migrate
3. **New projects** - Use Microsoft Entra External ID from the start

---

## Next Steps for CPR API

1. **Follow Phase 1 Guide**: Use `phase-1-entra-external-id-setup-guide.md`
2. **Create External ID Tenant**: Set up `cpr-dev.ciamlogin.com`
3. **Test Authentication**: Verify sign-up and sign-in work
4. **Proceed to Phase 2**: Database schema migration with `entra_external_id`

---

## References

- [Microsoft Entra External ID Overview](https://learn.microsoft.com/en-us/entra/external-id/)
- [What's new in Microsoft Entra](https://learn.microsoft.com/en-us/entra/fundamentals/whats-new)
- [Migrate from Azure AD B2C to External ID](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-migrate-customers-from-azure-ad-b2c)

---

**Status**: Documentation updated ✅  
**Ready for Phase 1**: Yes  
**Breaking Changes**: None (this is a new implementation)
