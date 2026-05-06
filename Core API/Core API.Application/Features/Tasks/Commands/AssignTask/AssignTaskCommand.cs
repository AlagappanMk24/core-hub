using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Commands.AssignTask
{
    /// <summary>
    /// Command to assign a task to a user
    /// </summary>
    public record AssignTaskCommand : BaseCommand<TaskDto>
    {
        public int TaskId { get; init; }
        public string UserId { get; init; } = string.Empty;
    }
}