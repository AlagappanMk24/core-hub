// Core_API.Infrastructure/Repositories/TaskCommentRepository.cs
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class TaskCommentRepository(CoreInvoiceDbContext dbContext) : GenericRepository<TaskComment>(dbContext), ITaskCommentRepository
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext;
        public async Task<IEnumerable<TaskComment>> GetCommentsByTaskAsync(int taskId)
        {
            return await _dbContext.TaskComments
                .Where(c => c.TaskId == taskId && !c.IsDeleted)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<TaskComment>> GetCommentsByUserAsync(string userId)
        {
            return await _dbContext.TaskComments
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Include(c => c.Task)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }
        public async Task<int> GetCommentCountByTaskAsync(int taskId)
        {
            return await _dbContext.TaskComments
                .CountAsync(c => c.TaskId == taskId && !c.IsDeleted);
        }
    }
}