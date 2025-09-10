using System;

namespace CPR.Domain.Entities
{
    public class Location : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? Timezone { get; set; }
        public string? ContactPhone { get; set; }
    }
}
