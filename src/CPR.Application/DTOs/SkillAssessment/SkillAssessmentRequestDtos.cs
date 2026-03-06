using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.SkillAssessment
{
    public class UpsertSkillAssessmentDto
    {
        [Required]
        [JsonPropertyName("skill_level_id")]
        public Guid SkillLevelId { get; set; }

        [StringLength(1000)]
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }

    public class UpsertSkillTargetDto
    {
        [Required]
        [JsonPropertyName("skill_level_id")]
        public Guid SkillLevelId { get; set; }
    }

    public class LinkEvidenceDto
    {
        [Required]
        [JsonPropertyName("feedback_id")]
        public Guid FeedbackId { get; set; }
    }
}
