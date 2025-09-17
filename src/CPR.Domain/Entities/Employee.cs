using System;

namespace CPR.Domain.Entities
{
    public class Employee : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ManagerId { get; set; }
        public string? Title { get; set; }
        public string? Department { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }
}
