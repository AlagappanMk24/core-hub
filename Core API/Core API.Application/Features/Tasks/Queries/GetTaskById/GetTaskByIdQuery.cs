using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskById
{
    /// <summary>
    /// Query to retrieve a single task by its ID with authorization check.
    /// </summary>
    public record GetTaskByIdQuery : BaseQuery<TaskDto>
    {
        public int TaskId { get; init; }
    }
}