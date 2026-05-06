using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;
using Microsoft.AspNetCore.Http;

namespace Core_API.Application.Features.Tasks.Commands.AddTaskAttachment
{
    /// <summary>
    /// Command to upload an attachment to a task
    /// </summary>
    public record AddTaskAttachmentCommand : BaseCommand<TaskAttachmentDto>
    {
        public int TaskId { get; init; }
        public IFormFile File { get; init; } = null!;
    }
}