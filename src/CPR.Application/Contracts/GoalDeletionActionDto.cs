using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Request body for PATCH /api/goals/{id}/deletion-request (manager approves or rejects).
    /// </summary>
    public class GoalDeletionActionDto
    {
        /// <summary>Action: approve | reject</summary>
        [JsonPropertyName("action")]
        public string Action { get; set; } = null!;
    }
}
