using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CPR.Application.Services;
using CPR.Domain.Entities;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CPR.Infrastructure.Services
{
    /// <summary>
    /// Email service implementation using MailKit/MimeKit
    /// Feature 0004 - T031: Email notifications with T088: Calendar attachments
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _appBaseUrl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Read SMTP configuration from appsettings
            _smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = _configuration["Email:FromAddress"] ?? "noreply@cpr.local";
            _fromName = _configuration["Email:FromName"] ?? "CPR - Career Progress Registry";
            _appBaseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5173";
        }

        /// <inheritdoc />
        public async Task<bool> SendFeedbackRequestNotificationAsync(
            FeedbackRequest request,
            FeedbackRequestRecipient recipient,
            string requestorName,
            string recipientEmail,
            string? calendarContent = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress(recipient.Employee?.User?.DisplayName ?? "Recipient", recipientEmail));
                message.Subject = $"Feedback Request from {requestorName}";

                // Build email body
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = BuildFeedbackRequestNotificationHtml(request, requestorName, recipient);
                bodyBuilder.TextBody = BuildFeedbackRequestNotificationText(request, requestorName, recipient);

                // Attach calendar file if provided
                if (!string.IsNullOrEmpty(calendarContent))
                {
                    var calendarBytes = Encoding.UTF8.GetBytes(calendarContent);
                    bodyBuilder.Attachments.Add($"feedback-request-{request.Id}.ics", calendarBytes, ContentType.Parse("text/calendar"));
                }

                message.Body = bodyBuilder.ToMessageBody();

                return await SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send feedback request notification to {Email}", recipientEmail);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendFeedbackRequestReminderAsync(
            FeedbackRequest request,
            FeedbackRequestRecipient recipient,
            string requestorName,
            string recipientEmail,
            string? calendarContent = null,
            bool isOverdue = false)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress(recipient.Employee?.User?.DisplayName ?? "Recipient", recipientEmail));
                message.Subject = isOverdue
                    ? $"OVERDUE: Feedback Request from {requestorName}"
                    : $"Reminder: Feedback Request from {requestorName}";

                // Build email body
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = BuildFeedbackRequestReminderHtml(request, requestorName, recipient, isOverdue);
                bodyBuilder.TextBody = BuildFeedbackRequestReminderText(request, requestorName, recipient, isOverdue);

                // Attach calendar file if provided
                if (!string.IsNullOrEmpty(calendarContent))
                {
                    var calendarBytes = Encoding.UTF8.GetBytes(calendarContent);
                    bodyBuilder.Attachments.Add($"feedback-request-{request.Id}.ics", calendarBytes, ContentType.Parse("text/calendar"));
                }

                message.Body = bodyBuilder.ToMessageBody();

                return await SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send feedback request reminder to {Email}", recipientEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(MimeMessage message)
        {
            try
            {
                using var client = new SmtpClient();

                // For development, accept all SSL certificates
                if (_smtpHost == "localhost" || _smtpHost == "127.0.0.1")
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                }

                await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);

                // Authenticate if credentials provided
                if (!string.IsNullOrEmpty(_smtpUsername))
                {
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", message.To));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", message.To));
                return false;
            }
        }

        // ====================================
        // EMAIL TEMPLATE METHODS
        // ====================================

        private string BuildFeedbackRequestNotificationHtml(
            FeedbackRequest request,
            string requestorName,
            FeedbackRequestRecipient recipient)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head><meta charset='utf-8'><title>Feedback Request</title></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>");

            sb.AppendLine($"<h2 style='color: #1976d2;'>New Feedback Request</h2>");
            sb.AppendLine($"<p>Hello,</p>");
            sb.AppendLine($"<p><strong>{requestorName}</strong> has requested your feedback to support their professional development.</p>");

            if (!string.IsNullOrEmpty(request.Message))
            {
                sb.AppendLine("<div style='background: #f5f5f5; padding: 15px; border-left: 4px solid #1976d2; margin: 20px 0;'>");
                sb.AppendLine($"<p style='margin: 0;'><strong>Message:</strong></p>");
                sb.AppendLine($"<p style='margin: 10px 0 0 0;'>{request.Message}</p>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>");

            if (request.Project != null)
            {
                sb.AppendLine("<tr><td style='padding: 8px 0;'><strong>Project:</strong></td>");
                sb.AppendLine($"<td style='padding: 8px 0;'>{request.Project.Title}</td></tr>");
            }

            if (request.Goal != null)
            {
                sb.AppendLine("<tr><td style='padding: 8px 0;'><strong>Goal:</strong></td>");
                sb.AppendLine($"<td style='padding: 8px 0;'>{request.Goal.Title}</td></tr>");
            }

            if (request.DueDate.HasValue)
            {
                sb.AppendLine("<tr><td style='padding: 8px 0;'><strong>Due Date:</strong></td>");
                sb.AppendLine($"<td style='padding: 8px 0;'>{request.DueDate.Value:MMMM dd, yyyy}</td></tr>");
            }

            sb.AppendLine("</table>");

            var respondUrl = $"{_appBaseUrl}/feedback/provide?request_id={request.Id}&recipient_id={recipient.Id}";
            sb.AppendLine($"<p style='margin: 30px 0;'>");
            sb.AppendLine($"<a href='{respondUrl}' style='background: #1976d2; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Provide Feedback</a>");
            sb.AppendLine("</p>");

            sb.AppendLine("<p style='color: #666; font-size: 12px; margin-top: 40px; border-top: 1px solid #ddd; padding-top: 20px;'>");
            sb.AppendLine("This is an automated message from CPR - Career Progress Registry. Please do not reply to this email.");
            sb.AppendLine("</p>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string BuildFeedbackRequestNotificationText(
            FeedbackRequest request,
            string requestorName,
            FeedbackRequestRecipient recipient)
        {
            var sb = new StringBuilder();
            sb.AppendLine("NEW FEEDBACK REQUEST");
            sb.AppendLine("====================");
            sb.AppendLine();
            sb.AppendLine($"{requestorName} has requested your feedback to support their professional development.");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(request.Message))
            {
                sb.AppendLine("Message:");
                sb.AppendLine(request.Message);
                sb.AppendLine();
            }

            if (request.Project != null)
            {
                sb.AppendLine($"Project: {request.Project.Title}");
            }

            if (request.Goal != null)
            {
                sb.AppendLine($"Goal: {request.Goal.Title}");
            }

            if (request.DueDate.HasValue)
            {
                sb.AppendLine($"Due Date: {request.DueDate.Value:MMMM dd, yyyy}");
            }

            sb.AppendLine();
            var respondUrl = $"{_appBaseUrl}/feedback/provide?request_id={request.Id}&recipient_id={recipient.Id}";
            sb.AppendLine($"Provide feedback: {respondUrl}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("This is an automated message from CPR - Career Progress Registry.");

            return sb.ToString();
        }

        private string BuildFeedbackRequestReminderHtml(
            FeedbackRequest request,
            string requestorName,
            FeedbackRequestRecipient recipient,
            bool isOverdue)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head><meta charset='utf-8'><title>Feedback Request Reminder</title></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>");

            if (isOverdue)
            {
                sb.AppendLine($"<h2 style='color: #d32f2f;'>⚠️ OVERDUE: Feedback Request</h2>");
                sb.AppendLine($"<p>Hello,</p>");
                sb.AppendLine($"<p>This is a reminder that you have an <strong>overdue feedback request</strong> from <strong>{requestorName}</strong>.</p>");
            }
            else
            {
                sb.AppendLine($"<h2 style='color: #1976d2;'>Reminder: Feedback Request</h2>");
                sb.AppendLine($"<p>Hello,</p>");
                sb.AppendLine($"<p>This is a friendly reminder that <strong>{requestorName}</strong> is waiting for your feedback.</p>");
            }

            if (!string.IsNullOrEmpty(request.Message))
            {
                sb.AppendLine("<div style='background: #f5f5f5; padding: 15px; border-left: 4px solid #1976d2; margin: 20px 0;'>");
                sb.AppendLine($"<p style='margin: 0;'><strong>Message:</strong></p>");
                sb.AppendLine($"<p style='margin: 10px 0 0 0;'>{request.Message}</p>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>");

            if (request.Project != null)
            {
                sb.AppendLine("<tr><td style='padding: 8px 0;'><strong>Project:</strong></td>");
                sb.AppendLine($"<td style='padding: 8px 0;'>{request.Project.Title}</td></tr>");
            }

            if (request.Goal != null)
            {
                sb.AppendLine("<tr><td style='padding: 8px 0;'><strong>Goal:</strong></td>");
                sb.AppendLine($"<td style='padding: 8px 0;'>{request.Goal.Title}</td></tr>");
            }

            if (request.DueDate.HasValue)
            {
                var color = isOverdue ? "#d32f2f" : "#333";
                sb.AppendLine($"<tr><td style='padding: 8px 0;'><strong>Due Date:</strong></td>");
                sb.AppendLine($"<td style='padding: 8px 0; color: {color};'><strong>{request.DueDate.Value:MMMM dd, yyyy}</strong></td></tr>");
            }

            sb.AppendLine("</table>");

            var respondUrl = $"{_appBaseUrl}/feedback/provide?request_id={request.Id}&recipient_id={recipient.Id}";
            sb.AppendLine($"<p style='margin: 30px 0;'>");
            sb.AppendLine($"<a href='{respondUrl}' style='background: {(isOverdue ? "#d32f2f" : "#1976d2")}; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Provide Feedback Now</a>");
            sb.AppendLine("</p>");

            sb.AppendLine("<p style='color: #666; font-size: 12px; margin-top: 40px; border-top: 1px solid #ddd; padding-top: 20px;'>");
            sb.AppendLine("This is an automated message from CPR - Career Progress Registry. Please do not reply to this email.");
            sb.AppendLine("</p>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string BuildFeedbackRequestReminderText(
            FeedbackRequest request,
            string requestorName,
            FeedbackRequestRecipient recipient,
            bool isOverdue)
        {
            var sb = new StringBuilder();

            if (isOverdue)
            {
                sb.AppendLine("OVERDUE: FEEDBACK REQUEST REMINDER");
                sb.AppendLine("===================================");
                sb.AppendLine();
                sb.AppendLine($"This is a reminder that you have an OVERDUE feedback request from {requestorName}.");
            }
            else
            {
                sb.AppendLine("FEEDBACK REQUEST REMINDER");
                sb.AppendLine("=========================");
                sb.AppendLine();
                sb.AppendLine($"This is a friendly reminder that {requestorName} is waiting for your feedback.");
            }

            sb.AppendLine();

            if (!string.IsNullOrEmpty(request.Message))
            {
                sb.AppendLine("Message:");
                sb.AppendLine(request.Message);
                sb.AppendLine();
            }

            if (request.Project != null)
            {
                sb.AppendLine($"Project: {request.Project.Title}");
            }

            if (request.Goal != null)
            {
                sb.AppendLine($"Goal: {request.Goal.Title}");
            }

            if (request.DueDate.HasValue)
            {
                sb.AppendLine($"Due Date: {request.DueDate.Value:MMMM dd, yyyy}{(isOverdue ? " (OVERDUE)" : "")}");
            }

            sb.AppendLine();
            var respondUrl = $"{_appBaseUrl}/feedback/provide?request_id={request.Id}&recipient_id={recipient.Id}";
            sb.AppendLine($"Provide feedback: {respondUrl}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("This is an automated message from CPR - Career Progress Registry.");

            return sb.ToString();
        }
    }
}
