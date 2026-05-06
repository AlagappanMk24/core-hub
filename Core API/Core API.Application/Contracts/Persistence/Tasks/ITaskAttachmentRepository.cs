using Core_API.Domain.Entities.Tasks;

namespace Core_API.Application.Contracts.Persistence.Tasks
{
    public interface ITaskAttachmentRepository : IGenericRepository<TaskAttachment>
    {
        Task<IEnumerable<TaskAttachment>> GetAttachmentsByTaskAsync(int taskId);
        Task<TaskAttachment?> GetAttachmentByFileIdAsync(int attachmentId);
        Task<int> GetAttachmentCountByTaskAsync(int taskId);
        Task<IEnumerable<TaskAttachment>> GetAttachmentsByFileTypeAsync(int taskId, string contentType);
        Task<bool> DeleteAttachmentAsync(int attachmentId);
        Task<bool> DeleteAttachmentsByTaskAsync(int taskId);
        Task<long> GetTotalAttachmentsSizeByTaskAsync(int taskId);
    }
}