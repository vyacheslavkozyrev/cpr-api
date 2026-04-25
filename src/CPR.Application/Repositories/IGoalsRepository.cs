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

        /// <summary>
        /// Get all non-deleted goals for an employee for the manager view.
        /// Includes tasks and suggested-by user info; also flags whether a pending deletion request exists.
        /// </summary>
        Task<Goal[]> GetEmployeeGoalsForManagerAsync(Guid employeeId);

        /// <summary>
        /// Soft-delete all non-deleted tasks belonging to a goal.
        /// </summary>
        Task SoftDeleteTasksForGoalAsync(Guid goalId, Guid deletedBy);
    }
}