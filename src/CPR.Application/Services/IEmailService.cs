using System;
using System.Threading.Tasks;
using CPR.Domain.Entities;

namespace CPR.Application.Services
{
    /// <summary>
    /// Service interface for sending emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send notification email when a new feedback request is created
        /// </summary>
        /// <param name="request">The feedback request</param>
        /// <param name="recipient">The recipient employee</param>
        /// <param name="requestorName">Name of person requesting feedback</param>
        /// <param name="recipientEmail">Recipient's email address</param>
        /// <param name="calendarContent">Optional .ics calendar file content</param>
        /// <returns>True if email was sent successfully</returns>
        Task<bool> SendFeedbackRequestNotificationAsync(
            FeedbackRequest request,
            FeedbackRequestRecipient recipient,
            string requestorName,
            string recipientEmail,
            string? calendarContent = null);

        /// <summary>
        /// Send reminder email for a pending feedback request
        /// </summary>
        /// <param name="request">The feedback request</param>
        /// <param name="recipient">The recipient employee</param>
        /// <param name="requestorName">Name of person requesting feedback</param>
        /// <param name="recipientEmail">Recipient's email address</param>
        /// <param name="calendarContent">Optional .ics calendar file content</param>
        /// <param name="isOverdue">Whether the request is overdue</param>
        /// <returns>True if email was sent successfully</returns>
        Task<bool> SendFeedbackRequestReminderAsync(
            FeedbackRequest request,
            FeedbackRequestRecipient recipient,
            string requestorName,
            string recipientEmail,
            string? calendarContent = null,
            bool isOverdue = false);
    }
}
