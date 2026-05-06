using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Commands.AddTaskAttachment
{
    public class AddTaskAttachmentCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddTaskAttachmentCommandHandler> logger)
        : IRequestHandler<AddTaskAttachmentCommand, OperationResult<TaskAttachmentDto>>
    {
        public async Task<OperationResult<TaskAttachmentDto>> Handle(
            AddTaskAttachmentCommand request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                var task = await unitOfWork.TaskItems.GetAsync(t => t.Id == request.TaskId);
                if (task == null)
                    return OperationResult<TaskAttachmentDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskAttachmentDto>.FailureResult("You don't have permission to add attachments to this task");

                if (request.File == null || request.File.Length == 0)
                    return OperationResult<TaskAttachmentDto>.FailureResult("No file provided");

                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (request.File.Length > maxFileSize)
                    return OperationResult<TaskAttachmentDto>.FailureResult($"File size exceeds maximum allowed size of 10MB");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Tasks", request.TaskId.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                var attachment = new TaskAttachment
                {
                    TaskId = request.TaskId,
                    FileName = request.File.FileName,
                    FilePath = filePath,
                    FileUrl = $"/api/tasks/{request.TaskId}/attachments/download/{Guid.NewGuid()}",
                    FileSize = request.File.Length,
                    ContentType = request.File.ContentType,
                    CreatedDate = DateTime.UtcNow
                };

                await unitOfWork.TaskAttachments.AddAsync(attachment);
                await unitOfWork.SaveChangesAsync();

                await LogTaskActionAsync(request.TaskId, "Attachment Added", request.File.FileName, context);

                var attachmentDto = mapper.Map<TaskAttachmentDto>(attachment);
                return OperationResult<TaskAttachmentDto>.SuccessResult(attachmentDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding attachment to task {TaskId}", request.TaskId);
                return OperationResult<TaskAttachmentDto>.FailureResult("Failed to add attachment");
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