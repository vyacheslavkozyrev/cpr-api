using System;

namespace CPR.Domain.Entities
{
    public class CareerPath : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
