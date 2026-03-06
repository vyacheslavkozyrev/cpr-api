using CPR.Application.DTOs.Taxonomy;
using FluentValidation;

namespace CPR.Application.Validators.Taxonomy
{
    public class CreatePositionDtoValidator : AbstractValidator<CreatePositionDto>
    {
        public CreatePositionDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("errors.validation.title_required")
                .MaximumLength(200).WithMessage("errors.validation.title_too_long");
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
            RuleFor(x => x.Expectations)
                .MaximumLength(2000).WithMessage("errors.validation.expectations_too_long")
                .When(x => x.Expectations != null);
            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("errors.validation.sort_order_negative");
            RuleFor(x => x.CareerTrackId)
                .NotEmpty().WithMessage("errors.validation.career_track_id_required");
        }
    }

    public class UpdatePositionDtoValidator : AbstractValidator<UpdatePositionDto>
    {
        public UpdatePositionDtoValidator()
        {
            RuleFor(x => x.Title)
                .MinimumLength(1).WithMessage("errors.validation.title_required")
                .MaximumLength(200).WithMessage("errors.validation.title_too_long")
                .When(x => x.Title != null);
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("errors.validation.description_too_long")
                .When(x => x.Description != null);
            RuleFor(x => x.Expectations)
                .MaximumLength(2000).WithMessage("errors.validation.expectations_too_long")
                .When(x => x.Expectations != null);
            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("errors.validation.sort_order_negative")
                .When(x => x.SortOrder.HasValue);
        }
    }
}
