using System;
using System.Collections.Generic;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Dashboard summary response DTO
    /// </summary>
    public class DashboardSummaryDto
    {
        public GoalsSummary Goals { get; set; } = new();
        public FeedbackSummary Feedback { get; set; } = new();
        public SkillsSummary Skills { get; set; } = new();
        public ActivitySummary Activity { get; set; } = new();
    }

    public class GoalsSummary
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class FeedbackSummary
    {
        public int TotalReceived { get; set; }
        public int PendingRequests { get; set; }
        public decimal AverageRating { get; set; }
        public int RecentCount { get; set; }
    }

    public class SkillsSummary
    {
        public int TotalSkills { get; set; }
        public int AssessedSkills { get; set; }
        public decimal AssessmentProgress { get; set; }
        public decimal AverageLevel { get; set; }
    }

    public class ActivitySummary
    {
        public int TotalActivities { get; set; }
        public int RecentActivities { get; set; }
    }

    /// <summary>
    /// Activity feed response DTO
    /// </summary>
    public class ActivityFeedDto
    {
        public List<ActivityItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
    }

    public class ActivityItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Timestamp { get; set; }
        public ActivityMetadata Metadata { get; set; } = new();
    }

    public class ActivityMetadata
    {
        public Guid? GoalId { get; set; }
        public Guid? FeedbackId { get; set; }
        public Guid? SkillId { get; set; }
        public Guid? FromUserId { get; set; }
        public int? Rating { get; set; }
    }

    /// <summary>
    /// Goals summary response DTO
    /// </summary>
    public class GoalsSummaryDto
    {
        public GoalsStatistics Statistics { get; set; } = new();
        public List<RecentGoalDto> RecentGoals { get; set; } = new();
        public List<ProgressTrendDto> ProgressTrend { get; set; } = new();
    }

    public class GoalsStatistics
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageProgress { get; set; }
    }

    public class RecentGoalDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Progress { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class ProgressTrendDto
    {
        public string Period { get; set; } = null!;
        public int Completed { get; set; }
        public int Created { get; set; }
    }

    /// <summary>
    /// Dashboard feedback summary response DTO
    /// </summary>
    public class DashboardFeedbackSummaryDto
    {
        public FeedbackStatistics Statistics { get; set; } = new();
        public List<RecentFeedbackDto> RecentFeedback { get; set; } = new();
        public List<RatingTrendDto> RatingTrend { get; set; } = new();
    }

    public class FeedbackStatistics
    {
        public int TotalReceived { get; set; }
        public int PendingRequests { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }

    public class RecentFeedbackDto
    {
        public Guid Id { get; set; }
        public Guid FromEmployeeId { get; set; }
        public string FromEmployeeName { get; set; } = null!;
        public string GoalTitle { get; set; } = null!;
        public int? Rating { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class RatingTrendDto
    {
        public string Period { get; set; } = null!;
        public decimal AverageRating { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Skills summary response DTO
    /// </summary>
    public class SkillsSummaryDto
    {
        public SkillsStatistics Statistics { get; set; } = new();
        public List<SkillCategorySummaryDto> SkillCategories { get; set; } = new();
        public List<RecentAssessmentDto> RecentAssessments { get; set; } = new();
    }

    public class SkillsStatistics
    {
        public int TotalSkills { get; set; }
        public int AssessedSkills { get; set; }
        public decimal AssessmentProgress { get; set; }
        public decimal AverageLevel { get; set; }
        public int SkillGaps { get; set; }
    }

    public class SkillCategorySummaryDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int TotalSkills { get; set; }
        public int AssessedSkills { get; set; }
        public decimal AverageLevel { get; set; }
    }

    public class RecentAssessmentDto
    {
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = null!;
        public int Level { get; set; }
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