using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Commands.DeleteTaskAttachment
{
    public class DeleteTaskAttachmentCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteTaskAttachmentCommandHandler> logger)
            : IRequestHandler<DeleteTaskAttachmentCommand, OperationResult<bool>>
    {
        public async Task<OperationResult<bool>> Handle(
            DeleteTaskAttachmentCommand request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                var attachment = await unitOfWork.TaskAttachments.GetAsync(a =>
                    a.Id == request.AttachmentId && a.TaskId == request.TaskId);

                if (attachment == null)
                    return OperationResult<bool>.FailureResult("Attachment not found");

                var task = await unitOfWork.TaskItems.GetAsync(t => t.Id == request.TaskId);
                if (!CanAccessTask(task, context))
                    return OperationResult<bool>.FailureResult("You don't have permission to delete this attachment");

                if (System.IO.File.Exists(attachment.FilePath))
                {
                    System.IO.File.Delete(attachment.FilePath);
                }

                attachment.IsDeleted = true;
                attachment.UpdatedDate = DateTime.UtcNow;

                unitOfWork.TaskAttachments.Update(attachment);
                await unitOfWork.SaveChangesAsync();

                await LogTaskActionAsync(request.TaskId, "Attachment Deleted", attachment.FileName, context);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting attachment {AttachmentId} from task {TaskId}",
                    request.AttachmentId, request.TaskId);
                return OperationResult<bool>.FailureResult("Failed to delete attachment");
            }
        }

        private bool CanAccessTask(TaskItem? task, OperationContext context)
        {
            if (task == null || context.IsSuperAdmin) return true;
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
