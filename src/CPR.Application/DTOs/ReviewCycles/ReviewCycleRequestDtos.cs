using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CPR.Application.DTOs.ReviewCycles
{
    public class CreateReviewCycleDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [Required]
        [JsonPropertyName("subject_employee_id")]
        public Guid SubjectEmployeeId { get; set; }

        [StringLength(2000)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class TransitionCycleStatusDto
    {
        [Required]
        [RegularExpression("^(open|in_progress|closed)$", ErrorMessage = "status must be one of: open, in_progress, closed")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    public class AddReviewNomineeDto
    {
        [Required]
        [JsonPropertyName("reviewer_employee_id")]
        public Guid ReviewerEmployeeId { get; set; }
    }

    public class SubmitReviewResponseDto
    {
        [Required]
        [Range(1, 5)]
        [JsonPropertyName("overall_rating")]
        public int OverallRating { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        [JsonPropertyName("comments")]
        public string Comments { get; set; } = null!;
    }

    public class ListReviewCyclesQueryDto
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        public string? Status { get; set; }

        [RegularExpression("^(asc|desc)$", ErrorMessage = "sort_dir must be 'asc' or 'desc'")]
        public string SortDir { get; set; } = "desc";
    }
}
