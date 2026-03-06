using CPR.Application.DTOs.Taxonomy;
using FluentValidation;

namespace CPR.Application.Validators.Taxonomy
{
    public class CreateCareerTrackDtoValidator : AbstractValidator<CreateCareerTrackDto>
    {
        public CreateCareerTrackDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("errors.validation.title_required")
                .MaximumLength(200).WithMessage("errors.validation.title_too_long");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
            RuleFor(x => x.CareerPathId)
                .NotEmpty().WithMessage("errors.validation.career_path_id_required");
        }
    }

    public class UpdateCareerTrackDtoValidator : AbstractValidator<UpdateCareerTrackDto>
    {
        public UpdateCareerTrackDtoValidator()
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
