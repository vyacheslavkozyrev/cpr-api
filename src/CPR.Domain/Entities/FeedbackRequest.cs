using System;

namespace CPR.Domain.Entities
{
    public class FeedbackRequest : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid RequestorId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? GoalId { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset? DueDate { get; set; }
    }
}
// Audit
