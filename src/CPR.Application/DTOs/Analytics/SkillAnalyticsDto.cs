using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.Analytics
{
    /// <summary>
    /// A single history snapshot for a skill assessment.
    /// </summary>
    public record SkillHistoryEntryDto
    {
        /// <summary>Timestamp when the change was recorded.</summary>
        [JsonPropertyName("recorded_at")]
        public DateTimeOffset RecordedAt { get; init; }

        /// <summary>Self-assessment value at the time of the snapshot.</summary>
        [JsonPropertyName("self_assessment_value")]
        public decimal SelfAssessmentValue { get; init; }

        /// <summary>Manager assessment value at the time of the snapshot; null if not yet assessed.</summary>
        [JsonPropertyName("manager_assessment_value")]
        public decimal? ManagerAssessmentValue { get; init; }
    }

    /// <summary>
    /// A single skill row in the skill analytics response.
    /// </summary>
    public record SkillRowDto
    {
        /// <summary>Skill identifier.</summary>
        [JsonPropertyName("skill_id")]
        public Guid SkillId { get; init; }

        /// <summary>Skill display title.</summary>
        [JsonPropertyName("skill_title")]
        public string SkillTitle { get; init; } = null!;

        /// <summary>Skill category title.</summary>
        [JsonPropertyName("category_title")]
        public string CategoryTitle { get; init; } = null!;

        /// <summary>Current self-assessment value.</summary>
        [JsonPropertyName("current_self_assessment")]
        public decimal CurrentSelfAssessment { get; init; }

        /// <summary>Current manager assessment value; null if not yet assessed by manager.</summary>
        [JsonPropertyName("current_manager_assessment")]
        public decimal? CurrentManagerAssessment { get; init; }

        /// <summary>Required level value for the employee's current position; null if no requirement.</summary>
        [JsonPropertyName("required_level")]
        public decimal? RequiredLevel { get; init; }

        /// <summary>Gap = required_level - current_self_assessment; null when required_level is null; 0 when met.</summary>
        [JsonPropertyName("gap")]
        public decimal? Gap { get; init; }

        /// <summary>History entries within the requested period, ordered by recorded_at ascending.</summary>
        [JsonPropertyName("history")]
        public List<SkillHistoryEntryDto> History { get; init; } = new();
    }

    /// <summary>
    /// Summary of gap closure activity during the period.
    /// </summary>
    public record GapClosureSummaryDto
    {
        /// <summary>Total skills the employee has assessed (employee_to_skill rows).</summary>
        [JsonPropertyName("skills_assessed")]
        public int SkillsAssessed { get; init; }

        /// <summary>Skills where gap > 0 at period end.</summary>
        [JsonPropertyName("skills_with_gaps")]
        public int SkillsWithGaps { get; init; }

        /// <summary>Skills where gap was > 0 at period start and is 0 or negative now.</summary>
        [JsonPropertyName("gaps_closed_in_period")]
        public int GapsClosedInPeriod { get; init; }

        /// <summary>Skills where gap worsened during the period.</summary>
        [JsonPropertyName("gaps_worsened_in_period")]
        public int GapsWorsenedInPeriod { get; init; }

        /// <summary>Average gap across skills with a required level, at period start; null if no skills with requirements.</summary>
        [JsonPropertyName("avg_gap_at_period_start")]
        public double? AvgGapAtPeriodStart { get; init; }

        /// <summary>Average gap across skills with a required level, at period end (current); null if no skills with requirements.</summary>
        [JsonPropertyName("avg_gap_at_period_end")]
        public double? AvgGapAtPeriodEnd { get; init; }
    }

    /// <summary>
    /// Full skill analytics response returned by <c>GET /api/me/analytics/skills</c>
    /// and <c>GET /api/employees/{id}/analytics/skills</c>.
    /// </summary>
    public record SkillAnalyticsDto
    {
        /// <summary>Requested period identifier string.</summary>
        [JsonPropertyName("period")]
        public string Period { get; init; } = null!;

        /// <summary>Inclusive start of the resolved period window (UTC).</summary>
        [JsonPropertyName("period_start")]
        public DateTimeOffset PeriodStart { get; init; }

        /// <summary>Inclusive end of the resolved period window (UTC).</summary>
        [JsonPropertyName("period_end")]
        public DateTimeOffset PeriodEnd { get; init; }

        /// <summary>Gap closure summary for the period.</summary>
        [JsonPropertyName("gap_closure_summary")]
        public GapClosureSummaryDto GapClosureSummary { get; init; } = null!;

        /// <summary>List of skill rows with history data.</summary>
        [JsonPropertyName("skills")]
        public List<SkillRowDto> Skills { get; init; } = new();
    }
}
