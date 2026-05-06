using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetTasksDueToday
{
    public class GetTasksDueTodayQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetTasksDueTodayQueryHandler> logger) : IRequestHandler<GetTasksDueTodayQuery, OperationResult<IEnumerable<TaskDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetTasksDueTodayQueryHandler> _logger = logger;

        public async Task<OperationResult<IEnumerable<TaskDto>>> Handle(GetTasksDueTodayQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.Context;
                _logger.LogInformation("Getting tasks due today for user {UserId}", context?.UserId);

                // Get tasks due today from repository
                var dueTodayTasks = await _unitOfWork.TaskItems.GetTasksDueTodayAsync();

                // Filter by permission
                var filteredTasks = dueTodayTasks.Where(t => CanAccessTask(t, context));

                // Map to DTOs
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(filteredTasks);

                _logger.LogInformation("Retrieved {Count} tasks due today for user {UserId}", taskDtos.Count(), context?.UserId);

                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks due today");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve tasks due today");
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