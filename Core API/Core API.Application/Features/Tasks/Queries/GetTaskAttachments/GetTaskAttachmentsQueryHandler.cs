using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskAttachments
{
    public class GetTaskAttachmentsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetTaskAttachmentsQueryHandler> logger) : IRequestHandler<GetTaskAttachmentsQuery, OperationResult<IEnumerable<TaskAttachmentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetTaskAttachmentsQueryHandler> _logger = logger;

        public async Task<OperationResult<IEnumerable<TaskAttachmentDto>>> Handle(GetTaskAttachmentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.Context;
                _logger.LogInformation("Getting attachments for task {TaskId} by user {UserId}", request.TaskId, context?.UserId);

                // Validate task exists
                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == request.TaskId);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found", request.TaskId);
                    return OperationResult<IEnumerable<TaskAttachmentDto>>.FailureResult("Task not found");
                }

                // Check permission
                if (!CanAccessTask(task, context))
                {
                    _logger.LogWarning("User {UserId} doesn't have permission to view attachments for task {TaskId}",
                        context?.UserId, request.TaskId);
                    return OperationResult<IEnumerable<TaskAttachmentDto>>.FailureResult("You don't have permission to view attachments");
                }

                // Get attachments
                var attachments = await _unitOfWork.TaskAttachments.GetAttachmentsByTaskAsync(request.TaskId);
                var attachmentDtos = _mapper.Map<IEnumerable<TaskAttachmentDto>>(attachments);

                _logger.LogInformation("Retrieved {Count} attachments for task {TaskId}", attachmentDtos.Count(), request.TaskId);

                return OperationResult<IEnumerable<TaskAttachmentDto>>.SuccessResult(attachmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments for task {TaskId}", request.TaskId);
                return OperationResult<IEnumerable<TaskAttachmentDto>>.FailureResult("Failed to retrieve attachments");
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