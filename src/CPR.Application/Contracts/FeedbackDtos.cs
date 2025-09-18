using System;
using System.ComponentModel.DataAnnotations;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Custom validation attribute to prevent self-feedback
    /// </summary>
    public class NoSelfFeedbackAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Note: Self-feedback validation is now handled in the controller/service layer
            // since FromEmployeeId is no longer part of the DTO (it's derived from authentication)
            return ValidationResult.Success;
        }
    }
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

    /// <summary>
    /// Request DTO for submitting feedback
    /// </summary>
    [NoSelfFeedback]
    public class SubmitFeedbackRequestDto
    {
        /// <summary>The project this feedback is for</summary>
        public Guid? ProjectId { get; set; }

        /// <summary>The goal this feedback is for</summary>
        [Required(ErrorMessage = "Goal ID is required")]
        public Guid GoalId { get; set; }

        /// <summary>The employee receiving the feedback</summary>
        [Required(ErrorMessage = "Employee ID is required")]
        public Guid EmployeeId { get; set; }

        /// <summary>The feedback content</summary>
        [Required(ErrorMessage = "Feedback content is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Feedback content must be between 10 and 2000 characters")]
        public string Content { get; set; } = null!;

        /// <summary>The rating (1-5 scale)</summary>
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
    }

    /// <summary>
    /// DTO for feedback information
    /// </summary>
    public class FeedbackDto
    {
        /// <summary>Feedback identifier</summary>
        public Guid Id { get; set; }

        /// <summary>The project this feedback is for</summary>
        public Guid? ProjectId { get; set; }

        /// <summary>The goal this feedback is for</summary>
        public Guid GoalId { get; set; }

        /// <summary>The employee providing the feedback</summary>
        public Guid FromEmployeeId { get; set; }

        /// <summary>The employee receiving the feedback</summary>
        public Guid ToEmployeeId { get; set; }

        /// <summary>The feedback content</summary>
        public string Content { get; set; } = null!;

        /// <summary>The rating (1-5 scale)</summary>
        public int Rating { get; set; }

        /// <summary>When the feedback was created</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Summary of the project</summary>
        public ProjectSummaryDto? Project { get; set; }

        /// <summary>Summary of the goal</summary>
        public GoalSummaryDto? Goal { get; set; }

        /// <summary>Summary of the employee providing feedback</summary>
        public EmployeeSummaryDto? FromEmployee { get; set; }

        /// <summary>Summary of the employee receiving feedback</summary>
        public EmployeeSummaryDto? ToEmployee { get; set; }
    }

    /// <summary>
    /// DTO for feedback information when viewing feedback addressed to current user
    /// Excludes ToEmployee fields since they would always be the current user
    /// </summary>
    public class MyFeedbackDto
    {
        /// <summary>Feedback identifier</summary>
        public Guid Id { get; set; }

        /// <summary>The project this feedback is for</summary>
        public Guid? ProjectId { get; set; }

        /// <summary>The goal this feedback is for</summary>
        public Guid GoalId { get; set; }

        /// <summary>The employee providing the feedback</summary>
        public Guid FromEmployeeId { get; set; }

        /// <summary>The feedback content</summary>
        public string Content { get; set; } = null!;

        /// <summary>The rating (1-5 scale)</summary>
        public int Rating { get; set; }

        /// <summary>When the feedback was created</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Summary of the project</summary>
        public ProjectSummaryDto? Project { get; set; }

        /// <summary>Summary of the goal</summary>
        public GoalSummaryDto? Goal { get; set; }

        /// <summary>Summary of the employee providing feedback</summary>
        public EmployeeSummaryDto? FromEmployee { get; set; }
    }

    /// <summary>
    /// Summary DTO for feedback information
    /// </summary>
    public class FeedbackSummaryDto
    {
        /// <summary>Feedback identifier</summary>
        public Guid Id { get; set; }

        /// <summary>The goal this feedback is for</summary>
        public Guid GoalId { get; set; }

        /// <summary>The employee providing the feedback</summary>
        public Guid FromEmployeeId { get; set; }

        /// <summary>The employee receiving the feedback</summary>
        public Guid ToEmployeeId { get; set; }

        /// <summary>The feedback content (truncated)</summary>
        public string Content { get; set; } = null!;

        /// <summary>The rating (1-5 scale)</summary>
        public int Rating { get; set; }

        /// <summary>When the feedback was created</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Goal title</summary>
        public string GoalTitle { get; set; } = null!;

        /// <summary>Name of the employee providing feedback</summary>
        public string FromEmployeeName { get; set; } = null!;

        /// <summary>Name of the employee receiving feedback</summary>
        public string ToEmployeeName { get; set; } = null!;
    }
}