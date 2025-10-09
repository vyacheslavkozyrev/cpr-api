using System;
using System.Collections.Generic;

namespace CPR.Application.Contracts
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? OwnerId { get; set; }
        public Guid? SponsorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public List<ProjectRoleDto> Roles { get; set; } = new();
        public List<ProjectTeamDto> Team { get; set; } = new();
    }
}
