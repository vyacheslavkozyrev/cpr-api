using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace CPR.Infrastructure.Jobs
{
    /// <summary>
    /// Background job for sending automatic reminders for pending feedback requests
    /// Feature 0004 - T084: Automatic reminders
    /// </summary>
    public class FeedbackRequestReminderJob
    {
        private readonly IFeedbackRequestRepository _repository;
        private readonly CPR.Application.Services.IEmailService _emailService;
        private readonly CPR.Application.Services.ICalendarService _calendarService;
        private readonly ILogger<FeedbackRequestReminderJob> _logger;
        private static readonly TimeSpan ReminderCooldown = TimeSpan.FromHours(48);

        public FeedbackRequestReminderJob(
            IFeedbackRequestRepository repository,
            CPR.Application.Services.IEmailService emailService,
            CPR.Application.Services.ICalendarService calendarService,
            ILogger<FeedbackRequestReminderJob> logger)
        {
            _repository = repository;
            _emailService = emailService;
            _calendarService = calendarService;
            _logger = logger;
        }

        /// <summary>
        /// Send reminders for feedback requests that are approaching their due date
        /// Runs daily to check for requests due within the next 2 days
        /// </summary>
        public async Task SendUpcomingDueDateRemindersAsync()
        {
            _logger.LogInformation("Starting upcoming due date reminders job");

            try
            {
                var now = DateTimeOffset.UtcNow;
                var twoDaysFromNow = now.AddDays(2);

                // Get all active feedback requests with due dates in the next 2 days
                var upcomingRequests = await _repository.GetRequestsDueWithinAsync(
                    now,
                    twoDaysFromNow,
                    includeDeleted: false);

                int remindersSent = 0;

                foreach (var request in upcomingRequests)
                {
                    if (request.Recipients == null || !request.Recipients.Any())
                        continue;

                    // Find recipients eligible for reminders
                    var eligibleRecipients = request.Recipients
                        .Where(r => !r.IsCompleted &&
                                   (!r.LastReminderAt.HasValue ||
                                    (now - r.LastReminderAt.Value) >= ReminderCooldown))
                        .ToList();

                    foreach (var recipient in eligibleRecipients)
                    {
                        try
                        {
                            // Update last reminder timestamp
                            await _repository.UpdateLastReminderAsync(recipient.Id, now);

                            // Send reminder email with calendar attachment (T031/T088)
                            var recipientEmail = recipient.Employee?.User?.UserName;
                            if (!string.IsNullOrEmpty(recipientEmail))
                            {
                                var requestorName = request.Requestor?.User?.DisplayName ?? "Unknown";
                                var calendarContent = await _calendarService.GenerateFeedbackRequestCalendarAsync(
                                    request.Id,
                                    recipient.EmployeeId,
                                    requestorName,
                                    recipient.Employee?.User?.DisplayName ?? "Unknown",
                                    request.Message,
                                    request.DueDate ?? DateTimeOffset.UtcNow.AddDays(7),
                                    request.Project?.Title,
                                    request.Goal?.Title
                                );

                                await _emailService.SendFeedbackRequestReminderAsync(
                                    request,
                                    recipient,
                                    requestorName,
                                    recipientEmail,
                                    calendarContent,
                                    isOverdue: false
                                );
                            }

                            remindersSent++;

                            _logger.LogInformation(
                                "Sent upcoming due date reminder for request {RequestId} to recipient {RecipientId}",
                                request.Id,
                                recipient.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Failed to send reminder for request {RequestId} to recipient {RecipientId}",
                                request.Id,
                                recipient.Id);
                        }
                    }
                }

                _logger.LogInformation(
                    "Upcoming due date reminders job completed. Sent {Count} reminders",
                    remindersSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in upcoming due date reminders job");
                throw;
            }
        }

        /// <summary>
        /// Send reminders for feedback requests that are overdue
        /// Runs daily to check for requests that are past their due date
        /// </summary>
        public async Task SendOverdueRemindersAsync()
        {
            _logger.LogInformation("Starting overdue reminders job");

            try
            {
                var now = DateTimeOffset.UtcNow;

                // Get all active feedback requests that are overdue
                var overdueRequests = await _repository.GetOverdueRequestsAsync(
                    now,
                    includeDeleted: false);

                int remindersSent = 0;

                foreach (var request in overdueRequests)
                {
                    if (request.Recipients == null || !request.Recipients.Any())
                        continue;

                    // Find recipients eligible for reminders
                    var eligibleRecipients = request.Recipients
                        .Where(r => !r.IsCompleted &&
                                   (!r.LastReminderAt.HasValue ||
                                    (now - r.LastReminderAt.Value) >= ReminderCooldown))
                        .ToList();

                    foreach (var recipient in eligibleRecipients)
                    {
                        try
                        {
                            // Update last reminder timestamp
                            await _repository.UpdateLastReminderAsync(recipient.Id, now);

                            // Send overdue reminder email with calendar attachment (T031/T088)
                            var recipientEmail = recipient.Employee?.User?.UserName;
                            if (!string.IsNullOrEmpty(recipientEmail))
                            {
                                var requestorName = request.Requestor?.User?.DisplayName ?? "Unknown";
                                var calendarContent = await _calendarService.GenerateFeedbackRequestCalendarAsync(
                                    request.Id,
                                    recipient.EmployeeId,
                                    requestorName,
                                    recipient.Employee?.User?.DisplayName ?? "Unknown",
                                    request.Message,
                                    request.DueDate ?? DateTimeOffset.UtcNow.AddDays(7),
                                    request.Project?.Title,
                                    request.Goal?.Title
                                );

                                await _emailService.SendFeedbackRequestReminderAsync(
                                    request,
                                    recipient,
                                    requestorName,
                                    recipientEmail,
                                    calendarContent,
                                    isOverdue: true
                                );
                            }

                            remindersSent++;

                            _logger.LogInformation(
                                "Sent overdue reminder for request {RequestId} to recipient {RecipientId}",
                                request.Id,
                                recipient.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Failed to send overdue reminder for request {RequestId} to recipient {RecipientId}",
                                request.Id,
                                recipient.Id);
                        }
                    }
                }

                _logger.LogInformation(
                    "Overdue reminders job completed. Sent {Count} reminders",
                    remindersSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in overdue reminders job");
                throw;
            }
        }
    }
}
