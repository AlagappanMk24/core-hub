using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.DeleteTask
{
    /// <summary>
    /// Validator for DeleteTaskCommand
    /// </summary>
    public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
    {
        public DeleteTaskCommandValidator()
        {
            RuleFor(x => x.Context)
                .NotNull().WithMessage("Operation context is required.");

            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("Task ID must be greater than 0.");
        }
    }
}