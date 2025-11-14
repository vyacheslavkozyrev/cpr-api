using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for feedback request management operations with multi-recipient support
    /// </summary>
    public interface IFeedbackRequestService
    {
        /// <summary>
        /// Create a new feedback request with 1-20 recipients
        /// </summary>
        /// <param name="requestorId">Employee ID making the request (from JWT claims)</param>
        /// <param name="dto">Feedback request data with employee_ids array</param>
        /// <returns>Created feedback request with all recipients</returns>
        /// <exception cref="ArgumentException">If validation fails (duplicate recipients, invalid employees, etc.)</exception>
        /// <exception cref="InvalidOperationException">If rate limit exceeded (50 requests/day)</exception>
        Task<FeedbackRequestDto> CreateAsync(Guid requestorId, CreateFeedbackRequestDto dto);

        /// <summary>
        /// Get a feedback request by ID
        /// </summary>
        /// <param name="id">Feedback request ID</param>
        /// <param name="requestorId">Employee ID to verify ownership</param>
        /// <returns>Feedback request details or null if not found/not authorized</returns>
        Task<FeedbackRequestDto?> GetByIdAsync(Guid id, Guid requestorId);

        /// <summary>
        /// Get paginated list of feedback requests sent by the employee
        /// </summary>
        /// <param name="requestorId">Employee ID who sent the requests</param>
        /// <param name="query">Query parameters for pagination, filtering, sorting</param>
        /// <returns>Paginated list with summary statistics</returns>
        Task<PaginatedFeedbackRequestsDto> GetSentRequestsAsync(Guid requestorId, FeedbackRequestListQuery query);

        /// <summary>
        /// Get paginated list of feedback requests addressed to the employee (todo list)
        /// </summary>
        /// <param name="employeeId">Employee ID who should provide feedback</param>
        /// <param name="query">Query parameters for pagination, filtering, sorting</param>
        /// <returns>Paginated list with summary statistics</returns>
        Task<PaginatedFeedbackRequestsDto> GetTodoRequestsAsync(Guid employeeId, FeedbackRequestListQuery query);

        /// <summary>
        /// Get paginated list of feedback requests sent by team members (manager view)
        /// </summary>
        /// <param name="managerId">Manager's employee ID</param>
        /// <param name="query">Query parameters for pagination, filtering, sorting</param>
        /// <returns>Paginated list of team member requests</returns>
        Task<PaginatedFeedbackRequestsDto> GetTeamSentRequestsAsync(Guid managerId, FeedbackRequestListQuery query);

        /// <summary>
        /// Get paginated list of feedback requests received by team members (manager view)
        /// </summary>
        /// <param name="managerId">Manager's employee ID</param>
        /// <param name="query">Query parameters for pagination, filtering, sorting</param>
        /// <returns>Paginated list of requests addressed to team members</returns>
        Task<PaginatedFeedbackRequestsDto> GetTeamReceivedRequestsAsync(Guid managerId, FeedbackRequestListQuery query);

        /// <summary>
        /// Update a feedback request's due date
        /// </summary>
        /// <param name="id">Feedback request ID</param>
        /// <param name="requestorId">Employee ID to verify ownership</param>
        /// <param name="dto">Updated data (only due_date can be changed)</param>
        /// <returns>Updated feedback request</returns>
        /// <exception cref="ArgumentException">If request not found or not authorized</exception>
        Task<FeedbackRequestDto> UpdateAsync(Guid id, Guid requestorId, UpdateFeedbackRequestDto dto);

        /// <summary>
        /// Cancel entire feedback request (soft delete)
        /// </summary>
        /// <param name="id">Feedback request ID</param>
        /// <param name="requestorId">Employee ID to verify ownership</param>
        /// <returns>Task representing async operation</returns>
        /// <exception cref="ArgumentException">If request not found or not authorized</exception>
        Task CancelRequestAsync(Guid id, Guid requestorId);

        /// <summary>
        /// Cancel a specific recipient within a feedback request
        /// </summary>
        /// <param name="requestId">Feedback request ID</param>
        /// <param name="recipientId">Recipient ID to cancel</param>
        /// <param name="requestorId">Employee ID to verify ownership</param>
        /// <returns>Task representing async operation</returns>
        /// <exception cref="ArgumentException">If request/recipient not found or not authorized</exception>
        Task CancelRecipientAsync(Guid requestId, Guid recipientId, Guid requestorId);

        /// <summary>
        /// Send reminder to a specific recipient
        /// </summary>
        /// <param name="requestId">Feedback request ID</param>
        /// <param name="recipientId">Recipient ID to remind</param>
        /// <param name="requestorId">Employee ID to verify ownership</param>
        /// <returns>Task representing async operation</returns>
        /// <exception cref="ArgumentException">If request/recipient not found or not authorized</exception>
        /// <exception cref="InvalidOperationException">If reminder sent within last 48 hours (cooldown)</exception>
        Task SendReminderAsync(Guid requestId, Guid recipientId, Guid requestorId);

        /// <summary>
        /// Send reminders to all pending recipients who haven't been reminded in 48 hours
        /// </summary>
        /// <param name="requestId">Feedback request ID</param>
        /// <param name="requestorId">Employee ID to verify ownership</param>
        /// <returns>Count of reminders sent</returns>
        /// <exception cref="ArgumentException">If request not found or not authorized</exception>
        Task<int> SendRemindersToAllAsync(Guid requestId, Guid requestorId);

        /// <summary>
        /// Check for duplicate recipients before creating a request
        /// </summary>
        /// <param name="requestorId">Employee ID making the request</param>
        /// <param name="recipientIds">List of recipient employee IDs</param>
        /// <param name="projectId">Optional project ID</param>
        /// <param name="goalId">Optional goal ID</param>
        /// <returns>List of employee IDs that already have active requests</returns>
        Task<List<Guid>> CheckDuplicateRecipientsAsync(Guid requestorId, List<Guid> recipientIds, Guid? projectId, Guid? goalId);

        /// <summary>
        /// Validate that the employee hasn't exceeded daily rate limit (50 requests/day)
        /// </summary>
        /// <param name="requestorId">Employee ID to check</param>
        /// <returns>True if under limit, false if limit exceeded</returns>
        Task<bool> ValidateRateLimitAsync(Guid requestorId);
    }
}
