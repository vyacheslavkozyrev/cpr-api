using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.SkillAssessment
{
    public class SkillAssessmentResponseDto
    {
        [JsonPropertyName("position")]
        public PositionBriefDto? Position { get; set; }

        [JsonPropertyName("next_position")]
        public NextPositionDto? NextPosition { get; set; }

        [JsonPropertyName("skill_categories")]
        public List<SkillCategoryGroupDto> SkillCategories { get; set; } = new();
    }

    public class EmployeeSkillAssessmentResponseDto : SkillAssessmentResponseDto
    {
        [JsonPropertyName("employee")]
        public EmployeeBriefDto Employee { get; set; } = null!;
    }

    public class PositionBriefDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("career_track")]
        public CareerTrackBriefDto? CareerTrack { get; set; }

        [JsonPropertyName("career_path")]
        public CareerPathBriefDto? CareerPath { get; set; }
    }

    public class CareerTrackBriefDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }

    public class CareerPathBriefDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }

    public class NextPositionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }

    public class EmployeeBriefDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = null!;
    }

    public class SkillCategoryGroupDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("skills")]
        public List<SkillItemDto> Skills { get; set; } = new();
    }

    public class SkillItemDto
    {
        [JsonPropertyName("skill_id")]
        public Guid SkillId { get; set; }

        [JsonPropertyName("skill_title")]
        public string SkillTitle { get; set; } = null!;

        [JsonPropertyName("skill_description")]
        public string? SkillDescription { get; set; }

        [JsonPropertyName("required_level")]
        public SkillLevelBriefDto RequiredLevel { get; set; } = null!;

        [JsonPropertyName("next_position_required_level")]
        public SkillLevelBriefDto? NextPositionRequiredLevel { get; set; }

        [JsonPropertyName("assessed")]
        public AssessedLevelDto? Assessed { get; set; }

        [JsonPropertyName("evidence")]
        public List<EvidenceItemDto> Evidence { get; set; } = new();
    }

    public class SkillLevelBriefDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    public class AssessedLevelDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("skill_id")]
        public Guid SkillId { get; set; }

        [JsonPropertyName("self_assessment_value")]
        public decimal SelfAssessmentValue { get; set; }

        [JsonPropertyName("manager_assessment_value")]
        public decimal? ManagerAssessmentValue { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }

    public class EvidenceItemDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("feedback_id")]
        public Guid FeedbackId { get; set; }

        [JsonPropertyName("sender_display_name")]
        public string SenderDisplayName { get; set; } = null!;

        [JsonPropertyName("rating")]
        public int? Rating { get; set; }

        [JsonPropertyName("feedback_content")]
        public string FeedbackContent { get; set; } = null!;
    }

    public class TeamSkillSummaryResponseDto
    {
        [JsonPropertyName("team")]
        public List<TeamMemberSummaryDto> Team { get; set; } = new();
    }

    public class TeamMemberSummaryDto
    {
        [JsonPropertyName("employee_id")]
        public Guid EmployeeId { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = null!;

        [JsonPropertyName("position_title")]
        public string? PositionTitle { get; set; }

        [JsonPropertyName("total_required_skills")]
        public int TotalRequiredSkills { get; set; }

        [JsonPropertyName("assessed_skill_count")]
        public int AssessedSkillCount { get; set; }

        [JsonPropertyName("skills_meeting_requirement_count")]
        public int SkillsMeetingRequirementCount { get; set; }
    }
}
