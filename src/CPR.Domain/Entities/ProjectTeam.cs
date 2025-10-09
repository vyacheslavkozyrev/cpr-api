using System;

namespace CPR.Domain.Entities
{
    public class ProjectTeam : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectRoleId { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
