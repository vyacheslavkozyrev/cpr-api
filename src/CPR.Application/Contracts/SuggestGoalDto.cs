using System;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Request body for POST /api/employees/{id}/goals (manager suggests a goal).
    /// </summary>
    public class SuggestGoalDto
    {
        /// <summary>Goal name (1–200 characters).</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>Optional description (max 2000 characters).</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Optional skill category ID.</summary>
        [JsonPropertyName("skill_category_id")]
        public Guid? SkillCategoryId { get; set; }

        /// <summary>Required timeframe: week | month | quarter | year.</summary>
        [JsonPropertyName("timeframe")]
        public string Timeframe { get; set; } = null!;

        /// <summary>Optional due date (ISO 8601 date; must be today or future).</summary>
        [JsonPropertyName("due_date")]
        public DateTime? DueDate { get; set; }
    }
}
