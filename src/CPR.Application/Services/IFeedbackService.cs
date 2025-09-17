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
    }
}