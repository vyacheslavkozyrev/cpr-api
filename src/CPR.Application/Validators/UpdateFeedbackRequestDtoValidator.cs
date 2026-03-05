using System;
using CPR.Application.Contracts;
using FluentValidation;

namespace CPR.Application.Validators
{
    /// <summary>
    /// Validator for UpdateFeedbackRequestDto
    /// </summary>
    public class UpdateFeedbackRequestDtoValidator : AbstractValidator<UpdateFeedbackRequestDto>
    {
        public UpdateFeedbackRequestDtoValidator()
        {
            // Validate due_date (if provided, must be today or future)
            RuleFor(x => x.DueDate)
                .Must(date => !date.HasValue || date.Value.Date >= DateTimeOffset.UtcNow.Date)
                .WithMessage("Due date must be today or in the future")
                .When(x => x.DueDate.HasValue);
        }
    }
}
