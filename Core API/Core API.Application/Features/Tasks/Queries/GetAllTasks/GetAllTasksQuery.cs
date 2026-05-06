using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Queries.GetAllTasks
{
    /// <summary>
    /// Query to retrieve all tasks with optional filtering, sorting, and pagination.
    /// </summary>
    public record GetAllTasksQuery : BaseQuery<IEnumerable<TaskDto>>
    {
        public TaskFilterDto? Filter { get; init; }
    }
}
