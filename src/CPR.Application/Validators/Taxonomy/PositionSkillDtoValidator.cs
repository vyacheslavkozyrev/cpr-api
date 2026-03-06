using CPR.Application.DTOs.Taxonomy;
using FluentValidation;

namespace CPR.Application.Validators.Taxonomy
{
    public class AddPositionSkillDtoValidator : AbstractValidator<AddPositionSkillDto>
    {
        public AddPositionSkillDtoValidator()
        {
            RuleFor(x => x.SkillId)
                .NotEmpty().WithMessage("errors.validation.skill_id_required");
            RuleFor(x => x.SkillLevelId)
                .NotEmpty().WithMessage("errors.validation.skill_level_id_required");
            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("errors.validation.weight_must_be_positive")
                .When(x => x.Weight.HasValue);
            RuleFor(x => x.Rationale)
                .MaximumLength(500).WithMessage("errors.validation.rationale_too_long")
                .When(x => x.Rationale != null);
        }
    }

    public class UpdatePositionSkillDtoValidator : AbstractValidator<UpdatePositionSkillDto>
    {
        public UpdatePositionSkillDtoValidator()
        {
            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("errors.validation.weight_must_be_positive")
                .When(x => x.Weight.HasValue);
            RuleFor(x => x.Rationale)
                .MaximumLength(500).WithMessage("errors.validation.rationale_too_long")
                .When(x => x.Rationale != null);
        }
    }
}
