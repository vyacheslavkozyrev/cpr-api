using System;

namespace CPR.Domain.Entities
{
    public class AuditLog : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Action { get; set; } = null!;
        public string? TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public string? Detail { get; set; } // json stored as string by default
    }
}
