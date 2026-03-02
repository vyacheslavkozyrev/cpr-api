using System;

namespace CPR.Domain.Entities
{
    public class ReviewResponse : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CycleId { get; set; }
        public Guid NomineeId { get; set; }
        public Guid ReviewerEmployeeId { get; set; }
        public int OverallRating { get; set; }
        public string Comments { get; set; } = null!;

        // Navigation properties
        public virtual ReviewCycle? Cycle { get; set; }
        public virtual ReviewNominee? Nominee { get; set; }
        public virtual Employee? ReviewerEmployee { get; set; }
    }
}
