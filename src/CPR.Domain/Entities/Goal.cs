using System;

namespace CPR.Domain.Entities
{
    public class Goal : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "open"; // open, in_progress, completed
    }
}
