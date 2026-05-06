using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.AddTaskAttachment
{
    public class AddTaskAttachmentCommandValidator : AbstractValidator<AddTaskAttachmentCommand>
    {
        public AddTaskAttachmentCommandValidator()
        {
            RuleFor(x => x.Context).NotNull();
            RuleFor(x => x.TaskId).GreaterThan(0);

            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.");
        }
    }
}