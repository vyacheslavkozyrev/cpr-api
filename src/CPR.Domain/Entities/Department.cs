using System;

namespace CPR.Domain.Entities
{
    public class Department : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Guid? ParentDepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
    }
}
