using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface ITaskAuditLogRepository : IGenericRepository<TaskAuditLog>
    {
        Task<IEnumerable<TaskAuditLog>> GetAuditLogsByTaskAsync(int taskId);
        Task<IEnumerable<TaskAuditLog>> GetAuditLogsByUserAsync(string userId);
        Task<IEnumerable<TaskAuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TaskAuditLog>> GetAuditLogsByActionAsync(string action);
        Task<int> GetAuditLogCountByTaskAsync(int taskId);
        Task<IEnumerable<TaskAuditLog>> GetRecentAuditLogsAsync(int limit = 50);
        Task<bool> CleanupOldAuditLogsAsync(int daysToKeep = 90);
    }
}