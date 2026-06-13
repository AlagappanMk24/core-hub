using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Application.Features.Tasks.Queries.Base;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf.Filters;

namespace Core_API.Application.Features.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetAllTasksQueryHandler> logger) : IRequestHandler<GetAllTasksQuery, OperationResult<PaginatedResult<TaskDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetAllTasksQueryHandler> _logger = logger;

        public async Task<OperationResult<PaginatedResult<TaskDto>>> Handle(
              GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var context = request.Context;
            var filter = request.Filter ?? new TaskFilterDto();

            try
            {
                _logger.LogInformation("Getting all tasks for user {UserId}", context.UserId);

                var query = _unitOfWork.TaskItems.Query()
                    .Where(t => !t.IsDeleted);

                // Security filtering for non-super admins
                if (!context.IsSuperAdmin && filter.MyTasks != true)
                {
                    query = query.Where(t => t.AssignedToUserId == context.UserId ||
                                            t.CreatedByUserId == context.UserId);
                }

                // My Tasks filter
                if (filter.MyTasks == true)
                {
                    query = query.Where(t => t.AssignedToUserId == context.UserId);
                }

                // Apply filters
                query = ApplyFilters(query, filter);

                // Get total count before pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting
                query = ApplySorting(query, filter);

                // Apply pagination
                query = query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize);

                // Include related data
                query = ApplyIncludes(query);

                var tasks = await query.ToListAsync(cancellationToken);
                var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

                var paginatedResult = new PaginatedResult<TaskDto>(
                   taskDtos,
                   totalCount,
                   filter.Page,
                   filter.PageSize
               );

                _logger.LogInformation("Retrieved {Count} of {TotalCount} tasks for user {UserId}",
                 taskDtos.Count(), totalCount, context.UserId);

                return OperationResult<PaginatedResult<TaskDto>>.SuccessResult(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated tasks for user {UserId}", context.UserId);
                return OperationResult<PaginatedResult<TaskDto>>.FailureResult("Failed to retrieve tasks.");
            }
        }

        /// <summary>
        /// Applies common includes for task details
        /// </summary>
        private IQueryable<TaskItem> ApplyIncludes(IQueryable<TaskItem> query)
        {
            return query
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.ParentTask)
                .Include(t => t.Subtasks.Where(s => !s.IsDeleted))
                .Include(t => t.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.User)
                .Include(t => t.Attachments.Where(a => !a.IsDeleted));
        }

        /// <summary>
        /// Applies filtering logic (shared between GetAll and GetMyTasks)
        /// </summary>
        private IQueryable<TaskItem> ApplyFilters(IQueryable<TaskItem> query, TaskFilterDto? filter)
        {
            if (filter == null) return query;

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);

            if (!string.IsNullOrEmpty(filter.AssignedToUserId))
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);

            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(t => t.Category == filter.Category);

            if (filter.DueDateFrom.HasValue)
                query = query.Where(t => t.DueDate >= filter.DueDateFrom);

            if (filter.DueDateTo.HasValue)
                query = query.Where(t => t.DueDate <= filter.DueDateTo);

            if (filter.Overdue == true)
                query = query.Where(t => t.DueDate.HasValue
                    && t.DueDate.Value.Date < DateTime.UtcNow.Date
                    && t.Status != Domain.Enums.TaskStatus.Completed);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(term) ||
                                       (t.Description != null && t.Description.ToLower().Contains(term)));
            }

            return query;
        }

        /// <summary>
        /// Applies sorting logic
        /// </summary>
        private IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, TaskFilterDto? filter)
        {
            if (filter == null)
                return query.OrderByDescending(t => t.DueDate);

            return filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortDescending
                    ? query.OrderByDescending(t => t.Title)
                    : query.OrderBy(t => t.Title),

                "status" => filter.SortDescending
                    ? query.OrderByDescending(t => t.Status)
                    : query.OrderBy(t => t.Status),

                "priority" => filter.SortDescending
                    ? query.OrderByDescending(t => t.Priority)
                    : query.OrderBy(t => t.Priority),

                _ => filter.SortDescending
                    ? query.OrderByDescending(t => t.DueDate)
                    : query.OrderBy(t => t.DueDate)
            };
        }

        /// <summary>
        /// Applies pagination
        /// </summary>
        private IQueryable<TaskItem> ApplyPagination(IQueryable<TaskItem> query, TaskFilterDto? filter)
        {
            if (filter == null) return query;

            return query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }
    }
}