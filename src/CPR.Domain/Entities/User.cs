using System;

namespace CPR.Domain.Entities
{
    public class User : AuditableEntity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? DisplayName { get; set; }
    }
}
