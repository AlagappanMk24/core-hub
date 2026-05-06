using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Commands.CreateTask
{
    /// <summary>
    /// Command to create a new task
    /// </summary>
    public record CreateTaskCommand : BaseCommand<TaskDto>
    {
        public CreateTaskDto CreateDto { get; init; } = null!;
    }

    public record CreateTaskResponse
    {
        public TaskDto Task { get; init; } = null!;
    }
}
