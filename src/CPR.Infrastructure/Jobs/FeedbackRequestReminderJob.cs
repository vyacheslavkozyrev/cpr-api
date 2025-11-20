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
        private readonly ILogger<FeedbackRequestReminderJob> _logger;
        private static readonly TimeSpan ReminderCooldown = TimeSpan.FromHours(48);

        public FeedbackRequestReminderJob(
            IFeedbackRequestRepository repository,
            ILogger<FeedbackRequestReminderJob> logger)
        {
            _repository = repository;
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
                        // Update last reminder timestamp
                        await _repository.UpdateLastReminderAsync(recipient.Id, now);

                        // TODO: Queue email notification job here (T031/T088)
                        // await _emailService.SendReminderEmailAsync(request, recipient);

                        remindersSent++;

                        _logger.LogInformation(
                            "Sent upcoming due date reminder for request {RequestId} to recipient {RecipientId}",
                            request.Id,
                            recipient.Id);
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
                        // Update last reminder timestamp
                        await _repository.UpdateLastReminderAsync(recipient.Id, now);

                        // TODO: Queue email notification job here (T031/T088)
                        // await _emailService.SendOverdueReminderEmailAsync(request, recipient);

                        remindersSent++;

                        _logger.LogInformation(
                            "Sent overdue reminder for request {RequestId} to recipient {RecipientId}",
                            request.Id,
                            recipient.Id);
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
