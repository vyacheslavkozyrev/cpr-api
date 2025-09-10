using System;

namespace CPR.Domain.Entities
{
    public class Skill : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
    }
}
