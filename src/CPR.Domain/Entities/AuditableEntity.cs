using System;

namespace CPR.Domain.Entities
{
    public abstract class AuditableEntity
    {
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
