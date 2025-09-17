using System;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// DTO for creating a new feedback request
    /// </summary>
    public class CreateFeedbackRequestDto
    {
        /// <summary>Employee ID to request feedback from</summary>
        public Guid EmployeeId { get; set; }

        /// <summary>Optional project context for the feedback request</summary>
        public Guid? ProjectId { get; set; }

        /// <summary>Optional goal context for the feedback request</summary>
        public Guid? GoalId { get; set; }

        /// <summary>Optional message explaining the feedback request</summary>
        public string? Message { get; set; }

        /// <summary>Optional due date for the feedback response</summary>
        public DateTimeOffset? DueDate { get; set; }
    }

    /// <summary>
    /// DTO for feedback request details
    /// </summary>
    public class FeedbackRequestDto
    {
        /// <summary>Feedback request identifier</summary>
        public Guid Id { get; set; }

        /// <summary>Employee ID who made the request</summary>
        public Guid RequestorId { get; set; }

        /// <summary>Employee ID who should provide feedback</summary>
        public Guid EmployeeId { get; set; }

        /// <summary>Optional project context</summary>
        public Guid? ProjectId { get; set; }

        /// <summary>Optional goal context</summary>
        public Guid? GoalId { get; set; }

        /// <summary>Optional request message</summary>
        public string? Message { get; set; }

        /// <summary>Optional due date for response</summary>
        public DateTimeOffset? DueDate { get; set; }

        /// <summary>When the request was created</summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>User ID who created the request</summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>Requestor employee details</summary>
        public EmployeeSummaryDto? Requestor { get; set; }

        /// <summary>Target employee details</summary>
        public EmployeeSummaryDto? Employee { get; set; }

        /// <summary>Project details (if provided)</summary>
        public ProjectSummaryDto? Project { get; set; }

        /// <summary>Goal details (if provided)</summary>
        public GoalSummaryDto? Goal { get; set; }
    }

    /// <summary>
    /// Summary DTO for employee information in feedback requests
    /// </summary>
    public class EmployeeSummaryDto
    {
        /// <summary>Employee identifier</summary>
        public Guid Id { get; set; }

        /// <summary>Employee display name</summary>
        public string DisplayName { get; set; } = null!;
    }

    /// <summary>
    /// Summary DTO for project information in feedback requests
    /// </summary>
    public class ProjectSummaryDto
    {
        /// <summary>Project identifier</summary>
        public Guid Id { get; set; }

        /// <summary>Project title</summary>
        public string Title { get; set; } = null!;
    }

    /// <summary>
    /// Summary DTO for goal information in feedback requests
    /// </summary>
    public class GoalSummaryDto
    {
        /// <summary>Goal identifier</summary>
        public Guid Id { get; set; }

        /// <summary>Goal title</summary>
        public string Title { get; set; } = null!;
    }
}