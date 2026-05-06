using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskStats
{
    /// <summary>
    /// Query to retrieve task statistics for the current user or company.
    /// </summary>
    public record GetTaskStatsQuery : BaseQuery<TaskStatsDto>
    {
        // No additional properties needed
    }
}