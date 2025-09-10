using System;

namespace CPR.Domain.Entities
{
    public class SkillLevel : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid SkillId { get; set; }
        public int Value { get; set; }
    }
}
