using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Dashboard summary response DTO
    /// </summary>
    public class DashboardSummaryDto
    {
        [JsonPropertyName("goals")]
        public GoalsSummary Goals { get; set; } = new();
        [JsonPropertyName("feedback")]
        public FeedbackSummary Feedback { get; set; } = new();
        [JsonPropertyName("skills")]
        public SkillsSummary Skills { get; set; } = new();
        [JsonPropertyName("activity")]
        public ActivitySummary Activity { get; set; } = new();
    }

    public class GoalsSummary
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("active")]
        public int Active { get; set; }
        [JsonPropertyName("completed")]
        public int Completed { get; set; }
        [JsonPropertyName("overdue")]
        public int Overdue { get; set; }
        [JsonPropertyName("completion_rate")]
        public decimal CompletionRate { get; set; }
    }

    public class FeedbackSummary
    {
        [JsonPropertyName("total_received")]
        public int TotalReceived { get; set; }
        [JsonPropertyName("pending_requests")]
        public int PendingRequests { get; set; }
        [JsonPropertyName("average_rating")]
        public decimal AverageRating { get; set; }
        [JsonPropertyName("recent_count")]
        public int RecentCount { get; set; }
    }

    public class SkillsSummary
    {
        [JsonPropertyName("total_skills")]
        public int TotalSkills { get; set; }
        [JsonPropertyName("assessed_skills")]
        public int AssessedSkills { get; set; }
        [JsonPropertyName("assessment_progress")]
        public decimal AssessmentProgress { get; set; }
        [JsonPropertyName("average_level")]
        public decimal AverageLevel { get; set; }
    }

    public class ActivitySummary
    {
        [JsonPropertyName("total_activities")]
        public int TotalActivities { get; set; }
        [JsonPropertyName("recent_activities")]
        public int RecentActivities { get; set; }
    }

    /// <summary>
    /// Activity feed response DTO
    /// </summary>
    public class ActivityFeedDto
    {
        [JsonPropertyName("items")]
        public List<ActivityItemDto> Items { get; set; } = new();
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }
    }

    public class ActivityItemDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
        [JsonPropertyName("metadata")]
        public ActivityMetadata Metadata { get; set; } = new();
    }

    public class ActivityMetadata
    {
        [JsonPropertyName("goal_id")]
        public Guid? GoalId { get; set; }
        [JsonPropertyName("feedback_id")]
        public Guid? FeedbackId { get; set; }
        [JsonPropertyName("skill_id")]
        public Guid? SkillId { get; set; }
        [JsonPropertyName("from_user_id")]
        public Guid? FromUserId { get; set; }
        [JsonPropertyName("rating")]
        public int? Rating { get; set; }
    }

    /// <summary>
    /// Goals summary response DTO
    /// </summary>
    public class GoalsSummaryDto
    {
        [JsonPropertyName("statistics")]
        public GoalsStatistics Statistics { get; set; } = new();
        [JsonPropertyName("recent_goals")]
        public List<RecentGoalDto> RecentGoals { get; set; } = new();
        [JsonPropertyName("progress_trend")]
        public List<ProgressTrendDto> ProgressTrend { get; set; } = new();
    }

    public class GoalsStatistics
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("active")]
        public int Active { get; set; }
        [JsonPropertyName("completed")]
        public int Completed { get; set; }
        [JsonPropertyName("overdue")]
        public int Overdue { get; set; }
        [JsonPropertyName("completion_rate")]
        public decimal CompletionRate { get; set; }
        [JsonPropertyName("average_progress")]
        public decimal AverageProgress { get; set; }
    }

    public class RecentGoalDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
        [JsonPropertyName("progress")]
        public decimal Progress { get; set; }
        [JsonPropertyName("deadline")]
        public DateTime? Deadline { get; set; }
        [JsonPropertyName("is_overdue")]
        public bool IsOverdue { get; set; }
    }

    public class ProgressTrendDto
    {
        [JsonPropertyName("period")]
        public string Period { get; set; } = null!;
        [JsonPropertyName("completed")]
        public int Completed { get; set; }
        [JsonPropertyName("created")]
        public int Created { get; set; }
    }

    /// <summary>
    /// Dashboard feedback summary response DTO
    /// </summary>
    public class DashboardFeedbackSummaryDto
    {
        [JsonPropertyName("statistics")]
        public FeedbackStatistics Statistics { get; set; } = new();
        [JsonPropertyName("recent_feedback")]
        public List<RecentFeedbackDto> RecentFeedback { get; set; } = new();
        [JsonPropertyName("rating_trend")]
        public List<RatingTrendDto> RatingTrend { get; set; } = new();
    }

    public class FeedbackStatistics
    {
        [JsonPropertyName("total_received")]
        public int TotalReceived { get; set; }
        [JsonPropertyName("pending_requests")]
        public int PendingRequests { get; set; }
        [JsonPropertyName("average_rating")]
        public decimal AverageRating { get; set; }
        [JsonPropertyName("rating_distribution")]
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }

    public class RecentFeedbackDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("from_employee_id")]
        public Guid FromEmployeeId { get; set; }
        [JsonPropertyName("from_employee_name")]
        public string FromEmployeeName { get; set; } = null!;
        [JsonPropertyName("goal_title")]
        public string GoalTitle { get; set; } = null!;
        [JsonPropertyName("rating")]
        public int? Rating { get; set; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class RatingTrendDto
    {
        [JsonPropertyName("period")]
        public string Period { get; set; } = null!;
        [JsonPropertyName("average_rating")]
        public decimal AverageRating { get; set; }
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// Skills summary response DTO
    /// </summary>
    public class SkillsSummaryDto
    {
        [JsonPropertyName("statistics")]
        public SkillsStatistics Statistics { get; set; } = new();
        [JsonPropertyName("skill_categories")]
        public List<SkillCategorySummaryDto> SkillCategories { get; set; } = new();
        [JsonPropertyName("recent_assessments")]
        public List<RecentAssessmentDto> RecentAssessments { get; set; } = new();
    }

    public class SkillsStatistics
    {
        [JsonPropertyName("total_skills")]
        public int TotalSkills { get; set; }
        [JsonPropertyName("assessed_skills")]
        public int AssessedSkills { get; set; }
        [JsonPropertyName("assessment_progress")]
        public decimal AssessmentProgress { get; set; }
        [JsonPropertyName("average_level")]
        public decimal AverageLevel { get; set; }
        [JsonPropertyName("skill_gaps")]
        public int SkillGaps { get; set; }
    }

    public class SkillCategorySummaryDto
    {
        [JsonPropertyName("category_id")]
        public Guid CategoryId { get; set; }
        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = null!;
        [JsonPropertyName("total_skills")]
        public int TotalSkills { get; set; }
        [JsonPropertyName("assessed_skills")]
        public int AssessedSkills { get; set; }
        [JsonPropertyName("average_level")]
        public decimal AverageLevel { get; set; }
    }

    public class RecentAssessmentDto
    {
        [JsonPropertyName("skill_id")]
        public Guid SkillId { get; set; }
        [JsonPropertyName("skill_name")]
        public string SkillName { get; set; } = null!;
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("assessed_at")]
        public DateTimeOffset AssessedAt { get; set; }
    }

    /// <summary>
    /// Period enumeration for filtering
    /// </summary>
    public enum DashboardPeriod
    {
        Week,
        Month,
        Quarter,
        Year
    }
}