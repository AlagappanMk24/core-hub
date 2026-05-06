using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskAttachments
{
    public record GetTaskAttachmentsQuery : BaseQuery<IEnumerable<TaskAttachmentDto>>
    {
        [Required(ErrorMessage = "Task ID is required")]
        public int TaskId { get; set; }
    }
}