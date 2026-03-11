// Validation for SkillAssessment DTOs is enforced via DataAnnotations on the DTO classes in
// CPR.Application.DTOs.SkillAssessment. ASP.NET Core's model binding validates these automatically
// when [ApiController] is applied on the controller.
// This file documents the validation rules per the spec.
//
// UpsertSkillAssessmentDto:    self_assessment_value Required, > 0; notes MaxLength(1000)
// UpsertManagerAssessmentDto:  manager_assessment_value Required, > 0
// LinkEvidenceDto:             feedback_id Required (non-empty Guid)
//
// Business-rule validations enforced in SkillAssessmentService:
// - skill_id must be in the user's current position's position_to_skill set → 404
// - feedback must have to_employee_id = current employee → 404 feedback_not_found
// - current assessment must exist before evidence can be linked → 422 assessment_required
// - feedback already linked to this skill → 422 already_linked
// - PeopleManager may only set manager_assessment_value for direct reports → 403

namespace CPR.Application.Validators
{
}
