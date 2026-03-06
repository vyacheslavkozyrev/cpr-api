using CPR.Application.DTOs.Taxonomy;
using FluentValidation;

namespace CPR.Application.Validators.Taxonomy
{
    public class CreateCareerPathDtoValidator : AbstractValidator<CreateCareerPathDto>
    {
        public CreateCareerPathDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("errors.validation.title_required")
                .MaximumLength(200).WithMessage("errors.validation.title_too_long");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
        }
    }

    public class UpdateCareerPathDtoValidator : AbstractValidator<UpdateCareerPathDto>
    {
        public UpdateCareerPathDtoValidator()
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
}
