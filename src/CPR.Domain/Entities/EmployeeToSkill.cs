using System;

namespace CPR.Domain.Entities
{
    public class EmployeeToSkill : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid SkillId { get; set; }
        public Guid? SkillLevelId { get; set; }
        public decimal? PersistValue { get; set; }
        public string? Source { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public bool IsTarget { get; set; }
    }
}
