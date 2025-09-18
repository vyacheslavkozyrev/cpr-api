using System;
using System.Collections.Generic;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Basic team member information for team listing
    /// </summary>
    public class TeamMemberDto
    {
        /// <summary>Employee identifier</summary>
        public Guid Id { get; set; }

        /// <summary>Employee's display name</summary>
        public string DisplayName { get; set; } = null!;

        /// <summary>Employee's job title</summary>
        public string? Title { get; set; }

        /// <summary>Employee's department</summary>
        public string? Department { get; set; }

        /// <summary>Number of active goals</summary>
        public int ActiveGoalsCount { get; set; }

        /// <summary>Latest feedback rating (if any)</summary>
        public int? LatestFeedbackRating { get; set; }

        /// <summary>Date of last feedback received</summary>
        public DateTimeOffset? LastFeedbackDate { get; set; }
    }

    /// <summary>
    /// Comprehensive team member profile with all relevant information
    /// </summary>
    public class TeamMemberProfileDto
    {
        /// <summary>Employee identifier</summary>
        public Guid Id { get; set; }

        /// <summary>Basic employee information</summary>
        public EmployeeInfoDto EmployeeInfo { get; set; } = null!;

        /// <summary>Employee's skill assessments</summary>
        public List<EmployeeSkillDto> Skills { get; set; } = new();

        /// <summary>Employee's goals</summary>
        public List<TeamGoalSummaryDto> Goals { get; set; } = new();

        /// <summary>Recent feedback received by the employee</summary>
        public List<TeamFeedbackSummaryDto> RecentFeedback { get; set; } = new();

        /// <summary>Projects the employee is involved in</summary>
        public List<ProjectRoleDto> Projects { get; set; } = new();
    }

    /// <summary>
    /// Basic employee information
    /// </summary>
    public class EmployeeInfoDto
    {
        /// <summary>Employee identifier</summary>
        public Guid Id { get; set; }

        /// <summary>User identifier</summary>
        public Guid UserId { get; set; }

        /// <summary>Employee's display name</summary>
        public string DisplayName { get; set; } = null!;

        /// <summary>Employee's job title</summary>
        public string? Title { get; set; }

        /// <summary>Employee's department</summary>
        public string? Department { get; set; }

        /// <summary>Manager's identifier</summary>
        public Guid? ManagerId { get; set; }

        /// <summary>Manager's display name</summary>
        public string? ManagerName { get; set; }
    }

    /// <summary>
    /// Summary of a goal for team member profile
    /// </summary>
    public class TeamGoalSummaryDto
    {
        /// <summary>Goal identifier</summary>
        public Guid GoalId { get; set; }

        /// <summary>Goal title</summary>
        public string Title { get; set; } = null!;

        /// <summary>Goal description</summary>
        public string? Description { get; set; }

        /// <summary>Goal status</summary>
        public string Status { get; set; } = null!;

        /// <summary>Goal deadline</summary>
        public DateTimeOffset? Deadline { get; set; }

        /// <summary>Goal priority</summary>
        public string Priority { get; set; } = null!;

        /// <summary>Related skill name</summary>
        public string? RelatedSkillName { get; set; }
    }

    /// <summary>
    /// Summary of feedback for team member profile
    /// </summary>
    public class TeamFeedbackSummaryDto
    {
        /// <summary>Feedback identifier</summary>
        public Guid FeedbackId { get; set; }

        /// <summary>Feedback giver's name</summary>
        public string FromEmployeeName { get; set; } = null!;

        /// <summary>Feedback content</summary>
        public string Content { get; set; } = null!;

        /// <summary>Feedback type</summary>
        public string FeedbackType { get; set; } = null!;

        /// <summary>Related skill name</summary>
        public string? RelatedSkillName { get; set; }

        /// <summary>Date feedback was given</summary>
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// Project role information for team member profile
    /// </summary>
    public class ProjectRoleDto
    {
        /// <summary>Project identifier</summary>
        public Guid ProjectId { get; set; }

        /// <summary>Project title</summary>
        public string ProjectTitle { get; set; } = null!;

        /// <summary>Project role identifier</summary>
        public Guid ProjectRoleId { get; set; }

        /// <summary>Project role title</summary>
        public string ProjectRoleTitle { get; set; } = null!;

        /// <summary>Role description</summary>
        public string? ProjectRoleDescription { get; set; }

        /// <summary>Date joined the project</summary>
        public DateTimeOffset? JoinedAt { get; set; }
    }

    /// <summary>
    /// Team goals aggregation with filtering options
    /// </summary>
    public class TeamGoalsDto
    {
        /// <summary>Total number of goals across the team</summary>
        public int TotalGoals { get; set; }

        /// <summary>Number of active (non-completed) goals</summary>
        public int ActiveGoals { get; set; }

        /// <summary>Number of completed goals</summary>
        public int CompletedGoals { get; set; }

        /// <summary>Number of overdue goals</summary>
        public int OverdueGoals { get; set; }

        /// <summary>Average progress percentage across active goals</summary>
        public decimal AverageProgress { get; set; }

        /// <summary>Goals grouped by status</summary>
        public Dictionary<string, int> GoalsByStatus { get; set; } = new();

        /// <summary>Individual team member goal summaries</summary>
        public List<TeamMemberGoalsDto> MemberGoals { get; set; } = new();
    }

    /// <summary>
    /// Individual team member's goals summary
    /// </summary>
    public class TeamMemberGoalsDto
    {
        /// <summary>Employee identifier</summary>
        public Guid EmployeeId { get; set; }

        /// <summary>Employee display name</summary>
        public string EmployeeName { get; set; } = null!;

        /// <summary>Total goals for this employee</summary>
        public int TotalGoals { get; set; }

        /// <summary>Active goals for this employee</summary>
        public int ActiveGoals { get; set; }

        /// <summary>Completed goals for this employee</summary>
        public int CompletedGoals { get; set; }

        /// <summary>Overdue goals for this employee</summary>
        public int OverdueGoals { get; set; }

        /// <summary>Average progress across employee's active goals</summary>
        public decimal AverageProgress { get; set; }
    }
}