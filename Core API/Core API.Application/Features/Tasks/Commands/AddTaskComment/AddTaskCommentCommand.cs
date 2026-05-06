using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Commands.AddTaskComment
{
    /// <summary>
    /// Command to add a comment to a task
    /// </summary>
    public record AddTaskCommentCommand : BaseCommand<TaskCommentDto>
    {
        public int TaskId { get; init; }
        public CreateTaskCommentDto CommentDto { get; init; } = null!;
    }
}