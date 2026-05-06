using Core_API.Application.Contracts.Persistence.RecurringInvoice;
using Core_API.Domain.Entities.RecurringInvoices;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Repositories.RecurringInvoice
{
    public class RecurringInvoiceInstanceRepository(
        CoreInvoiceDbContext dbContext,
        ILogger<RecurringInvoiceInstanceRepository> logger) : GenericRepository<RecurringInvoiceInstance>(dbContext), IRecurringInvoiceInstanceRepository
    {
        private readonly ILogger<RecurringInvoiceInstanceRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<List<RecurringInvoiceInstance>> GetByRecurringInvoiceIdAsync(int recurringInvoiceId, int companyId)
        {
            try
            {
                return await _dbSet
                    .Where(i => i.RecurringInvoiceId == recurringInvoiceId &&
                               i.RecurringInvoice.CompanyId == companyId &&
                               !i.IsDeleted)
                    .Include(i => i.Invoice)
                    .OrderByDescending(i => i.GeneratedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instances for recurring invoice {RecurringInvoiceId}", recurringInvoiceId);
                throw;
            }
        }

        public async Task<RecurringInvoiceInstance> GetByInvoiceIdAsync(int invoiceId, int companyId)
        {
            try
            {
                return await _dbSet
                    .Include(i => i.RecurringInvoice)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId &&
                                              i.RecurringInvoice.CompanyId == companyId &&
                                              !i.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recurring instance for invoice {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<bool> IsInvoiceFromRecurringAsync(int invoiceId)
        {
            try
            {
                return await _dbSet.AnyAsync(i => i.InvoiceId == invoiceId && !i.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if invoice {InvoiceId} is from recurring", invoiceId);
                throw;
            }
        }
    }
}