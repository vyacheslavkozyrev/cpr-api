using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CPR.Application.Services;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service for synchronizing users from Microsoft Entra External ID to the local database
    /// </summary>
    public class UserSyncService : IUserSyncService
    {
        private readonly CprDbContext _context;
        private readonly ILogger<UserSyncService> _logger;

        public UserSyncService(CprDbContext context, ILogger<UserSyncService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Synchronize a user from Entra External ID claims to the local database.
        /// Creates a new user if EntraExternalId doesn't exist, or updates existing user if found.
        /// </summary>
        public async Task<User> SyncUserFromClaimsAsync(ClaimsPrincipal claims, Guid syncedBy)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            // Extract Entra External ID (oid claim)
            var entraExternalId = ExtractEntraExternalId(claims);
            if (string.IsNullOrWhiteSpace(entraExternalId))
            {
                throw new InvalidOperationException("Missing required 'oid' claim from Entra External ID token");
            }

            // Extract display name
            var displayName = ExtractDisplayName(claims);
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new InvalidOperationException("Missing required 'name' or 'preferred_username' claim from Entra External ID token");
            }

            // Extract email (optional)
            var email = claims.FindFirst(ClaimTypes.Email)?.Value ?? claims.FindFirst("email")?.Value;

            // Extract username from preferred_username or email
            var username = claims.FindFirst("preferred_username")?.Value ??
                          email?.Split('@')[0] ??
                          $"user_{entraExternalId.Substring(0, 8)}";

            // Check if user exists by EntraExternalId
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EntraExternalId == entraExternalId && !u.IsDeleted);

            if (existingUser != null)
            {
                // Update existing user
                _logger.LogInformation(
                    "Updating existing user {UserId} with EntraExternalId {EntraExternalId}",
                    existingUser.Id,
                    entraExternalId);

                existingUser.DisplayName = displayName;
                existingUser.ModifiedAt = DateTimeOffset.UtcNow;
                existingUser.ModifiedBy = syncedBy;

                await _context.SaveChangesAsync();
                return existingUser;
            }

            // Create new user
            _logger.LogInformation(
                "Creating new user with EntraExternalId {EntraExternalId} and DisplayName {DisplayName}",
                entraExternalId,
                displayName);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                EntraExternalId = entraExternalId,
                UserName = username,
                DisplayName = displayName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = syncedBy,
                IsDeleted = false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        /// <summary>
        /// Get user by Entra External ID (Object ID from 'oid' claim)
        /// </summary>
        public async Task<User?> GetUserByEntraExternalIdAsync(string entraExternalId)
        {
            if (string.IsNullOrWhiteSpace(entraExternalId))
            {
                return null;
            }

            return await _context.Users
                .FirstOrDefaultAsync(u => u.EntraExternalId == entraExternalId && !u.IsDeleted);
        }

        /// <summary>
        /// Extract Entra External ID from claims principal (from 'oid' claim)
        /// </summary>
        public string? ExtractEntraExternalId(ClaimsPrincipal claims)
        {
            if (claims == null)
            {
                return null;
            }

            // Entra External ID uses 'oid' (object ID) claim
            return claims.FindFirst("oid")?.Value ??
                   claims.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        }

        /// <summary>
        /// Extract display name from claims principal (from 'name' or 'preferred_username' claim)
        /// </summary>
        public string? ExtractDisplayName(ClaimsPrincipal claims)
        {
            if (claims == null)
            {
                return null;
            }

            // Try multiple claim types in order of preference
            return claims.FindFirst(ClaimTypes.Name)?.Value ??
                   claims.FindFirst("name")?.Value ??
                   claims.FindFirst("preferred_username")?.Value ??
                   claims.FindFirst("email")?.Value;
        }
    }
}
