using System;
using System.Threading.Tasks;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for generating calendar files (.ics) for feedback requests
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// Generate an iCalendar (.ics) file for a feedback request
        /// </summary>
        /// <param name="feedbackRequestId">Feedback request ID</param>
        /// <param name="recipientEmployeeId">Recipient employee ID (to include in event)</param>
        /// <param name="requestorName">Name of the person requesting feedback</param>
        /// <param name="recipientName">Name of the recipient</param>
        /// <param name="message">Request message</param>
        /// <param name="dueDate">Due date for the feedback</param>
        /// <param name="projectName">Optional project name</param>
        /// <param name="goalTitle">Optional goal title</param>
        /// <returns>iCalendar content as string</returns>
        Task<string> GenerateFeedbackRequestCalendarAsync(
            Guid feedbackRequestId,
            Guid recipientEmployeeId,
            string requestorName,
            string recipientName,
            string? message,
            DateTimeOffset dueDate,
            string? projectName = null,
            string? goalTitle = null);
    }
}
