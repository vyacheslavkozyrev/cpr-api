using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Domain.Entities;

namespace CPR.Application.Repositories
{
    /// <summary>
    /// Repository interface for feedback request operations with multi-recipient support
    /// </summary>
    public interface IFeedbackRequestRepository
    {
        /// <summary>
        /// Create a new feedback request with multiple recipients
        /// </summary>
        /// <param name="feedbackRequest">The feedback request to create (with Recipients collection)</param>
        /// <returns>The created feedback request with generated IDs</returns>
        Task<FeedbackRequest> CreateAsync(FeedbackRequest feedbackRequest);

        /// <summary>
        /// Get a feedback request by ID with all related data (Recipients, Requestor, Project, Goal)
        /// </summary>
        /// <param name="id">The feedback request ID</param>
        /// <param name="includeDeleted">Whether to include soft-deleted requests</param>
        /// <returns>The feedback request or null if not found</returns>
        Task<FeedbackRequest?> GetByIdAsync(Guid id, bool includeDeleted = false);

        /// <summary>
        /// Get paginated list of feedback requests sent by a specific requestor
        /// </summary>
        /// <param name="requestorId">The employee ID who created the requests</param>
        /// <param name="query">Query parameters for pagination, filtering, and sorting</param>
        /// <returns>Paginated list of feedback requests with summary</returns>
        Task<PaginatedFeedbackRequestsDto> GetSentRequestsAsync(Guid requestorId, FeedbackRequestListQuery query);

        /// <summary>
        /// Get paginated list of feedback requests addressed to a specific employee (todo list)
        /// </summary>
        /// <param name="employeeId">The employee ID who should provide feedback</param>
        /// <param name="query">Query parameters for pagination, filtering, and sorting</param>
        /// <returns>Paginated list of feedback request recipients with parent request data</returns>
        Task<PaginatedFeedbackRequestsDto> GetTodoRequestsAsync(Guid employeeId, FeedbackRequestListQuery query);

        /// <summary>
        /// Get paginated list of feedback requests sent by team members (direct reports)
        /// </summary>
        /// <param name="managerId">The manager's employee ID</param>
        /// <param name="query">Query parameters for pagination, filtering, and sorting</param>
        /// <returns>Paginated list of team member requests</returns>
        Task<PaginatedFeedbackRequestsDto> GetTeamSentRequestsAsync(Guid managerId, FeedbackRequestListQuery query);

        /// <summary>
        /// Get paginated list of feedback requests received by team members (direct reports)
        /// </summary>
        /// <param name="managerId">The manager's employee ID</param>
        /// <param name="query">Query parameters for pagination, filtering, and sorting</param>
        /// <returns>Paginated list of requests addressed to team members</returns>
        Task<PaginatedFeedbackRequestsDto> GetTeamReceivedRequestsAsync(Guid managerId, FeedbackRequestListQuery query);

        /// <summary>
        /// Update a feedback request (only due_date can be updated)
        /// </summary>
        /// <param name="feedbackRequest">The feedback request with updated fields</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateAsync(FeedbackRequest feedbackRequest);

        /// <summary>
        /// Soft delete a feedback request (sets IsDeleted = true, DeletedAt = now, DeletedBy = userId)
        /// </summary>
        /// <param name="feedbackRequest">The feedback request to delete</param>
        /// <param name="deletedBy">The user ID who is deleting the request</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteAsync(FeedbackRequest feedbackRequest, Guid deletedBy);

        /// <summary>
        /// Cancel a specific recipient within a feedback request (sets IsCompleted = true without feedback)
        /// </summary>
        /// <param name="recipientId">The feedback request recipient ID</param>
        /// <returns>Task representing the async operation</returns>
        Task CancelRecipientAsync(Guid recipientId);

        /// <summary>
        /// Update last reminder timestamp for a specific recipient
        /// </summary>
        /// <param name="recipientId">The feedback request recipient ID</param>
        /// <param name="reminderSentAt">The timestamp when reminder was sent</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateLastReminderAsync(Guid recipientId, DateTimeOffset reminderSentAt);

        /// <summary>
        /// Check for duplicate active requests (same requestor, same recipients, same project/goal)
        /// </summary>
        /// <param name="requestorId">The employee ID creating the request</param>
        /// <param name="recipientIds">List of recipient employee IDs</param>
        /// <param name="projectId">Optional project ID</param>
        /// <param name="goalId">Optional goal ID</param>
        /// <returns>List of employee IDs that already have active requests matching the criteria</returns>
        Task<List<Guid>> CheckDuplicateRecipientsAsync(Guid requestorId, List<Guid> recipientIds, Guid? projectId, Guid? goalId);

        /// <summary>
        /// Get count of requests created by an employee today (for rate limiting)
        /// </summary>
        /// <param name="requestorId">The employee ID</param>
        /// <returns>Count of requests created today</returns>
        Task<int> GetTodayRequestCountAsync(Guid requestorId);
    }
}
