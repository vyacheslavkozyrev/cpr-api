// Validation for SkillAssessment DTOs is enforced via DataAnnotations on the DTO classes in
// CPR.Application.DTOs.SkillAssessment. ASP.NET Core's model binding validates these automatically
// when [ApiController] is applied on the controller.
// This file documents the validation rules per the spec.
//
// UpsertSkillAssessmentDto: skill_level_id Required (non-empty Guid); notes MaxLength(1000)
// UpsertSkillTargetDto:     skill_level_id Required (non-empty Guid)
// LinkEvidenceDto:          feedback_id Required (non-empty Guid)
//
// Business-rule validations enforced in SkillAssessmentService:
// - skill_id must be in the user's current position's position_to_skill set → 404
// - skill_level_id must belong to the given skill → 422 invalid_level
// - target level value must be strictly greater than current assessed level value → 422 target_too_low
// - feedback must have to_employee_id = current employee → 404 feedback_not_found
// - current assessment must exist before evidence can be linked → 422 assessment_required
// - feedback already linked to this skill → 422 already_linked

namespace CPR.Application.Validators
{
}
