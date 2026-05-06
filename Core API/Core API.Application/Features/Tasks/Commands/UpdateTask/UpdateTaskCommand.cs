using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
namespace Core_API.Application.Features.Tasks.Commands.UpdateTask
{
    /// <summary>
    /// Command to update an existing task
    /// </summary>
    public record UpdateTaskCommand : BaseCommand<TaskDto>
    {
        public int TaskId { get; init; }
        public UpdateTaskDto UpdateDto { get; init; } = null!;
    }
}