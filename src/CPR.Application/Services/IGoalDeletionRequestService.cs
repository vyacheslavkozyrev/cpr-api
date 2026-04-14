using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for goal deletion request workflow.
    /// </summary>
    public interface IGoalDeletionRequestService
    {
        /// <summary>
        /// Create a deletion request for a goal. Enforces one-pending-request constraint.
        /// The requesting employee must own the goal.
        /// </summary>
        Task<GoalDeletionRequestDto> RequestDeletionAsync(Guid goalId, Guid employeeId);

        /// <summary>
        /// Cancel a pending deletion request (employee action). Hard-deletes the row.
        /// </summary>
        Task CancelDeletionRequestAsync(Guid goalId, Guid employeeId);

        /// <summary>
        /// Approve a deletion request — soft-deletes the goal and all its tasks.
        /// Manager must be the direct manager of the goal owner.
        /// </summary>
        Task ApproveDeletionAsync(Guid goalId, Guid managerEmployeeId);

        /// <summary>
        /// Reject a deletion request — sets request status to rejected.
        /// Manager must be the direct manager of the goal owner.
        /// </summary>
        Task<GoalDto> RejectDeletionAsync(Guid goalId, Guid managerEmployeeId);
    }
}
