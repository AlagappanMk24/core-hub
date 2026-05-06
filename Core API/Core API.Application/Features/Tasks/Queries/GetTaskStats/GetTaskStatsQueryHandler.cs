using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskStats
{
    public class GetTaskStatsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetTaskStatsQueryHandler> logger) : IRequestHandler<GetTaskStatsQuery, OperationResult<TaskStatsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetTaskStatsQueryHandler> _logger = logger;

        public async Task<OperationResult<TaskStatsDto>> Handle(GetTaskStatsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.Context;
                _logger.LogInformation("Getting task statistics for user {UserId}", context?.UserId);

                // Build query
                var query = _unitOfWork.TaskItems.Query().Where(t => !t.IsDeleted);

                // Apply user-based filtering
                if (context != null && !context.IsSuperAdmin)
                {
                    query = query.Where(t => t.AssignedToUserId == context.UserId || t.CreatedByUserId == context.UserId);
                }

                var tasks = await query.ToListAsync(cancellationToken);
                var today = DateTime.UtcNow.Date;
                var endOfWeek = today.AddDays(7 - (int)today.DayOfWeek);
                var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                // Calculate statistics
                var stats = new TaskStatsDto
                {
                    TotalTasks = tasks.Count,
                    TasksAssignedToMe = context != null ? tasks.Count(t => t.AssignedToUserId == context.UserId) : 0,
                    TasksCreatedByMe = context != null ? tasks.Count(t => t.CreatedByUserId == context.UserId) : 0,
                    MyTasks = context != null ? tasks.Count(t => t.AssignedToUserId == context.UserId || t.CreatedByUserId == context.UserId) : 0,

                    // Status-based counts
                    PendingTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Pending),
                    InProgressTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.InProgress),
                    CompletedTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed),
                    CancelledTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Cancelled),
                    OnHoldTasks = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.OnHold),

                    // Priority-based counts
                    HighPriorityTasks = tasks.Count(t => t.Priority == TaskPriority.High),
                    UrgentPriorityTasks = tasks.Count(t => t.Priority == TaskPriority.Urgent),

                    // Due date-based counts
                    OverdueTasks = tasks.Count(t => t.Status != Domain.Enums.TaskStatus.Completed && t.DueDate.HasValue && t.DueDate.Value.Date < today),
                    TasksDueToday = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date == today),
                    TasksDueThisWeek = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date >= today && t.DueDate.Value.Date <= endOfWeek),
                    TasksDueThisMonth = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date >= today && t.DueDate.Value.Date <= endOfMonth),

                    // Grouped statistics
                    TasksByCategory = tasks.GroupBy(t => t.Category ?? "Uncategorized")
                                           .ToDictionary(g => g.Key, g => g.Count()),
                    TasksByStatus = tasks.GroupBy(t => t.Status.ToString())
                                         .ToDictionary(g => g.Key, g => g.Count()),
                    TasksByPriority = tasks.GroupBy(t => t.Priority.ToString())
                                           .ToDictionary(g => g.Key, g => g.Count())
                };

                _logger.LogInformation("Task statistics retrieved successfully for user {UserId}", context?.UserId);

                return OperationResult<TaskStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task statistics");
                return OperationResult<TaskStatsDto>.FailureResult("Failed to retrieve task statistics");
            }
        }
    }

}
