using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Commands.DeleteTask
{
    public class DeleteTaskCommandHandler(
         IUnitOfWork unitOfWork,
         ILogger<DeleteTaskCommandHandler> logger)
         : IRequestHandler<DeleteTaskCommand, OperationResult<bool>>
    {
        public async Task<OperationResult<bool>> Handle(
            DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                var task = await unitOfWork.TaskItems.GetAsync(t => t.Id == request.TaskId);
                if (task == null)
                    return OperationResult<bool>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<bool>.FailureResult("You don't have permission to delete this task");

                var hasSubtasks = await unitOfWork.TaskItems.HasSubtasksAsync(request.TaskId);
                if (hasSubtasks)
                    return OperationResult<bool>.FailureResult("Cannot delete task with subtasks. Delete subtasks first.");

                task.IsDeleted = true;
                task.UpdatedDate = DateTime.UtcNow;

                unitOfWork.TaskItems.Update(task);
                await unitOfWork.SaveChangesAsync();

                await LogTaskActionAsync(request.TaskId, "Deleted", $"Task deleted by {context.UserId}", context);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting task {TaskId}", request.TaskId);
                return OperationResult<bool>.FailureResult("Failed to delete task");
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
