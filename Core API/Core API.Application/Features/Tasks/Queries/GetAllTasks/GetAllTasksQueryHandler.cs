using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Application.Features.Tasks.Queries.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQueryHandler(
         IUnitOfWork unitOfWork,
         IMapper mapper,
         ILogger<GetAllTasksQueryHandler> logger)
     : BaseTaskQueryHandler<GetAllTasksQuery>(unitOfWork, mapper, logger)
    { 
        public override async Task<OperationResult<IEnumerable<TaskDto>>> Handle(
            GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                _logger.LogInformation("Getting all tasks for user {UserId}", context.UserId);

                var query = _unitOfWork.TaskItems.Query()
                    .Where(t => !t.IsDeleted);

                // Security filtering for non-super admins
                if (!context.IsSuperAdmin)
                {
                    query = query.Where(t => t.AssignedToUserId == context.UserId ||
                                           t.CreatedByUserId == context.UserId);
                }

                query = ApplyFilters(query, request.Filter);
                query = ApplySorting(query, request.Filter);
                query = ApplyPagination(query, request.Filter);
                query = ApplyIncludes(query);

                var tasks = await query.ToListAsync(cancellationToken);
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

                _logger.LogInformation("Retrieved {Count} tasks for user {UserId}", taskDtos.Count(), context.UserId);

                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tasks for user {UserId}", context.UserId);
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve tasks.");
            }
        }
    }
}