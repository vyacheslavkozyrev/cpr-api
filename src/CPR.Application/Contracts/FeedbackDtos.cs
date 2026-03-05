using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

    // ====================================
    // FEEDBACK REQUEST DTOs (Multi-Recipient Support)
    // ====================================

    /// <summary>
    /// DTO for creating a new feedback request with multiple recipients (1-20)
    /// </summary>
    public class CreateFeedbackRequestDto
    {
        /// <summary>List of employee IDs to request feedback from (1-20 recipients)</summary>
        [JsonPropertyName("employee_ids")]
        [Required(ErrorMessage = "At least one employee must be selected")]
        [MinLength(1, ErrorMessage = "At least one employee must be selected")]
        [MaxLength(20, ErrorMessage = "Maximum 20 recipients allowed per request")]
        public List<Guid> EmployeeIds { get; set; } = new();

        /// <summary>Optional project context for the feedback request</summary>
        [JsonPropertyName("project_id")]
        public Guid? ProjectId { get; set; }

        /// <summary>Optional goal context for the feedback request</summary>
        [JsonPropertyName("goal_id")]
        public Guid? GoalId { get; set; }

        /// <summary>Optional message explaining the feedback request (max 500 characters)</summary>
        [JsonPropertyName("message")]
        [StringLength(500, ErrorMessage = "Message must be 500 characters or less")]
        public string? Message { get; set; }

        /// <summary>Optional due date for the feedback response</summary>
        [JsonPropertyName("due_date")]
        public DateTimeOffset? DueDate { get; set; }
    }

    /// <summary>
    /// DTO for feedback request details with multi-recipient support
    /// </summary>
    public class FeedbackRequestDto
    {
        /// <summary>Feedback request identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Employee ID who made the request</summary>
        [JsonPropertyName("requestor_id")]
        public Guid RequestorId { get; set; }

        /// <summary>Optional project context</summary>
        [JsonPropertyName("project_id")]
        public Guid? ProjectId { get; set; }

        /// <summary>Optional goal context</summary>
        [JsonPropertyName("goal_id")]
        public Guid? GoalId { get; set; }

        /// <summary>Optional request message (max 500 characters)</summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>Optional due date for response</summary>
        [JsonPropertyName("due_date")]
        public DateTimeOffset? DueDate { get; set; }

        /// <summary>When the request was created</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>When the request was last updated</summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>User ID who created the request</summary>
        [JsonPropertyName("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>Whether the request has been soft-deleted</summary>
        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }

        /// <summary>List of recipients with individual status tracking</summary>
        [JsonPropertyName("recipients")]
        public List<FeedbackRequestRecipientDto> Recipients { get; set; } = new();

        /// <summary>Requestor employee details</summary>
        [JsonPropertyName("requestor")]
        public EmployeeSummaryDto? Requestor { get; set; }

        /// <summary>Project details (if provided)</summary>
        [JsonPropertyName("project")]
        public ProjectSummaryDto? Project { get; set; }

        /// <summary>Goal details (if provided)</summary>
        [JsonPropertyName("goal")]
        public GoalSummaryDto? Goal { get; set; }

        /// <summary>Request status summary (pending, partial, complete, cancelled)</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";

        /// <summary>Count of recipients who have responded</summary>
        [JsonPropertyName("responded_count")]
        public int RespondedCount { get; set; }

        /// <summary>Total count of recipients</summary>
        [JsonPropertyName("total_recipients")]
        public int TotalRecipients { get; set; }
    }

    /// <summary>
    /// DTO for individual recipient status within a feedback request
    /// </summary>
    public class FeedbackRequestRecipientDto
    {
        /// <summary>Recipient record identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Parent feedback request ID</summary>
        [JsonPropertyName("feedback_request_id")]
        public Guid FeedbackRequestId { get; set; }

        /// <summary>Employee ID of the recipient</summary>
        [JsonPropertyName("employee_id")]
        public Guid EmployeeId { get; set; }

        /// <summary>Whether the recipient has completed their response</summary>
        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }

        /// <summary>When the recipient responded (null if not yet responded)</summary>
        [JsonPropertyName("responded_at")]
        public DateTimeOffset? RespondedAt { get; set; }

        /// <summary>When the last reminder was sent to this recipient (null if never reminded)</summary>
        [JsonPropertyName("last_reminder_at")]
        public DateTimeOffset? LastReminderAt { get; set; }

        /// <summary>When this recipient record was created</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>When this recipient record was last updated</summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>Recipient employee details</summary>
        [JsonPropertyName("employee")]
        public EmployeeSummaryDto? Employee { get; set; }

        /// <summary>Recipient status badge (Pending, Overdue, Responded, Cancelled)</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";

        /// <summary>Feedback ID if recipient has responded (null if not responded)</summary>
        [JsonPropertyName("feedback_id")]
        public Guid? FeedbackId { get; set; }
    }

    /// <summary>
    /// DTO for updating a feedback request (only due_date can be updated)
    /// </summary>
    public class UpdateFeedbackRequestDto
    {
        /// <summary>Updated due date for the feedback response</summary>
        [JsonPropertyName("due_date")]
        public DateTimeOffset? DueDate { get; set; }
    }

    /// <summary>
    /// DTO for feedback request list item (lightweight version for paginated lists)
    /// </summary>
    public class FeedbackRequestListDto
    {
        /// <summary>Feedback request identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Employee ID who made the request</summary>
        [JsonPropertyName("requestor_id")]
        public Guid RequestorId { get; set; }

        /// <summary>Message preview (first 100 characters)</summary>
        [JsonPropertyName("message_preview")]
        public string? MessagePreview { get; set; }

        /// <summary>Optional due date</summary>
        [JsonPropertyName("due_date")]
        public DateTimeOffset? DueDate { get; set; }

        /// <summary>When the request was created</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>Request status summary (pending, partial, complete, cancelled)</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";

        /// <summary>Count of recipients who have responded</summary>
        [JsonPropertyName("responded_count")]
        public int RespondedCount { get; set; }

        /// <summary>Total count of recipients</summary>
        [JsonPropertyName("total_recipients")]
        public int TotalRecipients { get; set; }

        /// <summary>Whether any recipient is overdue</summary>
        [JsonPropertyName("has_overdue")]
        public bool HasOverdue { get; set; }

        /// <summary>Requestor employee details</summary>
        [JsonPropertyName("requestor")]
        public EmployeeSummaryDto? Requestor { get; set; }

        /// <summary>Project details (if provided)</summary>
        [JsonPropertyName("project")]
        public ProjectSummaryDto? Project { get; set; }

        /// <summary>Goal details (if provided)</summary>
        [JsonPropertyName("goal")]
        public GoalSummaryDto? Goal { get; set; }

        /// <summary>First 3 recipients for preview (collapsed view)</summary>
        [JsonPropertyName("recipients_preview")]
        public List<FeedbackRequestRecipientDto> RecipientsPreview { get; set; } = new();
    }

    /// <summary>
    /// DTO for pagination metadata
    /// </summary>
    public class PaginationDto
    {
        /// <summary>Current page number (1-based)</summary>
        [JsonPropertyName("page")]
        public int Page { get; set; }

        /// <summary>Number of items per page</summary>
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        /// <summary>Total number of items across all pages</summary>
        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }

        /// <summary>Total number of pages</summary>
        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        /// <summary>Whether there is a previous page</summary>
        [JsonPropertyName("has_previous")]
        public bool HasPrevious { get; set; }

        /// <summary>Whether there is a next page</summary>
        [JsonPropertyName("has_next")]
        public bool HasNext { get; set; }
    }

    /// <summary>
    /// DTO for paginated feedback request list response
    /// </summary>
    public class PaginatedFeedbackRequestsDto
    {
        /// <summary>List of feedback requests for the current page</summary>
        [JsonPropertyName("data")]
        public List<FeedbackRequestListDto> Data { get; set; } = new();

        /// <summary>Pagination metadata</summary>
        [JsonPropertyName("pagination")]
        public PaginationDto Pagination { get; set; } = new();

        /// <summary>Summary statistics</summary>
        [JsonPropertyName("summary")]
        public FeedbackRequestSummaryDto Summary { get; set; } = new();
    }

    /// <summary>
    /// DTO for feedback request summary statistics
    /// </summary>
    public class FeedbackRequestSummaryDto
    {
        /// <summary>Total count of active (non-deleted) requests</summary>
        [JsonPropertyName("total_active")]
        public int TotalActive { get; set; }

        /// <summary>Count of requests with all recipients pending</summary>
        [JsonPropertyName("pending_count")]
        public int PendingCount { get; set; }

        /// <summary>Count of requests with some recipients responded</summary>
        [JsonPropertyName("partial_count")]
        public int PartialCount { get; set; }

        /// <summary>Count of requests with all recipients responded</summary>
        [JsonPropertyName("complete_count")]
        public int CompleteCount { get; set; }

        /// <summary>Count of requests with any overdue recipient</summary>
        [JsonPropertyName("overdue_count")]
        public int OverdueCount { get; set; }
    }

    // ====================================
    // SHARED SUMMARY DTOs
    // ====================================

    /// <summary>
    /// Summary DTO for employee information
    /// </summary>
    public class EmployeeSummaryDto
    {
        /// <summary>Employee identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Employee display name</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Employee email</summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>Job title</summary>
        [JsonPropertyName("job_title")]
        public string? JobTitle { get; set; }

        /// <summary>Department name</summary>
        [JsonPropertyName("department")]
        public string? Department { get; set; }
    }

    /// <summary>
    /// Summary DTO for project information
    /// </summary>
    public class ProjectSummaryDto
    {
        /// <summary>Project identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Project name</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Project description</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Summary DTO for goal information
    /// </summary>
    public class GoalSummaryDto
    {
        /// <summary>Goal identifier</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>Goal title</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Goal description</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    // ====================================
    // FEEDBACK SUBMISSION DTOs
    // ====================================

    /// <summary>
    /// Request DTO for submitting feedback
    /// </summary>
    [NoSelfFeedback]
    public class SubmitFeedbackRequestDto
    {
        /// <summary>The feedback request this feedback is responding to (optional)</summary>
        [JsonPropertyName("feedback_request_id")]
        public Guid? FeedbackRequestId { get; set; }

        /// <summary>The project this feedback is for</summary>
        [JsonPropertyName("project_id")]
        public Guid? ProjectId { get; set; }

        /// <summary>The goal this feedback is for</summary>
        [Required(ErrorMessage = "Goal ID is required")]
        [JsonPropertyName("goal_id")]
        public Guid GoalId { get; set; }

        /// <summary>The employee receiving the feedback</summary>
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonPropertyName("employee_id")]
        public Guid EmployeeId { get; set; }

        /// <summary>The feedback content</summary>
        [Required(ErrorMessage = "Feedback content is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Feedback content must be between 10 and 2000 characters")]
        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;

        /// <summary>The rating (1-5 scale)</summary>
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        [JsonPropertyName("rating")]
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

    /// <summary>
    /// Query parameters for listing feedback requests
    /// </summary>
    public class FeedbackRequestListQuery
    {
        /// <summary>Current page number (1-based)</summary>
        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        /// <summary>Number of items per page (max: 100)</summary>
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; } = 20;

        /// <summary>Sort field</summary>
        [JsonPropertyName("sort_by")]
        public string SortBy { get; set; } = "created_at";

        /// <summary>Sort order (asc or desc)</summary>
        [JsonPropertyName("sort_order")]
        public string SortOrder { get; set; } = "desc";

        /// <summary>Filter by status (pending, partial, complete, cancelled, overdue)</summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>Search in message content</summary>
        [JsonPropertyName("search")]
        public string? Search { get; set; }
    }
}