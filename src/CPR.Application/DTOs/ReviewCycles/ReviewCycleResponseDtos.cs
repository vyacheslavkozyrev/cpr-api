using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.ReviewCycles
{
    public class ReviewCycleDetailDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("subject_employee_id")]
        public Guid SubjectEmployeeId { get; set; }

        [JsonPropertyName("subject_display_name")]
        public string SubjectDisplayName { get; set; } = null!;

        [JsonPropertyName("department_id")]
        public Guid DepartmentId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("nominee_count")]
        public int NomineeCount { get; set; }

        [JsonPropertyName("response_count")]
        public int ResponseCount { get; set; }

        [JsonPropertyName("opened_at")]
        public DateTimeOffset? OpenedAt { get; set; }

        [JsonPropertyName("started_at")]
        public DateTimeOffset? StartedAt { get; set; }

        [JsonPropertyName("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("created_by")]
        public Guid? CreatedBy { get; set; }
    }

    public class ReviewCycleSummaryDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("subject_display_name")]
        public string SubjectDisplayName { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("nominee_count")]
        public int NomineeCount { get; set; }

        [JsonPropertyName("response_count")]
        public int ResponseCount { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }
    }

    public class ReviewCycleStatusTransitionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("opened_at")]
        public DateTimeOffset? OpenedAt { get; set; }

        [JsonPropertyName("started_at")]
        public DateTimeOffset? StartedAt { get; set; }

        [JsonPropertyName("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }
    }

    public class ReviewNomineeDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("cycle_id")]
        public Guid CycleId { get; set; }

        [JsonPropertyName("reviewer_employee_id")]
        public Guid ReviewerEmployeeId { get; set; }

        [JsonPropertyName("reviewer_display_name")]
        public string ReviewerDisplayName { get; set; } = null!;

        [JsonPropertyName("nominated_by")]
        public Guid NominatedBy { get; set; }

        [JsonPropertyName("nominated_by_display_name")]
        public string NominatedByDisplayName { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class ReviewResponseDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("cycle_id")]
        public Guid CycleId { get; set; }

        [JsonPropertyName("nominee_id")]
        public Guid NomineeId { get; set; }

        [JsonPropertyName("overall_rating")]
        public int OverallRating { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; } = null!;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class CommentItemDto
    {
        [JsonPropertyName("overall_rating")]
        public int OverallRating { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; } = null!;
    }

    public class AggregatedResultsDto
    {
        [JsonPropertyName("view")]
        public string View { get; set; } = "aggregated";

        [JsonPropertyName("cycle_id")]
        public Guid CycleId { get; set; }

        [JsonPropertyName("cycle_title")]
        public string CycleTitle { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("average_rating")]
        public double AverageRating { get; set; }

        [JsonPropertyName("response_count")]
        public int ResponseCount { get; set; }

        [JsonPropertyName("comments")]
        public CommentItemDto[] Comments { get; set; } = Array.Empty<CommentItemDto>();
    }

    public class DetailedResultsDto
    {
        [JsonPropertyName("view")]
        public string View { get; set; } = "detailed";

        [JsonPropertyName("cycle_id")]
        public Guid CycleId { get; set; }

        [JsonPropertyName("cycle_title")]
        public string CycleTitle { get; set; } = null!;

        [JsonPropertyName("subject_display_name")]
        public string SubjectDisplayName { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("average_rating")]
        public double AverageRating { get; set; }

        [JsonPropertyName("response_count")]
        public int ResponseCount { get; set; }

        [JsonPropertyName("responses")]
        public DetailedResponseItemDto[] Responses { get; set; } = Array.Empty<DetailedResponseItemDto>();
    }

    public class DetailedResponseItemDto
    {
        [JsonPropertyName("reviewer_display_name")]
        public string ReviewerDisplayName { get; set; } = null!;

        [JsonPropertyName("reviewer_employee_id")]
        public Guid ReviewerEmployeeId { get; set; }

        [JsonPropertyName("overall_rating")]
        public int OverallRating { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; } = null!;

        [JsonPropertyName("submitted_at")]
        public DateTimeOffset SubmittedAt { get; set; }
    }

    public class ReviewRequestDto
    {
        [JsonPropertyName("cycle_id")]
        public Guid CycleId { get; set; }

        [JsonPropertyName("cycle_title")]
        public string CycleTitle { get; set; } = null!;

        [JsonPropertyName("subject_display_name")]
        public string SubjectDisplayName { get; set; } = null!;

        [JsonPropertyName("nominee_status")]
        public string NomineeStatus { get; set; } = null!;

        [JsonPropertyName("cycle_started_at")]
        public DateTimeOffset? CycleStartedAt { get; set; }
    }

    public class PagedResponseDto<T>
    {
        [JsonPropertyName("data")]
        public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();

        [JsonPropertyName("pagination")]
        public PaginationDto Pagination { get; set; } = new();
    }

    public class PaginationDto
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("total_items")]
        public int TotalCount { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
    }

    public class DataListDto<T>
    {
        [JsonPropertyName("data")]
        public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
    }
}
