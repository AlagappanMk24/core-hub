using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Queries.GetOverdueTasks
{
    public record GetOverdueTasksQuery : BaseQuery<IEnumerable<TaskDto>>
    {
        // No additional properties needed
    }
}