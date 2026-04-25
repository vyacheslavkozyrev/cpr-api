using System;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for goal deletion request operations.
    /// </summary>
    public interface IGoalDeletionRequestRepository
    {
        /// <summary>
        /// Get the pending deletion request for a goal, or null if none exists.
        /// </summary>
        Task<GoalDeletionRequest?> GetPendingByGoalIdAsync(Guid goalId);

        /// <summary>
        /// Persist a new deletion request.
        /// </summary>
        Task AddAsync(GoalDeletionRequest request);

        /// <summary>
        /// Update an existing deletion request's status fields.
        /// </summary>
        Task UpdateStatusAsync(GoalDeletionRequest request);

        /// <summary>
        /// Hard-delete a deletion request row (used for cancellation — no is_deleted column).
        /// </summary>
        Task DeleteAsync(GoalDeletionRequest request);
    }
}
