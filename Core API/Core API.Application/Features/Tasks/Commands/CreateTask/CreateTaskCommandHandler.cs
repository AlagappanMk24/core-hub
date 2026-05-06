using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Commands.CreateTask
{
    // <summary>
    /// Handles creation of a new task
    /// </summary>
    public class CreateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateTaskCommandHandler> logger)
        : IRequestHandler<CreateTaskCommand, OperationResult<TaskDto>>
    {
        public async Task<OperationResult<TaskDto>> Handle(
            CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                if (string.IsNullOrWhiteSpace(context.UserId))
                    return OperationResult<TaskDto>.FailureResult("Invalid user context");

                var task = mapper.Map<TaskItem>(request.CreateDto);
                task.CreatedByUserId = context.UserId;
                task.CreatedDate = DateTime.UtcNow;

                // Set default assignee if not provided
                if (string.IsNullOrEmpty(task.AssignedToUserId))
                    task.AssignedToUserId = context.UserId;

                await unitOfWork.TaskItems.AddAsync(task);
                await unitOfWork.SaveChangesAsync();

                // Log action
                await LogTaskActionAsync(task.Id, "Created", $"Task created by {context.UserId}", context);

                // Get full task with details
                var createdTask = await unitOfWork.TaskItems.GetTaskWithDetailsAsync(task.Id);
                var taskDto = mapper.Map<TaskDto>(createdTask);

                return OperationResult<TaskDto>.SuccessResult(taskDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating task");
                return OperationResult<TaskDto>.FailureResult("Failed to create task");
            }
        }

        private async Task LogTaskActionAsync(int taskId, string action, string description, OperationContext context)
        {
            var auditLog = new TaskAuditLog
            {
                TaskId = taskId,
                Action = action,
                Description = description,
                PerformedByUserId = context.UserId,
                CreatedDate = DateTime.UtcNow
            };

            await unitOfWork.TaskAuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
