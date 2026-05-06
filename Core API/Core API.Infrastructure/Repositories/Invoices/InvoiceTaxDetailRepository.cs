using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities.Invoices;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Invoice
{
    public class InvoiceTaxDetailRepository(CoreInvoiceDbContext dbContext) : GenericRepository<InvoiceTaxDetail>(dbContext), IInvoiceTaxDetailRepository
    {
        public async Task<List<InvoiceTaxDetail>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _dbSet
                .Where(t => t.InvoiceId == invoiceId && !t.IsDeleted)
                .ToListAsync();
        }
        public void DeleteRange(IEnumerable<InvoiceTaxDetail> taxDetails)
        {
            _dbSet.RemoveRange(taxDetails);
        }
    }
}