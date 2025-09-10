using System;

namespace CPR.Domain.Entities
{
    public class GoalTask : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid GoalId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}
