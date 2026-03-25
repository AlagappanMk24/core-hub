using Core_API.Application.Contracts.Persistence.RecurringInvoice;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Persistence.Repositories.RecurringInvoice
{
    public class RecurringInvoiceInstanceRepository(
        CoreAPIDbContext dbContext,
        ILogger<RecurringInvoiceInstanceRepository> logger) : GenericRepository<RecurringInvoiceInstance>(dbContext), IRecurringInvoiceInstanceRepository
    {
        private readonly ILogger<RecurringInvoiceInstanceRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<List<RecurringInvoiceInstance>> GetByRecurringInvoiceIdAsync(int recurringInvoiceId, int companyId)
        {
            try
            {
                return await dbset
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
                return await dbset
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
                return await dbset.AnyAsync(i => i.InvoiceId == invoiceId && !i.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if invoice {InvoiceId} is from recurring", invoiceId);
                throw;
            }
        }
    }
}