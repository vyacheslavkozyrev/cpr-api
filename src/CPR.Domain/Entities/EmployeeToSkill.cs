using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    public class EmployeeToSkill : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid SkillId { get; set; }
        public Guid? SkillLevelId { get; set; }
        public decimal SelfAssessmentValue { get; set; }
        public decimal? ManagerAssessmentValue { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ICollection<EmployeeSkillEvidence> Evidence { get; set; } = new List<EmployeeSkillEvidence>();
    }
}
