using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.DeleteTaskAttachment
{
    public class DeleteAttachmentCommandValidator : AbstractValidator<DeleteTaskAttachmentCommand>
    {
        public DeleteAttachmentCommandValidator()
        {
            RuleFor(x => x.Context).NotNull();
            RuleFor(x => x.TaskId).GreaterThan(0);
            RuleFor(x => x.AttachmentId).GreaterThan(0);
        }
    }
}