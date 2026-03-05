using System;

namespace CPR.Domain.Entities
{
    public class ReviewNominee : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CycleId { get; set; }
        public Guid ReviewerEmployeeId { get; set; }
        public Guid NominatedBy { get; set; }
        public ReviewNomineeStatus Status { get; set; }

        // Navigation properties
        public virtual ReviewCycle? Cycle { get; set; }
        public virtual Employee? ReviewerEmployee { get; set; }
        public virtual Employee? NominatedByEmployee { get; set; }
        public virtual ReviewResponse? Response { get; set; }
    }
}
