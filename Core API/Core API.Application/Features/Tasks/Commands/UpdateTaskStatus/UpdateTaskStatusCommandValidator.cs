using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
    {
        public UpdateTaskStatusCommandValidator()
        {
            RuleFor(x => x.Context).NotNull();
            RuleFor(x => x.TaskId).GreaterThan(0);
            RuleFor(x => x.Status).IsInEnum().WithMessage("Invalid task status.");
        }
    }
}