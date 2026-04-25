using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for feedback request operations
    /// </summary>
    public interface IFeedbackService
    {
        /// <summary>
        /// Create a new feedback request
        /// </summary>
        /// <param name="requestorId">Employee ID making the request</param>
        /// <param name="dto">Feedback request data</param>
        /// <returns>Created feedback request details</returns>
        Task<FeedbackRequestDto> CreateFeedbackRequestAsync(Guid requestorId, CreateFeedbackRequestDto dto);

        /// <summary>
        /// Get feedback requests sent by the specified employee
        /// </summary>
        /// <param name="requestorId">Employee ID who sent the requests</param>
        /// <returns>List of feedback requests sent by the employee</returns>
        Task<IEnumerable<FeedbackRequestDto>> GetSentRequestsAsync(Guid requestorId);

        /// <summary>
        /// Get feedback requests addressed to the specified employee (to respond to)
        /// </summary>
        /// <param name="employeeId">Employee ID who should respond to the requests</param>
        /// <returns>List of feedback requests addressed to the employee</returns>
        Task<IEnumerable<FeedbackRequestDto>> GetTodoRequestsAsync(Guid employeeId);

        /// <summary>
        /// Submit feedback from one employee to another
        /// </summary>
        /// <param name="fromEmployeeId">Employee ID providing the feedback</param>
        /// <param name="dto">Feedback data</param>
        /// <returns>Created feedback details</returns>
        Task<FeedbackDto> SubmitFeedbackAsync(Guid fromEmployeeId, SubmitFeedbackRequestDto dto);

        /// <summary>
        /// Get all feedback addressed to the specified employee
        /// </summary>
        /// <param name="employeeId">Employee ID to get feedback for</param>
        /// <returns>List of feedback addressed to the employee</returns>
        Task<IEnumerable<FeedbackDto>> GetFeedbackForEmployeeAsync(Guid employeeId);

        /// <summary>
        /// Get all feedback addressed to the current user (excludes redundant ToEmployee info)
        /// </summary>
        /// <param name="employeeId">Employee ID to get feedback for</param>
        /// <returns>List of feedback addressed to the current user</returns>
        Task<IEnumerable<MyFeedbackDto>> GetMyFeedbackAsync(Guid employeeId);

        /// <summary>
        /// Get a specific feedback by ID
        /// </summary>
        /// <param name="feedbackId">Feedback ID</param>
        /// <param name="requestingEmployeeId">Employee ID making the request</param>
        /// <returns>Feedback details</returns>
        Task<FeedbackDto?> GetFeedbackByIdAsync(Guid feedbackId, Guid requestingEmployeeId);

        /// <summary>
        /// Get feedback analytics for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID to get analytics for</param>
        /// <param name="dateFrom">Start date (ISO 8601 format)</param>
        /// <param name="dateTo">End date (ISO 8601 format)</param>
        /// <param name="includeComparison">Include comparison with previous period</param>
        /// <returns>Analytics data</returns>
        Task<FeedbackAnalyticsDto> GetFeedbackAnalyticsAsync(Guid employeeId, string dateFrom, string dateTo, bool includeComparison);

        /// <summary>
        /// Get all feedback received by an employee for display in a manager's dashboard view.
        /// Results are sorted newest-first. Manager must be the direct manager of the employee.
        /// Maps: feedback.content → comment; feedback.from_employee_id → submitted_by_id.
        /// </summary>
        Task<ManagerViewFeedbackDto[]> GetEmployeeFeedbackForManagerAsync(Guid employeeId, Guid managerEmployeeId);
    }
}