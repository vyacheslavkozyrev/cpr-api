using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    public class ReviewCycle : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid SubjectEmployeeId { get; set; }
        public Guid DepartmentId { get; set; }
        public ReviewCycleStatus Status { get; set; }
        public DateTimeOffset? OpenedAt { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }

        // Navigation properties
        public virtual Employee? SubjectEmployee { get; set; }
        public virtual ICollection<ReviewNominee> Nominees { get; set; } = new List<ReviewNominee>();
    }
}
