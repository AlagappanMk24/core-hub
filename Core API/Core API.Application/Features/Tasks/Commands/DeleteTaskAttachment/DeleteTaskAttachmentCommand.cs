using Core_API.Application.Common.Base;

namespace Core_API.Application.Features.Tasks.Commands.DeleteTaskAttachment
{
    /// <summary>
    /// Command to delete an attachment from a task
    /// </summary>
    public record DeleteTaskAttachmentCommand : BaseCommand<bool>
    {
        public int TaskId { get; init; }
        public int AttachmentId { get; init; }
    }
}