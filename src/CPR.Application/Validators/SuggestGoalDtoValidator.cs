using System;
using CPR.Application.Contracts;
using FluentValidation;

namespace CPR.Application.Validators
{
    /// <summary>
    /// Validator for <see cref="SuggestGoalDto"/>.
    /// </summary>
    public class SuggestGoalDtoValidator : AbstractValidator<SuggestGoalDto>
    {
        private static readonly string[] ValidTimeframes = { "week", "month", "quarter", "year" };

        public SuggestGoalDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("errors.validation.name")
                .MaximumLength(200).WithMessage("errors.validation.name");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("errors.validation.description")
                .When(x => x.Description != null);

            RuleFor(x => x.Timeframe)
                .NotEmpty().WithMessage("errors.validation.timeframe")
                .Must(t => Array.Exists(ValidTimeframes, v => v == t))
                .WithMessage("errors.validation.timeframe");

            RuleFor(x => x.DueDate)
                .Must(d => d == null || d.Value.Date >= DateTime.UtcNow.Date)
                .WithMessage("errors.validation.due_date")
                .When(x => x.DueDate.HasValue);
        }
    }
}
