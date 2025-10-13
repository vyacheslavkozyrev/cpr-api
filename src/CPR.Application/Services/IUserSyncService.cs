using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for synchronizing users from Microsoft Entra External ID to the local database
    /// </summary>
    public interface IUserSyncService
    {
        /// <summary>
        /// Synchronize a user from Entra External ID claims to the local database.
        /// Creates a new user if EntraExternalId doesn't exist, or updates existing user if found.
        /// </summary>
        /// <param name="claims">Claims principal from Entra External ID JWT token</param>
        /// <param name="syncedBy">ID of the user performing the sync (typically system user)</param>
        /// <returns>The synchronized user entity with ID</returns>
        /// <exception cref="ArgumentNullException">Thrown when claims is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when required claims (oid, name) are missing</exception>
        Task<User> SyncUserFromClaimsAsync(ClaimsPrincipal claims, Guid syncedBy);

        /// <summary>
        /// Get user by Entra External ID (Object ID from 'oid' claim)
        /// </summary>
        /// <param name="entraExternalId">Entra External ID object identifier</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetUserByEntraExternalIdAsync(string entraExternalId);

        /// <summary>
        /// Extract Entra External ID from claims principal (from 'oid' claim)
        /// </summary>
        /// <param name="claims">Claims principal from Entra External ID JWT token</param>
        /// <returns>Entra External ID if found, null otherwise</returns>
        string? ExtractEntraExternalId(ClaimsPrincipal claims);

        /// <summary>
        /// Extract display name from claims principal (from 'name' or 'preferred_username' claim)
        /// </summary>
        /// <param name="claims">Claims principal from Entra External ID JWT token</param>
        /// <returns>Display name if found, null otherwise</returns>
        string? ExtractDisplayName(ClaimsPrincipal claims);
    }
}
