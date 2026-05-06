using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Queries.GetMyTasks
{
    /// <summary>
    /// Query to retrieve tasks assigned to or created by the current user.
    /// </summary>
    public record GetMyTasksQuery : BaseQuery<IEnumerable<TaskDto>>
    {
        public TaskFilterDto? Filter { get; init; }
    }
}
