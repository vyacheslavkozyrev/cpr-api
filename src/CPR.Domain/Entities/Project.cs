using System;

namespace CPR.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? OwnerId { get; set; }
        public Guid? SponsorId { get; set; }
    }
}
