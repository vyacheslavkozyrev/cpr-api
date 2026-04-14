using System;
using CPR.Application.Contracts;
using FluentValidation;

namespace CPR.Application.Validators
{
    /// <summary>
    /// Validator for <see cref="GoalDeletionActionDto"/>.
    /// </summary>
    public class GoalDeletionActionDtoValidator : AbstractValidator<GoalDeletionActionDto>
    {
        private static readonly string[] ValidActions = { "approve", "reject" };

        public GoalDeletionActionDtoValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty().WithMessage("errors.validation.action")
                .Must(a => Array.Exists(ValidActions, v => v == a))
                .WithMessage("errors.validation.action");
        }
    }
}
