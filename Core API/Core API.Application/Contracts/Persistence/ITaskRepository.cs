using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Application.Contracts.Persistence
{
    public interface ITaskRepository : IGenericRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetTasksByUserAsync(string userId, bool includeSubtasks = false);
        Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string userId, bool includeSubtasks = false);
        Task<IEnumerable<TaskItem>> GetTasksByCreatorAsync(string userId, bool includeSubtasks = false);
        Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();
        Task<IEnumerable<TaskItem>> GetTasksDueSoonAsync(int days = 3);
        Task<IEnumerable<TaskItem>> GetTasksDueTodayAsync();
        Task<IEnumerable<TaskItem>> GetTasksDueThisWeekAsync();
        Task<TaskItem?> GetTaskWithDetailsAsync(int taskId);
        Task<IEnumerable<TaskItem>> GetSubtasksAsync(int parentTaskId);
        Task<bool> HasSubtasksAsync(int taskId);
        Task<int> GetTaskCountByStatusAsync(TaskStatus status, string? userId = null);
        Task<int> GetTaskCountByPriorityAsync(TaskPriority priority, string? userId = null);
        Task<IEnumerable<TaskItem>> GetRecurringTasksAsync();
        Task<IEnumerable<TaskItem>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate, string? userId = null);
    }
}
