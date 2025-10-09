using System;

namespace CPR.Application.Contracts
{
    public class ProjectRoleDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}
