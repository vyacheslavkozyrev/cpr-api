using System;

namespace CPR.Domain.Entities
{
    /// <summary>
    /// Junction table entity tracking individual recipient status within a feedback request.
    /// Enables 1-20 recipients per request with per-recipient completion tracking.
    /// </summary>
    public class FeedbackRequestRecipient
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Parent feedback request
        /// </summary>
        public Guid FeedbackRequestId { get; set; }

        /// <summary>
        /// Recipient employee
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Whether this recipient has responded or been cancelled
        /// False = Pending, True = Responded or Cancelled
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// When the recipient submitted feedback (NULL if pending/cancelled)
        /// </summary>
        public DateTimeOffset? RespondedAt { get; set; }

        /// <summary>
        /// Last time a manual reminder was sent to this recipient (48h cooldown enforcement)
        /// </summary>
        public DateTimeOffset? LastReminderAt { get; set; }

        /// <summary>
        /// When this recipient was added to the request
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Last status update timestamp (auto-managed by trigger)
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual FeedbackRequest FeedbackRequest { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;
    }
}
