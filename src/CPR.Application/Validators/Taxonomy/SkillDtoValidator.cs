using CPR.Application.DTOs.Taxonomy;
using FluentValidation;

namespace CPR.Application.Validators.Taxonomy
{
    public class CreateSkillDtoValidator : AbstractValidator<CreateSkillDto>
    {
        public CreateSkillDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("errors.validation.title_required")
                .MaximumLength(200).WithMessage("errors.validation.title_too_long");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("errors.validation.category_id_required");
        }
    }

    public class UpdateSkillDtoValidator : AbstractValidator<UpdateSkillDto>
    {
        public UpdateSkillDtoValidator()
        {
            RuleFor(x => x.Title)
                .MinimumLength(1).WithMessage("errors.validation.title_required")
                .MaximumLength(200).WithMessage("errors.validation.title_too_long")
                .When(x => x.Title != null);
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
        }
    }

    public class AddSkillLevelDtoValidator : AbstractValidator<AddSkillLevelDto>
    {
        public AddSkillLevelDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("errors.validation.title_required")
                .MaximumLength(100).WithMessage("errors.validation.title_too_long");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
            RuleFor(x => x.Value)
                .InclusiveBetween(1, 5).WithMessage("errors.validation.level_value_out_of_range");
        }
    }

    public class UpdateSkillLevelDtoValidator : AbstractValidator<UpdateSkillLevelDto>
    {
        public UpdateSkillLevelDtoValidator()
        {
            RuleFor(x => x.Title)
                .MinimumLength(1).WithMessage("errors.validation.title_required")
                .MaximumLength(100).WithMessage("errors.validation.title_too_long")
                .When(x => x.Title != null);
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
            RuleFor(x => x.Value)
                .InclusiveBetween(1, 5).WithMessage("errors.validation.level_value_out_of_range")
                .When(x => x.Value.HasValue);
        }
    }
}
