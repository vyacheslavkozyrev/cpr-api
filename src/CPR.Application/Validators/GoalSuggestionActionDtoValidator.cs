using System;
using CPR.Application.Contracts;
using FluentValidation;

namespace CPR.Application.Validators
{
    /// <summary>
    /// Validator for <see cref="GoalSuggestionActionDto"/>.
    /// </summary>
    public class GoalSuggestionActionDtoValidator : AbstractValidator<GoalSuggestionActionDto>
    {
        private static readonly string[] ValidActions = { "accept", "reject" };

        public GoalSuggestionActionDtoValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty().WithMessage("errors.validation.action")
                .Must(a => Array.Exists(ValidActions, v => v == a))
                .WithMessage("errors.validation.action");
        }
    }
}
