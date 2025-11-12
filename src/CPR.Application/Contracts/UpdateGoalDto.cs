using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    public class UpdateGoalDto
    {
        [StringLength(250, MinimumLength = 1)]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [StringLength(2000)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("deadline")]
        public DateTimeOffset? Deadline { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("related_skill_id")]
        public Guid? RelatedSkillId { get; set; }

        [JsonPropertyName("related_skill_level_id")]
        public Guid? RelatedSkillLevelId { get; set; }

        [Range(0, 100, ErrorMessage = "Priority must be between 0 and 100")]
        [JsonPropertyName("priority")]
        public short? Priority { get; set; }

        [StringLength(50)]
        [RegularExpression("^(private|team|org)$", ErrorMessage = "Visibility must be one of: private, team, org")]
        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }
}
