using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for goal operations
    /// </summary>
    public interface IGoalsRepository
    {
        /// <summary>
        /// Get goal by ID
        /// </summary>
        Task<Goal?> GetByIdAsync(Guid id);

        /// <summary>
        /// Query goals by employee (backwards-compatible alias for QueryByEmployee)
        /// </summary>
        IQueryable<Goal> QueryByOwner(Guid ownerId);

        /// <summary>
        /// Query goals by employee
        /// </summary>
        IQueryable<Goal> QueryByEmployee(Guid employeeId);

        /// <summary>
        /// Add new goal
        /// </summary>
        Task AddAsync(Goal goal);

        /// <summary>
        /// Update existing goal
        /// </summary>
        Task UpdateAsync(Goal goal);

        /// <summary>
        /// Soft delete goal
        /// </summary>
        Task DeleteAsync(Goal goal);
    }
}