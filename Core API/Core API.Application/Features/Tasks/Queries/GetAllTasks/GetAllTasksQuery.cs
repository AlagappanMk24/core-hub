using Core_API.Application.Common.Base;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using MediatR;

namespace Core_API.Application.Features.Tasks.Queries.GetAllTasks
{
    /// <summary>
    /// Query to retrieve all tasks with optional filtering, sorting, and pagination.
    /// </summary>
    public record GetAllTasksQuery : IRequest<OperationResult<PaginatedResult<TaskDto>>>
    {
        public TaskFilterDto? Filter { get; init; }
        public OperationContext Context { get; init; }
    }
}
