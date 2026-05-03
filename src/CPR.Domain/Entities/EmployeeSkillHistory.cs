using System;

namespace CPR.Domain.Entities
{
    /// <summary>
    /// Immutable history snapshot of an employee's skill assessment values.
    /// Rows are append-only and must never be modified or deleted (AC-030).
    /// </summary>
    public class EmployeeSkillHistory : AuditableEntity
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Foreign key to the employee who owns this skill assessment.</summary>
        public Guid EmployeeId { get; set; }

        /// <summary>Foreign key to the assessed skill.</summary>
        public Guid SkillId { get; set; }

        /// <summary>Self-assessment value captured at the time of change.</summary>
        public decimal SelfAssessmentValue { get; set; }

        /// <summary>Manager assessment value at the time of change; null if not yet assessed.</summary>
        public decimal? ManagerAssessmentValue { get; set; }

        /// <summary>Timestamp when the change occurred (set by application at write time).</summary>
        public DateTimeOffset RecordedAt { get; set; }
    }
}
