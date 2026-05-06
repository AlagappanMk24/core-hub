using Core_API.Application.Contracts.Persistence.Tasks;
using Core_API.Domain.Entities.Tasks;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Tasks
{
    public class TaskAuditLogRepository(CoreInvoiceDbContext dbContext) : GenericRepository<TaskAuditLog>(dbContext), ITaskAuditLogRepository
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext;
        public async Task<IEnumerable<TaskAuditLog>> GetAuditLogsByTaskAsync(int taskId)
        {
            return await _dbContext.TaskAuditLogs
                .Where(l => l.TaskId == taskId && !l.IsDeleted)
                .Include(l => l.PerformedByUser)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<TaskAuditLog>> GetAuditLogsByUserAsync(string userId)
        {
            return await _dbContext.TaskAuditLogs
                .Where(l => l.PerformedByUserId == userId && !l.IsDeleted)
                .Include(l => l.Task)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<TaskAuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.TaskAuditLogs
                .Where(l => l.CreatedDate >= startDate && l.CreatedDate <= endDate && !l.IsDeleted)
                .Include(l => l.Task)
                .Include(l => l.PerformedByUser)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<TaskAuditLog>> GetAuditLogsByActionAsync(string action)
        {
            return await _dbContext.TaskAuditLogs
                .Where(l => l.Action == action && !l.IsDeleted)
                .Include(l => l.Task)
                .Include(l => l.PerformedByUser)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }
        public async Task<int> GetAuditLogCountByTaskAsync(int taskId)
        {
            return await _dbContext.TaskAuditLogs
                .CountAsync(l => l.TaskId == taskId && !l.IsDeleted);
        }
        public async Task<IEnumerable<TaskAuditLog>> GetRecentAuditLogsAsync(int limit = 50)
        {
            return await _dbContext.TaskAuditLogs
                .Where(l => !l.IsDeleted)
                .Include(l => l.Task)
                .Include(l => l.PerformedByUser)
                .OrderByDescending(l => l.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }
        public async Task<bool> CleanupOldAuditLogsAsync(int daysToKeep = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldLogs = await _dbContext.TaskAuditLogs
                .Where(l => l.CreatedDate < cutoffDate && !l.IsDeleted)
                .ToListAsync();

            if (!oldLogs.Any())
                return true;

            foreach (var log in oldLogs)
            {
                log.IsDeleted = true;
                log.UpdatedDate = DateTime.UtcNow;
            }

            _dbContext.TaskAuditLogs.UpdateRange(oldLogs);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}