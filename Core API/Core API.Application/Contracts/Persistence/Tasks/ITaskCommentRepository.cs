using Core_API.Domain.Entities.Tasks;

namespace Core_API.Application.Contracts.Persistence.Tasks
{
    public interface ITaskCommentRepository : IGenericRepository<TaskComment>
    {
        Task<IEnumerable<TaskComment>> GetCommentsByTaskAsync(int taskId);
        Task<IEnumerable<TaskComment>> GetCommentsByUserAsync(string userId);
        Task<int> GetCommentCountByTaskAsync(int taskId);
    }
}