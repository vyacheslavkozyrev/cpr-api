using System;

namespace CPR.Domain.Entities
{
    public class PositionToSkill : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid PositionId { get; set; }
        public Guid SkillId { get; set; }
        public Guid SkillLevelId { get; set; }
        public decimal? Weight { get; set; }
        public bool IsMandatory { get; set; }
        public string? Rationale { get; set; }
    }
}
