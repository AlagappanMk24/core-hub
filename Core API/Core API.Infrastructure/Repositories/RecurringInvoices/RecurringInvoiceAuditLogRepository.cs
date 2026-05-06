using Core_API.Application.Contracts.Persistence.RecurringInvoice;
using Core_API.Domain.Entities.RecurringInvoices;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Repositories.RecurringInvoice
{
    public class RecurringInvoiceAuditLogRepository(
        CoreInvoiceDbContext dbContext,
        ILogger<RecurringInvoiceAuditLogRepository> logger) : GenericRepository<RecurringInvoiceAuditLog>(dbContext), IRecurringInvoiceAuditLogRepository
    {
        private readonly ILogger<RecurringInvoiceAuditLogRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<List<RecurringInvoiceAuditLog>> GetByRecurringInvoiceIdAsync(int recurringInvoiceId)
        {
            try
            {
                return await _dbSet
                    .Where(al => al.RecurringInvoiceId == recurringInvoiceId && !al.IsDeleted)
                    .OrderByDescending(al => al.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for recurring invoice {RecurringInvoiceId}", recurringInvoiceId);
                throw;
            }
        }
    }
}