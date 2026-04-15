// Core_API.Infrastructure/Repositories/TaskRepository.cs
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class TaskRepository(CoreInvoiceDbContext dbContext) : GenericRepository<TaskItem>(dbContext), ITaskRepository
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext;
        public async Task<IEnumerable<TaskItem>> GetTasksByUserAsync(string userId, bool includeSubtasks = false)
        {
            var query = _dbContext.TaskItems
                .Where(t => (t.AssignedToUserId == userId || t.CreatedByUserId == userId) && !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser);

            if (includeSubtasks)
            {
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<TaskItem, Domain.Entities.Identity.ApplicationUser?>)query.Include(t => t.Subtasks.Where(s => !s.IsDeleted));
            }

            return await query.OrderByDescending(t => t.DueDate).ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string userId, bool includeSubtasks = false)
        {
            var query = _dbContext.TaskItems
                .Where(t => t.AssignedToUserId == userId && !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser);

            if (includeSubtasks)
            {
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<TaskItem, Domain.Entities.Identity.ApplicationUser?>)query.Include(t => t.Subtasks.Where(s => !s.IsDeleted));
            }

            return await query.OrderByDescending(t => t.DueDate).ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByCreatorAsync(string userId, bool includeSubtasks = false)
        {
            var query = _dbContext.TaskItems
                .Where(t => t.CreatedByUserId == userId && !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser);

            if (includeSubtasks)
            {
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<TaskItem, Domain.Entities.Identity.ApplicationUser?>)query.Include(t => t.Subtasks.Where(s => !s.IsDeleted));
            }

            return await query.OrderByDescending(t => t.DueDate).ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbContext.TaskItems
                .Where(t => t.Status != TaskStatus.Completed &&
                           t.Status != TaskStatus.Cancelled &&
                           t.DueDate.HasValue &&
                           t.DueDate.Value.Date < today &&
                           !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksDueSoonAsync(int days = 3)
        {
            var today = DateTime.UtcNow.Date;
            var dueDate = today.AddDays(days);

            return await _dbContext.TaskItems
                .Where(t => t.Status != TaskStatus.Completed &&
                           t.Status != TaskStatus.Cancelled &&
                           t.DueDate.HasValue &&
                           t.DueDate.Value.Date >= today &&
                           t.DueDate.Value.Date <= dueDate &&
                           !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksDueTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbContext.TaskItems
                .Where(t => t.Status != TaskStatus.Completed &&
                           t.Status != TaskStatus.Cancelled &&
                           t.DueDate.HasValue &&
                           t.DueDate.Value.Date == today &&
                           !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksDueThisWeekAsync()
        {
            var today = DateTime.UtcNow.Date;
            var endOfWeek = today.AddDays(7 - (int)today.DayOfWeek);

            return await _dbContext.TaskItems
                .Where(t => t.Status != TaskStatus.Completed &&
                           t.Status != TaskStatus.Cancelled &&
                           t.DueDate.HasValue &&
                           t.DueDate.Value.Date >= today &&
                           t.DueDate.Value.Date <= endOfWeek &&
                           !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetTaskWithDetailsAsync(int taskId)
        {
            return await _dbContext.TaskItems
                .Where(t => t.Id == taskId && !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.ParentTask)
                .Include(t => t.Subtasks.Where(s => !s.IsDeleted))
                    .ThenInclude(s => s.AssignedToUser)
                .Include(t => t.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.User)
                .Include(t => t.Attachments.Where(a => !a.IsDeleted))
                .Include(t => t.AuditLogs.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.PerformedByUser)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetSubtasksAsync(int parentTaskId)
        {
            return await _dbContext.TaskItems
                .Where(t => t.ParentTaskId == parentTaskId && !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<bool> HasSubtasksAsync(int taskId)
        {
            return await _dbContext.TaskItems
                .AnyAsync(t => t.ParentTaskId == taskId && !t.IsDeleted);
        }

        public async Task<int> GetTaskCountByStatusAsync(TaskStatus status, string? userId = null)
        {
            var query = _dbContext.TaskItems.Where(t => t.Status == status && !t.IsDeleted);

            // FIX: Check if string is not null or empty
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(t => t.AssignedToUserId == userId || t.CreatedByUserId == userId);
            }

            return await query.CountAsync();
        }

        public async Task<int> GetTaskCountByPriorityAsync(TaskPriority priority, string? userId = null)
        {
            var query = _dbContext.TaskItems.Where(t => t.Priority == priority && !t.IsDeleted);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(t => t.AssignedToUserId == userId || t.CreatedByUserId == userId);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetRecurringTasksAsync()
        {
            return await _dbContext.TaskItems
                .Where(t => t.IsRecurring && !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate, string? userId = null)
        {
            var query = _dbContext.TaskItems
                .Where(t => t.DueDate.HasValue &&
                           t.DueDate.Value.Date >= startDate.Date &&
                           t.DueDate.Value.Date <= endDate.Date &&
                           !t.IsDeleted)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser);

            if (!string.IsNullOrEmpty(userId))
            {
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<TaskItem, Domain.Entities.Identity.ApplicationUser?>)query.Where(t => t.AssignedToUserId == userId || t.CreatedByUserId == userId);
            }

            return await query.OrderBy(t => t.DueDate).ToListAsync();
        }
    }
}