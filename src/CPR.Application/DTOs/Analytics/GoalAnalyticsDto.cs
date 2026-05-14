using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.Analytics
{
    /// <summary>
    /// A single monthly bucket in the completion trend series.
    /// </summary>
    public record CompletionTrendEntryDto
    {
        /// <summary>Month label in YYYY-MM format.</summary>
        [JsonPropertyName("period_label")]
        public string PeriodLabel { get; init; } = null!;

        /// <summary>Goals created in this calendar month.</summary>
        [JsonPropertyName("created")]
        public int Created { get; init; }

        /// <summary>Goals completed in this calendar month.</summary>
        [JsonPropertyName("completed")]
        public int Completed { get; init; }
    }

    /// <summary>
    /// Aggregate goal statistics for the requested period.
    /// </summary>
    public record GoalStatsDto
    {
        /// <summary>Total non-deleted goals (not period-bounded).</summary>
        [JsonPropertyName("total_goals")]
        public int TotalGoals { get; init; }

        /// <summary>Goals created within the period.</summary>
        [JsonPropertyName("created_in_period")]
        public int CreatedInPeriod { get; init; }

        /// <summary>Goals completed within the period.</summary>
        [JsonPropertyName("completed_in_period")]
        public int CompletedInPeriod { get; init; }

        /// <summary>Non-deleted goals currently in "open" status.</summary>
        [JsonPropertyName("open_goals")]
        public int OpenGoals { get; init; }

        /// <summary>Non-deleted goals currently in "in_progress" status.</summary>
        [JsonPropertyName("in_progress_goals")]
        public int InProgressGoals { get; init; }

        /// <summary>Goals where deadline has passed and is_completed = false.</summary>
        [JsonPropertyName("overdue_goals")]
        public int OverdueGoals { get; init; }

        /// <summary>Completion rate = completed_in_period / created_in_period; null when created_in_period = 0.</summary>
        [JsonPropertyName("completion_rate")]
        public double? CompletionRate { get; init; }

        /// <summary>Overdue rate = overdue_goals / total_goals; null when total_goals = 0.</summary>
        [JsonPropertyName("overdue_rate")]
        public double? OverdueRate { get; init; }

        /// <summary>Average days from created_at to completed_at for goals completed in period; null when none.</summary>
        [JsonPropertyName("avg_days_to_complete")]
        public double? AvgDaysToComplete { get; init; }
    }

    /// <summary>
    /// Current distribution of all non-deleted goals by status.
    /// </summary>
    public record GoalsByStatusDto
    {
        /// <summary>Goals with status "open".</summary>
        [JsonPropertyName("open")]
        public int Open { get; init; }

        /// <summary>Goals with status "in_progress".</summary>
        [JsonPropertyName("in_progress")]
        public int InProgress { get; init; }

        /// <summary>Goals with status "completed".</summary>
        [JsonPropertyName("completed")]
        public int Completed { get; init; }
    }

    /// <summary>
    /// Full goal analytics response returned by <c>GET /api/me/analytics/goals</c>
    /// and <c>GET /api/employees/{id}/analytics/goals</c>.
    /// </summary>
    public record GoalAnalyticsDto
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

        /// <summary>Aggregate goal statistics.</summary>
        [JsonPropertyName("stats")]
        public GoalStatsDto Stats { get; init; } = null!;

        /// <summary>Current distribution of goals across statuses.</summary>
        [JsonPropertyName("goals_by_status")]
        public GoalsByStatusDto GoalsByStatus { get; init; } = null!;

        /// <summary>Monthly completion trend within the period.</summary>
        [JsonPropertyName("completion_trend")]
        public List<CompletionTrendEntryDto> CompletionTrend { get; init; } = new();
    }
}
