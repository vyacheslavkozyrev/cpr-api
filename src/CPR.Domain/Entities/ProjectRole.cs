using System;

namespace CPR.Domain.Entities
{
    public class ProjectRole : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
