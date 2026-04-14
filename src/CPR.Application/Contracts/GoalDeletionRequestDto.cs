using System;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Response DTO for a goal deletion request record.
    /// </summary>
    public class GoalDeletionRequestDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("goal_id")]
        public Guid GoalId { get; set; }

        /// <summary>pending | approved | rejected</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
