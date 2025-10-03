using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for role-based access control operations
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Check if user has a specific role
        /// </summary>
        Task<bool> UserHasRoleAsync(Guid userId, string roleTitle);

        /// <summary>
        /// Check if user has any of the specified roles
        /// </summary>
        Task<bool> UserHasAnyRoleAsync(Guid userId, params string[] roleTitles);

        /// <summary>
        /// Get all role titles for a user
        /// </summary>
        Task<IEnumerable<string>> GetUserRoleTitlesAsync(Guid userId);

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        Task AssignRoleToUserAsync(Guid userId, string roleTitle, Guid assignedBy);

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        Task RemoveRoleFromUserAsync(Guid userId, string roleTitle, Guid removedBy);

        /// <summary>
        /// Get all available roles
        /// </summary>
        Task<IEnumerable<string>> GetAllRoleTitlesAsync();
    }
}