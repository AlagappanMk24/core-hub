using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Commands.AddTaskComment
{ 
    public class AddTaskCommentCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddTaskCommentCommandHandler> logger)
        : IRequestHandler<AddTaskCommentCommand, OperationResult<TaskCommentDto>>
    {
        public async Task<OperationResult<TaskCommentDto>> Handle(
            AddTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                var task = await unitOfWork.TaskItems.GetAsync(t => t.Id == request.TaskId);
                if (task == null)
                    return OperationResult<TaskCommentDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskCommentDto>.FailureResult("You don't have permission to comment on this task");

                var comment = mapper.Map<TaskComment>(request.CommentDto);
                comment.TaskId = request.TaskId;
                comment.UserId = context.UserId;
                comment.CreatedDate = DateTime.UtcNow;

                await unitOfWork.TaskComments.AddAsync(comment);
                await unitOfWork.SaveChangesAsync();

                await LogTaskActionAsync(request.TaskId, "Comment Added", request.CommentDto.Comment, context);

                var commentDto = mapper.Map<TaskCommentDto>(comment);
                return OperationResult<TaskCommentDto>.SuccessResult(commentDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding comment to task {TaskId}", request.TaskId);
                return OperationResult<TaskCommentDto>.FailureResult("Failed to add comment");
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
