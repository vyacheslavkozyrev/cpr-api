using System.Collections.Generic;
using Newtonsoft.Json;
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
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Position title.
    /// </summary>
    [JsonProperty("title")]
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
    [JsonProperty("user_id")]
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Employee identifier (GUID string).
    /// </summary>
    [JsonProperty("employee_id")]
    [JsonPropertyName("employee_id")]
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Username/login.
    /// </summary>
    [JsonProperty("user_name")]
    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the user.
    /// </summary>
    [JsonProperty("display_name")]
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Email address for the user (extracted from JWT claims).
    /// </summary>
    [JsonProperty("email")]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Current position information.
    /// </summary>
    [JsonProperty("position")]
    [JsonPropertyName("position")]
    public Position Position { get; set; } = new Position();

    /// <summary>
    /// System roles assigned to this user (e.g. "Employee", "People Manager").
    /// </summary>
    [JsonProperty("roles")]
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new List<string>();
}
