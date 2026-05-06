using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.CompleteTask
{
    public class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
    {
        public CompleteTaskCommandValidator()
        {
            RuleFor(x => x.Context).NotNull();
            RuleFor(x => x.TaskId).GreaterThan(0);
        }
    }
}