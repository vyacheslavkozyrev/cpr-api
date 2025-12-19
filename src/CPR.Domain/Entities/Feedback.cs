using System;

namespace CPR.Domain.Entities
{
    public class Feedback : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid? GoalId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid FromEmployeeId { get; set; }
        public Guid ToEmployeeId { get; set; }
        public string Content { get; set; } = null!;
        public int? Rating { get; set; }

        /// <summary>
        /// Optional link to feedback request that prompted this feedback
        /// </summary>
        public Guid? FeedbackRequestId { get; set; }

        // Navigation property
        public virtual FeedbackRequest? FeedbackRequest { get; set; }
    }
}
