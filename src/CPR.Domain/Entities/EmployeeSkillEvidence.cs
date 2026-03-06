using System;

namespace CPR.Domain.Entities
{
    public class EmployeeSkillEvidence : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid EmployeeToSkillId { get; set; }
        public Guid FeedbackId { get; set; }

        // Navigation properties
        public virtual EmployeeToSkill? EmployeeToSkill { get; set; }
        public virtual Feedback? Feedback { get; set; }
    }
}
