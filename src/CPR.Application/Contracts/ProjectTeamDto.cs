using System;

namespace CPR.Application.Contracts
{
    public class ProjectTeamDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ProjectRoleId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}
