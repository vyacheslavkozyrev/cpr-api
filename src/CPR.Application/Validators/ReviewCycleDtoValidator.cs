// Validation for ReviewCycle DTOs is enforced via DataAnnotations on the DTO classes in
// CPR.Application.DTOs.ReviewCycles. ASP.NET Core's model binding validates these automatically
// when [ApiController] is applied on the controller.
// This file is a placeholder documenting validation rules per the spec.
//
// CreateReviewCycleDto:   title NotEmpty, MaxLength(200); subject_employee_id NotEmpty; description MaxLength(2000)
// TransitionCycleStatusDto: status one of "open", "in_progress", "closed"
// AddReviewNomineeDto:    reviewer_employee_id NotEmpty
// SubmitReviewResponseDto: overall_rating 1-5; comments NotEmpty, MinLength(10), MaxLength(2000)
// ListReviewCyclesQueryDto: page >= 1; page_size 1-100; sort_dir "asc" or "desc"

namespace CPR.Application.Validators
{
}
