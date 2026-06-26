using AutoMapper;
using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.Base
{
    /// <summary>
    /// Base handler for task queries to eliminate code duplication.
    /// Handles common filtering, sorting, pagination, and includes.
    /// </summary>
    public abstract class BaseTaskQueryHandler<TQuery>(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger logger) : IRequestHandler<TQuery, OperationResult<IEnumerable<TaskDto>>>
        where TQuery : BaseQuery<IEnumerable<TaskDto>>
    {
        protected readonly IUnitOfWork _unitOfWork = unitOfWork;
        protected readonly IMapper _mapper = mapper;
        protected readonly ILogger _logger = logger;

        public abstract Task<OperationResult<IEnumerable<TaskDto>>> Handle(TQuery request, CancellationToken cancellationToken);

        /// <summary>
        /// Applies common includes for task details
        /// </summary>
        protected IQueryable<TaskItem> ApplyIncludes(IQueryable<TaskItem> query)
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
        protected IQueryable<TaskItem> ApplyFilters(IQueryable<TaskItem> query, TaskFilterDto? filter)
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
        protected IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, TaskFilterDto? filter)
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
        protected IQueryable<TaskItem> ApplyPagination(IQueryable<TaskItem> query, TaskFilterDto? filter)
        {
            if (filter == null) return query;

            return query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }
    }
}