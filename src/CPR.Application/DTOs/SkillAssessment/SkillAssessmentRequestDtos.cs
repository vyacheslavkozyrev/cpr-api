using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.SkillAssessment
{
    public class UpsertSkillAssessmentDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "self_assessment_value must be greater than 0")]
        [JsonPropertyName("self_assessment_value")]
        public decimal SelfAssessmentValue { get; set; }

        [StringLength(1000)]
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }

    public class UpsertManagerAssessmentDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "manager_assessment_value must be greater than 0")]
        [JsonPropertyName("manager_assessment_value")]
        public decimal ManagerAssessmentValue { get; set; }
    }

    public class LinkEvidenceDto
    {
        [Required]
        [JsonPropertyName("feedback_id")]
        public Guid FeedbackId { get; set; }
    }
}
