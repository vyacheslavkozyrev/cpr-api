using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Request body for PATCH /api/goals/{id}/suggestion (employee accepts or rejects a suggested goal).
    /// </summary>
    public class GoalSuggestionActionDto
    {
        /// <summary>Action: accept | reject</summary>
        [JsonPropertyName("action")]
        public string Action { get; set; } = null!;
    }
}
