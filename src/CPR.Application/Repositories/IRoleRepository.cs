using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for role operations
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Get role by ID
        /// </summary>
        Task<Role?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get role by title
        /// </summary>
        Task<Role?> GetByTitleAsync(string title);

        /// <summary>
        /// Query all active roles
        /// </summary>
        IQueryable<Role> QueryActive();

        /// <summary>
        /// Add new role
        /// </summary>
        Task AddAsync(Role role);

        /// <summary>
        /// Update existing role
        /// </summary>
        Task UpdateAsync(Role role);

        /// <summary>
        /// Soft delete role
        /// </summary>
        Task DeleteAsync(Role role);
    }
}