using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Commands.UpdateTaskStatus
{
    /// <summary>
    /// Command to update task status
    /// </summary>
    public record UpdateTaskStatusCommand : BaseCommand<TaskDto>
    {
        public int TaskId { get; init; }
        public Domain.Enums.TaskStatus Status { get; init; }
    }
}