using System.Text.Json.Serialization;

namespace CPR.Api.Models;

/// <summary>
/// A simple position DTO included in the user profile.
/// </summary>
public class Position
{
    /// <summary>
    /// Position identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Position title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Minimal user profile returned by the API.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// User identifier (GUID string).
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Employee identifier (GUID string).
    /// </summary>
    [JsonPropertyName("employee_id")]
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Username/login.
    /// </summary>
    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the user.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Email address for the user (extracted from JWT claims).
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Current position information.
    /// </summary>
    [JsonPropertyName("position")]
    public Position Position { get; set; } = new Position();
}
