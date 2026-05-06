using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.UpdateTask
{
    /// <summary>
    /// Validator for UpdateTaskCommand
    /// </summary>
    public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
    {
        public UpdateTaskCommandValidator()
        {
            RuleFor(x => x.Context)
                .NotNull().WithMessage("Operation context is required.");

            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("Task ID must be greater than 0.");

            RuleFor(x => x.UpdateDto.Title)
                .MaximumLength(200).When(x => x.UpdateDto.Title != null);

            RuleFor(x => x.UpdateDto.Description)
                .MaximumLength(1000).When(x => x.UpdateDto.Description != null);

            RuleFor(x => x.UpdateDto.Priority)
                .IsInEnum().When(x => x.UpdateDto.Priority.HasValue);

            RuleFor(x => x.UpdateDto.Status)
                .IsInEnum().When(x => x.UpdateDto.Status.HasValue);

            RuleFor(x => x.UpdateDto.DueDate)
                .GreaterThan(DateTime.UtcNow).When(x => x.UpdateDto.DueDate.HasValue)
                .WithMessage("Due date must be in the future.");

            RuleFor(x => x.UpdateDto.Category)
                .MaximumLength(50).When(x => x.UpdateDto.Category != null);

            RuleFor(x => x.UpdateDto.Tag)
                .MaximumLength(50).When(x => x.UpdateDto.Tag != null);

            RuleFor(x => x.UpdateDto.EstimatedHours)
                .GreaterThan(0).When(x => x.UpdateDto.EstimatedHours.HasValue);

            RuleFor(x => x.UpdateDto.ActualHours)
                .GreaterThan(0).When(x => x.UpdateDto.ActualHours.HasValue);
        }
    }
}
