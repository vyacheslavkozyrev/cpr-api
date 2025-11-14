using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    /// <summary>
    /// Represents a feedback request from an employee to one or more recipients.
    /// Parent entity in multi-recipient architecture; stores shared context (message, due date, project/goal).
    /// </summary>
    public class FeedbackRequest : AuditableEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Employee who requested the feedback
        /// </summary>
        public Guid RequestorId { get; set; }

        /// <summary>
        /// Optional project context for the feedback request
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Optional goal context for the feedback request
        /// </summary>
        public Guid? GoalId { get; set; }

        /// <summary>
        /// Optional message explaining what feedback is being requested (max 500 characters)
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Optional due date for feedback response (DATE type, not timestamp)
        /// </summary>
        public DateTime? DueDate { get; set; }

        // Navigation properties
        public virtual Employee Requestor { get; set; } = null!;
        public virtual Project? Project { get; set; }
        public virtual Goal? Goal { get; set; }

        /// <summary>
        /// Collection of recipients (1-20) with individual status tracking
        /// </summary>
        public virtual ICollection<FeedbackRequestRecipient> Recipients { get; set; } = new List<FeedbackRequestRecipient>();
    }
}
