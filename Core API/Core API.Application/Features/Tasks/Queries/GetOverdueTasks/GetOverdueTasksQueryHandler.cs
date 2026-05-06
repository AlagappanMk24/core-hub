using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetOverdueTasks
{
    public class GetOverdueTasksQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetOverdueTasksQueryHandler> logger) : IRequestHandler<GetOverdueTasksQuery, OperationResult<IEnumerable<TaskDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetOverdueTasksQueryHandler> _logger = logger;

        public async Task<OperationResult<IEnumerable<TaskDto>>> Handle(GetOverdueTasksQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.Context;
                _logger.LogInformation("Getting overdue tasks for user {UserId}", context?.UserId);

                // Get overdue tasks from repository
                var overdueTasks = await _unitOfWork.TaskItems.GetOverdueTasksAsync();

                // Filter by permission
                var filteredTasks = overdueTasks.Where(t => CanAccessTask(t, context));

                // Map to DTOs
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(filteredTasks);

                _logger.LogInformation("Retrieved {Count} overdue tasks for user {UserId}", taskDtos.Count(), context?.UserId);

                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue tasks");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve overdue tasks");
            }
        }
        private bool CanAccessTask(TaskItem task, OperationContext context)
        {
            if (context == null) return false;
            if (context.IsSuperAdmin) return true;
            return task.AssignedToUserId == context.UserId || task.CreatedByUserId == context.UserId;
        }
    }
}