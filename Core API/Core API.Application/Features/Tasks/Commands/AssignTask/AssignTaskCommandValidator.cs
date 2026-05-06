using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.AssignTask
{
    public class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
    {
        public AssignTaskCommandValidator()
        {
            RuleFor(x => x.Context).NotNull();
            RuleFor(x => x.TaskId).GreaterThan(0);
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.")
                .MaximumLength(450);
        }
    }
}