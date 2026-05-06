using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskComments
{
    public class GetTaskCommentsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetTaskCommentsQueryHandler> logger) : IRequestHandler<GetTaskCommentsQuery, OperationResult<IEnumerable<TaskCommentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetTaskCommentsQueryHandler> _logger = logger;

        public async Task<OperationResult<IEnumerable<TaskCommentDto>>> Handle(GetTaskCommentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.Context;
                _logger.LogInformation("Getting comments for task {TaskId} by user {UserId}", request.TaskId, context?.UserId);

                // Validate task exists
                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == request.TaskId);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found", request.TaskId);
                    return OperationResult<IEnumerable<TaskCommentDto>>.FailureResult("Task not found");
                }

                // Check permission
                if (!CanAccessTask(task, context))
                {
                    _logger.LogWarning("User {UserId} doesn't have permission to view comments for task {TaskId}",
                        context?.UserId, request.TaskId);
                    return OperationResult<IEnumerable<TaskCommentDto>>.FailureResult("You don't have permission to view comments");
                }

                // Get comments
                var comments = await _unitOfWork.TaskComments.GetCommentsByTaskAsync(request.TaskId);
                var commentDtos = _mapper.Map<IEnumerable<TaskCommentDto>>(comments);

                _logger.LogInformation("Retrieved {Count} comments for task {TaskId}", commentDtos.Count(), request.TaskId);

                return OperationResult<IEnumerable<TaskCommentDto>>.SuccessResult(commentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for task {TaskId}", request.TaskId);
                return OperationResult<IEnumerable<TaskCommentDto>>.FailureResult("Failed to retrieve comments");
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
