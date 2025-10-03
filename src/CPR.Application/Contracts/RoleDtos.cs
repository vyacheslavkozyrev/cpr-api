using System;

namespace CPR.Application.Contracts
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class UserRoleDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string RoleTitle { get; set; } = null!;
        public string? RoleDescription { get; set; }
        public DateTimeOffset AssignedAt { get; set; }
    }

    public class AssignRoleDto
    {
        public string RoleTitle { get; set; } = null!;
    }

    public class UserRolesDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
        public List<UserRoleDto> Roles { get; set; } = new();
    }
}