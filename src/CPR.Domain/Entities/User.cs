using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    public class User : AuditableEntity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? EntraExternalId { get; set; }
        public string? DisplayName { get; set; }

        // Navigation properties
        public ICollection<UserToRole> UserRoles { get; set; } = new List<UserToRole>();
    }
}
