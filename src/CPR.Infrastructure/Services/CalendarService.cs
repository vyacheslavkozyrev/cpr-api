using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CPR.Application.Services;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for generating calendar files using Ical.Net
    /// Feature 0004 - T085: Calendar file generation
    /// </summary>
    public class CalendarService : ICalendarService
    {
        private const string ProductId = "-//Career Progress Registry//Feedback Request//EN";

        /// <inheritdoc />
        public Task<string> GenerateFeedbackRequestCalendarAsync(
            Guid feedbackRequestId,
            Guid recipientEmployeeId,
            string requestorName,
            string recipientName,
            string? message,
            DateTimeOffset dueDate,
            string? projectName = null,
            string? goalTitle = null)
        {
            // Create calendar
            var calendar = new Calendar
            {
                ProductId = ProductId,
                Version = "2.0"
            };

            // Build event summary (title)
            var summary = $"Feedback Request: {requestorName} → {recipientName}";

            // Build event description
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine($"Feedback requested by: {requestorName}");
            descriptionBuilder.AppendLine($"Recipient: {recipientName}");
            descriptionBuilder.AppendLine();

            if (!string.IsNullOrWhiteSpace(message))
            {
                descriptionBuilder.AppendLine("Message:");
                descriptionBuilder.AppendLine(message);
                descriptionBuilder.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(projectName))
            {
                descriptionBuilder.AppendLine($"Project: {projectName}");
            }

            if (!string.IsNullOrWhiteSpace(goalTitle))
            {
                descriptionBuilder.AppendLine($"Goal: {goalTitle}");
            }

            descriptionBuilder.AppendLine();
            descriptionBuilder.AppendLine($"Request ID: {feedbackRequestId}");
            descriptionBuilder.AppendLine();
            descriptionBuilder.AppendLine("Please provide your feedback by the due date.");

            // Create event
            var calendarEvent = new CalendarEvent
            {
                Uid = $"feedback-request-{feedbackRequestId}-{recipientEmployeeId}@cpr.local",
                Summary = summary,
                Description = descriptionBuilder.ToString(),
                Start = new CalDateTime(dueDate.DateTime),
                End = new CalDateTime(dueDate.DateTime.AddHours(1)), // 1-hour event
                Status = EventStatus.Confirmed,
                Priority = 5, // Medium priority
                IsAllDay = false
            };

            // Add alarm reminder 24 hours before due date
            var alarm = new Alarm
            {
                Trigger = new Trigger(TimeSpan.FromHours(-24)),
                Action = AlarmAction.Display,
                Description = $"Reminder: Feedback due for {requestorName}"
            };
            calendarEvent.Alarms.Add(alarm);

            // Add event to calendar
            calendar.Events.Add(calendarEvent);

            // Serialize to iCalendar format
            var serializer = new CalendarSerializer();
            var icsContent = serializer.SerializeToString(calendar);

            return Task.FromResult(icsContent);
        }
    }
}
