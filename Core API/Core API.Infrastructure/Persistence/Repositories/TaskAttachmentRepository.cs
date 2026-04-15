using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class TaskAttachmentRepository(CoreInvoiceDbContext dbContext) : GenericRepository<TaskAttachment>(dbContext), ITaskAttachmentRepository
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext;
        public async Task<IEnumerable<TaskAttachment>> GetAttachmentsByTaskAsync(int taskId)
        {
            return await _dbContext.TaskAttachments
                .Where(a => a.TaskId == taskId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }
        public async Task<TaskAttachment?> GetAttachmentByFileIdAsync(int attachmentId)
        {
            return await _dbContext.TaskAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted);
        }
        public async Task<int> GetAttachmentCountByTaskAsync(int taskId)
        {
            return await _dbContext.TaskAttachments
                .CountAsync(a => a.TaskId == taskId && !a.IsDeleted);
        }
        public async Task<IEnumerable<TaskAttachment>> GetAttachmentsByFileTypeAsync(int taskId, string contentType)
        {
            return await _dbContext.TaskAttachments
                .Where(a => a.TaskId == taskId && a.ContentType == contentType && !a.IsDeleted)
                .ToListAsync();
        }
        public async Task<bool> DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _dbContext.TaskAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted);

            if (attachment == null)
                return false;

            attachment.IsDeleted = true;
            attachment.UpdatedDate = DateTime.UtcNow;
            _dbContext.TaskAttachments.Update(attachment);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteAttachmentsByTaskAsync(int taskId)
        {
            var attachments = await _dbContext.TaskAttachments
                .Where(a => a.TaskId == taskId && !a.IsDeleted)
                .ToListAsync();

            if (!attachments.Any())
                return true;

            foreach (var attachment in attachments)
            {
                attachment.IsDeleted = true;
                attachment.UpdatedDate = DateTime.UtcNow;
            }

            _dbContext.TaskAttachments.UpdateRange(attachments);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        public async Task<long> GetTotalAttachmentsSizeByTaskAsync(int taskId)
        {
            return await _dbContext.TaskAttachments
                .Where(a => a.TaskId == taskId && !a.IsDeleted)
                .SumAsync(a => a.FileSize);
        }
    }
}