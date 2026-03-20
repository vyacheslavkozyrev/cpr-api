using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.GapAnalysis
{
    /// <summary>Career track summary within a gap analysis response.</summary>
    public class GapCareerTrackDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }

    /// <summary>Current position summary within a gap analysis response.</summary>
    public class GapCurrentPositionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }

        [JsonPropertyName("career_track")]
        public GapCareerTrackDto CareerTrack { get; set; } = null!;
    }

    /// <summary>Next-level position summary within a gap analysis response.</summary>
    public class GapNextPositionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }
    }

    /// <summary>Skill category embedded in a skill gap entry.</summary>
    public class GapSkillCategoryDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }

    /// <summary>Skill embedded in a skill gap entry.</summary>
    public class GapSkillDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("category")]
        public GapSkillCategoryDto Category { get; set; } = null!;
    }

    /// <summary>Skill level embedded in a skill gap entry.</summary>
    public class GapSkillLevelDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    /// <summary>A linked non-completed goal targeting the same skill.</summary>
    public class LinkedGoalDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("progress_percentage")]
        public decimal ProgressPercentage { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    /// <summary>A single skill gap entry comparing required vs actual level.</summary>
    public class SkillGapDto
    {
        [JsonPropertyName("skill")]
        public GapSkillDto Skill { get; set; } = null!;

        [JsonPropertyName("required_level")]
        public GapSkillLevelDto RequiredLevel { get; set; } = null!;

        [JsonPropertyName("actual_level")]
        public GapSkillLevelDto ActualLevel { get; set; } = null!;

        [JsonPropertyName("gap")]
        public int Gap { get; set; }

        [JsonPropertyName("is_mandatory")]
        public bool IsMandatory { get; set; }

        /// <summary>"manager" when manager_assessment_value IS NOT NULL; "default" otherwise.</summary>
        [JsonPropertyName("assessment_source")]
        public string AssessmentSource { get; set; } = null!;

        [JsonPropertyName("linked_goals")]
        public List<LinkedGoalDto> LinkedGoals { get; set; } = new();
    }

    /// <summary>Aggregate counts for the gap analysis result.</summary>
    public class GapSummaryDto
    {
        [JsonPropertyName("total_skills")]
        public int TotalSkills { get; set; }

        [JsonPropertyName("skills_met")]
        public int SkillsMet { get; set; }

        [JsonPropertyName("skills_with_gap")]
        public int SkillsWithGap { get; set; }

        [JsonPropertyName("mandatory_gaps")]
        public int MandatoryGaps { get; set; }
    }

    /// <summary>Full gap analysis response returned by both endpoints.</summary>
    public class GapAnalysisDto
    {
        [JsonPropertyName("current_position")]
        public GapCurrentPositionDto CurrentPosition { get; set; } = null!;

        /// <summary>Null when the employee is already at the highest sort_order in their track.</summary>
        [JsonPropertyName("next_position")]
        public GapNextPositionDto? NextPosition { get; set; }

        [JsonPropertyName("skill_gaps")]
        public List<SkillGapDto> SkillGaps { get; set; } = new();

        [JsonPropertyName("summary")]
        public GapSummaryDto Summary { get; set; } = null!;
    }
}
