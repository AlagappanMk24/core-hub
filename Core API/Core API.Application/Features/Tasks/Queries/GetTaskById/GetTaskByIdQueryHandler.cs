using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskById
{
    public class GetTaskByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetTaskByIdQueryHandler> logger)
    : IRequestHandler<GetTaskByIdQuery, OperationResult<TaskDto>>
    {
        public async Task<OperationResult<TaskDto>> Handle(
        GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                var task = await unitOfWork.TaskItems.GetTaskWithDetailsAsync(request.TaskId);

                if (task == null)
                    return OperationResult<TaskDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskDto>.FailureResult("You don't have permission to view this task");

                var taskDto = mapper.Map<TaskDto>(task);

                return OperationResult<TaskDto>.SuccessResult(taskDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving task {TaskId} for user {UserId}", request.TaskId, context.UserId);
                return OperationResult<TaskDto>.FailureResult("Failed to retrieve task.");
            }
        }

        private bool CanAccessTask(TaskItem task, OperationContext context)
        {
            if (context.IsSuperAdmin) return true;
            return task.AssignedToUserId == context.UserId || task.CreatedByUserId == context.UserId;
        }
    }
}
