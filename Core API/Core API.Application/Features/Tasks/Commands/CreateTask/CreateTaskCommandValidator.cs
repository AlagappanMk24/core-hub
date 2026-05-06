using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.CreateTask
{
    /// <summary>
    /// Validator for CreateTaskCommand
    /// </summary>
    public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
    {
        public CreateTaskCommandValidator()
        {
            RuleFor(x => x.Context)
                .NotNull().WithMessage("Operation context is required.");

            RuleFor(x => x.CreateDto)
                .NotNull().WithMessage("Create task data is required.");

            RuleFor(x => x.CreateDto.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.CreateDto.Description)
                .MaximumLength(1000).When(x => x.CreateDto.Description != null)
                .WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.CreateDto.Priority)
                .IsInEnum().WithMessage("Invalid priority value.");

            RuleFor(x => x.CreateDto.DueDate)
                .GreaterThan(DateTime.UtcNow).When(x => x.CreateDto.DueDate.HasValue)
                .WithMessage("Due date must be in the future.");

            RuleFor(x => x.CreateDto.Category)
                .MaximumLength(50).When(x => x.CreateDto.Category != null);

            RuleFor(x => x.CreateDto.Tag)
                .MaximumLength(50).When(x => x.CreateDto.Tag != null);

            RuleFor(x => x.CreateDto.EstimatedHours)
                .GreaterThan(0).When(x => x.CreateDto.EstimatedHours.HasValue)
                .WithMessage("Estimated hours must be greater than 0.");
        }
    }
}