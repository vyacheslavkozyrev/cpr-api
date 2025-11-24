using System;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    public class RoleDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class UserRoleDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
        [JsonPropertyName("role_id")]
        public Guid RoleId { get; set; }
        [JsonPropertyName("role_title")]
        public string RoleTitle { get; set; } = null!;
        [JsonPropertyName("role_description")]
        public string? RoleDescription { get; set; }
        [JsonPropertyName("assigned_at")]
        public DateTimeOffset AssignedAt { get; set; }
    }

    public class AssignRoleDto
    {
        [JsonPropertyName("role_title")]
        public string RoleTitle { get; set; } = null!;
    }

    public class UserRolesDto
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = null!;
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
        [JsonPropertyName("roles")]
        public List<UserRoleDto> Roles { get; set; } = new();
    }
}