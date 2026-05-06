using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetTasksDueSoon
{
    public class GetTasksDueSoonQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetTasksDueSoonQueryHandler> logger) : IRequestHandler<GetTasksDueSoonQuery, OperationResult<IEnumerable<TaskDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetTasksDueSoonQueryHandler> _logger = logger;

        public async Task<OperationResult<IEnumerable<TaskDto>>> Handle(GetTasksDueSoonQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.Context;
                _logger.LogInformation("Getting tasks due within {Days} days for user {UserId}", request.Days, context?.UserId);

                // Validate days parameter
                if (request.Days <= 0)
                {
                    return OperationResult<IEnumerable<TaskDto>>.FailureResult("Days must be greater than 0");
                }

                // Get tasks due soon from repository
                var dueSoonTasks = await _unitOfWork.TaskItems.GetTasksDueSoonAsync(request.Days);

                // Filter by permission
                var filteredTasks = dueSoonTasks.Where(t => CanAccessTask(t, context));

                // Map to DTOs
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(filteredTasks);

                _logger.LogInformation("Retrieved {Count} tasks due within {Days} days for user {UserId}",
                    taskDtos.Count(), request.Days, context?.UserId);

                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks due soon");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve tasks due soon");
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