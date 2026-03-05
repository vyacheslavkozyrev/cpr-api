using System;
using System.Linq;
using CPR.Application.Contracts;
using FluentValidation;

namespace CPR.Application.Validators
{
    /// <summary>
    /// Validator for CreateFeedbackRequestDto
    /// </summary>
    public class CreateFeedbackRequestDtoValidator : AbstractValidator<CreateFeedbackRequestDto>
    {
        public CreateFeedbackRequestDtoValidator()
        {
            // Validate employee_ids list
            RuleFor(x => x.EmployeeIds)
                .NotNull()
                .WithMessage("Recipients list is required")
                .NotEmpty()
                .WithMessage("At least one recipient is required")
                .Must(list => list != null && list.Count >= 1)
                .WithMessage("At least one recipient is required")
                .Must(list => list != null && list.Count <= 20)
                .WithMessage("Maximum 20 recipients allowed per request")
                .Must(list => list != null && list.Distinct().Count() == list.Count)
                .WithMessage("Duplicate recipients found in the list");

            // Validate each employee ID is valid GUID
            RuleForEach(x => x.EmployeeIds)
                .NotEmpty()
                .WithMessage("Invalid employee ID");

            // Validate message
            RuleFor(x => x.Message)
                .MaximumLength(500)
                .WithMessage("Message cannot exceed 500 characters");

            // Validate due_date
            RuleFor(x => x.DueDate)
                .Must(date => !date.HasValue || date.Value.Date >= DateTimeOffset.UtcNow.Date)
                .WithMessage("Due date must be today or in the future")
                .When(x => x.DueDate.HasValue);

            // Validate project_id (if provided, must be valid GUID)
            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("Invalid project ID")
                .When(x => x.ProjectId.HasValue);

            // Validate goal_id (if provided, must be valid GUID)
            RuleFor(x => x.GoalId)
                .NotEmpty()
                .WithMessage("Invalid goal ID")
                .When(x => x.GoalId.HasValue);
        }
    }
}
