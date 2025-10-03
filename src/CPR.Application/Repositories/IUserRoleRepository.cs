using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for user-to-role operations
    /// </summary>
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Get user role assignment by ID
        /// </summary>
        Task<UserToRole?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        Task<IQueryable<UserToRole>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Check if user has a specific role
        /// </summary>
        Task<bool> UserHasRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// Check if user has a specific role by title
        /// </summary>
        Task<bool> UserHasRoleAsync(Guid userId, string roleTitle);

        /// <summary>
        /// Assign role to user
        /// </summary>
        Task AddAsync(UserToRole userRole);

        /// <summary>
        /// Remove role from user (soft delete)
        /// </summary>
        Task RemoveAsync(UserToRole userRole);

        /// <summary>
        /// Get all active user-role assignments
        /// </summary>
        IQueryable<UserToRole> QueryActive();
    }
}