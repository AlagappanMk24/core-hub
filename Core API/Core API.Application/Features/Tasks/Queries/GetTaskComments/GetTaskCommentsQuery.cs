using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskComments
{
    public record GetTaskCommentsQuery : BaseQuery<IEnumerable<TaskCommentDto>>
    {
        [Required(ErrorMessage = "Task ID is required")]
        public int TaskId { get; set; }
    }
}