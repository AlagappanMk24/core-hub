using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Commands.UpdateTask
{
    public class UpdateTaskCommandHandler(
         IUnitOfWork unitOfWork,
         IMapper mapper,
         ILogger<UpdateTaskCommandHandler> logger)
         : IRequestHandler<UpdateTaskCommand, OperationResult<TaskDto>>
    {
        public async Task<OperationResult<TaskDto>> Handle(
            UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                var task = await unitOfWork.TaskItems.GetTaskWithDetailsAsync(request.TaskId);
                if (task == null)
                    return OperationResult<TaskDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskDto>.FailureResult("You don't have permission to update this task");

                var changedFields = new List<string>();

                // Update fields if provided
                if (request.UpdateDto.Title != null && task.Title != request.UpdateDto.Title)
                {
                    changedFields.Add($"Title: '{task.Title}' → '{request.UpdateDto.Title}'");
                    task.Title = request.UpdateDto.Title;
                }

                if (request.UpdateDto.Description != null && task.Description != request.UpdateDto.Description)
                {
                    changedFields.Add("Description updated");
                    task.Description = request.UpdateDto.Description;
                }

                if (request.UpdateDto.Priority.HasValue && task.Priority != request.UpdateDto.Priority.Value)
                {
                    changedFields.Add($"Priority: {task.Priority} → {request.UpdateDto.Priority.Value}");
                    task.Priority = request.UpdateDto.Priority.Value;
                }

                if (request.UpdateDto.Status.HasValue && task.Status != request.UpdateDto.Status.Value)
                {
                    changedFields.Add($"Status: {task.Status} → {request.UpdateDto.Status.Value}");
                    task.Status = request.UpdateDto.Status.Value;

                    if (request.UpdateDto.Status.Value == Domain.Enums.TaskStatus.Completed)
                        task.CompletedAt = DateTime.UtcNow;
                }

                if (request.UpdateDto.DueDate.HasValue && task.DueDate != request.UpdateDto.DueDate.Value)
                {
                    changedFields.Add($"DueDate: {task.DueDate?.ToString("yyyy-MM-dd")} → {request.UpdateDto.DueDate.Value.ToString("yyyy-MM-dd")}");
                    task.DueDate = request.UpdateDto.DueDate.Value;
                }

                if (request.UpdateDto.AssignedToUserId != null && task.AssignedToUserId != request.UpdateDto.AssignedToUserId)
                {
                    changedFields.Add($"AssignedTo: {task.AssignedToUserId} → {request.UpdateDto.AssignedToUserId}");
                    task.AssignedToUserId = request.UpdateDto.AssignedToUserId;
                }

                task.UpdatedDate = DateTime.UtcNow;
                unitOfWork.TaskItems.Update(task);
                await unitOfWork.SaveChangesAsync();

                if (changedFields.Any())
                {
                    var changeDescription = $"Task updated. Changes: {string.Join("; ", changedFields)}";
                    await LogTaskActionAsync(task.Id, "Updated", changeDescription, context);
                }

                var updatedTask = await unitOfWork.TaskItems.GetTaskWithDetailsAsync(task.Id);
                var taskDto = mapper.Map<TaskDto>(updatedTask);

                return OperationResult<TaskDto>.SuccessResult(taskDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task {TaskId}", request.TaskId);
                return OperationResult<TaskDto>.FailureResult("Failed to update task");
            }
        }

        private bool CanAccessTask(TaskItem task, OperationContext context)
        {
            if (context.IsSuperAdmin) return true;
            return task.AssignedToUserId == context.UserId || task.CreatedByUserId == context.UserId;
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
