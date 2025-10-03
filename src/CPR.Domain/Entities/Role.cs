using System;
using System.Collections.Generic;

namespace CPR.Domain.Entities
{
    public class Role : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<UserToRole> UserRoles { get; set; } = new List<UserToRole>();
    }
}