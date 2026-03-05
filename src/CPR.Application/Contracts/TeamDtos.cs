using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Basic team member information for team listing
    /// </summary>
    public class TeamMemberDto
    {
        /// <summary>Employee identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Employee's display name</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = null!;

        /// <summary>Employee's job title</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>Employee's department</summary>
        [JsonPropertyName("department")]
        public string? Department { get; set; }

        /// <summary>Number of active goals</summary>
        [JsonPropertyName("active_goals_count")]
        public int ActiveGoalsCount { get; set; }

        /// <summary>Latest feedback rating (if any)</summary>
        [JsonPropertyName("latest_feedback_rating")]
        public int? LatestFeedbackRating { get; set; }

        /// <summary>Date of last feedback received</summary>
        [JsonPropertyName("last_feedback_date")]
        public DateTimeOffset? LastFeedbackDate { get; set; }
    }

    /// <summary>
    /// Comprehensive team member profile with all relevant information
    /// </summary>
    public class TeamMemberProfileDto
    {
        /// <summary>Employee identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Basic employee information</summary>
        [JsonPropertyName("employee_info")]
        public EmployeeInfoDto EmployeeInfo { get; set; } = null!;

        /// <summary>Employee's skill assessments</summary>
        [JsonPropertyName("skills")]
        public List<EmployeeSkillDto> Skills { get; set; } = new();

        /// <summary>Employee's goals</summary>
        [JsonPropertyName("goals")]
        public List<TeamGoalSummaryDto> Goals { get; set; } = new();

        /// <summary>Recent feedback received by the employee</summary>
        [JsonPropertyName("recent_feedback")]
        public List<TeamFeedbackSummaryDto> RecentFeedback { get; set; } = new();

        /// <summary>Projects the employee is involved in</summary>
        [JsonPropertyName("projects")]
        public List<TeamMemberProjectDto> Projects { get; set; } = new();
    }

    /// <summary>
    /// Basic employee information
    /// </summary>
    public class EmployeeInfoDto
    {
        /// <summary>Employee identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>User identifier</summary>
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        /// <summary>Employee's display name</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = null!;

        /// <summary>Employee's job title</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>Employee's department</summary>
        [JsonPropertyName("department")]
        public string? Department { get; set; }

        /// <summary>Manager's identifier</summary>
        [JsonPropertyName("manager_id")]
        public Guid? ManagerId { get; set; }

        /// <summary>Manager's display name</summary>
        [JsonPropertyName("manager_name")]
        public string? ManagerName { get; set; }
    }

    /// <summary>
    /// Summary of a goal for team member profile
    /// </summary>
    public class TeamGoalSummaryDto
    {
        /// <summary>Goal identifier</summary>
        [JsonPropertyName("goal_id")]
        public Guid GoalId { get; set; }

        /// <summary>Goal title</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        /// <summary>Goal description</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Goal status</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        /// <summary>Goal deadline</summary>
        [JsonPropertyName("deadline")]
        public DateTimeOffset? Deadline { get; set; }

        /// <summary>Goal priority</summary>
        [JsonPropertyName("priority")]
        public string Priority { get; set; } = null!;

        /// <summary>Related skill name</summary>
        [JsonPropertyName("related_skill_name")]
        public string? RelatedSkillName { get; set; }
    }

    /// <summary>
    /// Summary of feedback for team member profile
    /// </summary>
    public class TeamFeedbackSummaryDto
    {
        /// <summary>Feedback identifier</summary>
        [JsonPropertyName("feedback_id")]
        public Guid FeedbackId { get; set; }

        /// <summary>Feedback giver's name</summary>
        [JsonPropertyName("from_employee_name")]
        public string FromEmployeeName { get; set; } = null!;

        /// <summary>Feedback content</summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;

        /// <summary>Feedback type</summary>
        [JsonPropertyName("feedback_type")]
        public string FeedbackType { get; set; } = null!;

        /// <summary>Related skill name</summary>
        [JsonPropertyName("related_skill_name")]
        public string? RelatedSkillName { get; set; }

        /// <summary>Date feedback was given</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// Project assignment information for team member profile
    /// </summary>
    public class TeamMemberProjectDto
    {
        /// <summary>Project identifier</summary>
        [JsonPropertyName("project_id")]
        public Guid ProjectId { get; set; }

        /// <summary>Project title</summary>
        [JsonPropertyName("project_title")]
        public string ProjectTitle { get; set; } = null!;

        /// <summary>Project role identifier</summary>
        [JsonPropertyName("project_role_id")]
        public Guid ProjectRoleId { get; set; }

        /// <summary>Project role title</summary>
        [JsonPropertyName("project_role_title")]
        public string ProjectRoleTitle { get; set; } = null!;

        /// <summary>Role description</summary>
        [JsonPropertyName("project_role_description")]
        public string? ProjectRoleDescription { get; set; }

        /// <summary>Date joined the project</summary>
        [JsonPropertyName("joined_at")]
        public DateTimeOffset? JoinedAt { get; set; }
    }

    /// <summary>
    /// Team goals aggregation with filtering options
    /// </summary>
    public class TeamGoalsDto
    {
        /// <summary>Total number of goals across the team</summary>
        [JsonPropertyName("total_goals")]
        public int TotalGoals { get; set; }

        /// <summary>Number of active (non-completed) goals</summary>
        [JsonPropertyName("active_goals")]
        public int ActiveGoals { get; set; }

        /// <summary>Number of completed goals</summary>
        [JsonPropertyName("completed_goals")]
        public int CompletedGoals { get; set; }

        /// <summary>Number of overdue goals</summary>
        [JsonPropertyName("overdue_goals")]
        public int OverdueGoals { get; set; }

        /// <summary>Average progress percentage across active goals</summary>
        [JsonPropertyName("average_progress")]
        public decimal AverageProgress { get; set; }

        /// <summary>Goals grouped by status</summary>
        [JsonPropertyName("goals_by_status")]
        public Dictionary<string, int> GoalsByStatus { get; set; } = new();

        /// <summary>Individual team member goal summaries</summary>
        [JsonPropertyName("member_goals")]
        public List<TeamMemberGoalsDto> MemberGoals { get; set; } = new();
    }

    /// <summary>
    /// Individual team member's goals summary
    /// </summary>
    public class TeamMemberGoalsDto
    {
        /// <summary>Employee identifier</summary>
        [JsonPropertyName("employee_id")]
        public Guid EmployeeId { get; set; }

        /// <summary>Employee display name</summary>
        [JsonPropertyName("employee_name")]
        public string EmployeeName { get; set; } = null!;

        /// <summary>Total goals for this employee</summary>
        [JsonPropertyName("total_goals")]
        public int TotalGoals { get; set; }

        /// <summary>Active goals for this employee</summary>
        [JsonPropertyName("active_goals")]
        public int ActiveGoals { get; set; }

        /// <summary>Completed goals for this employee</summary>
        [JsonPropertyName("completed_goals")]
        public int CompletedGoals { get; set; }

        /// <summary>Overdue goals for this employee</summary>
        [JsonPropertyName("overdue_goals")]
        public int OverdueGoals { get; set; }

        /// <summary>Average progress across employee's active goals</summary>
        [JsonPropertyName("average_progress")]
        public decimal AverageProgress { get; set; }
    }
}