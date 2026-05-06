using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities.Invoices;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Invoice
{
    public class InvoiceItemRepository(CoreInvoiceDbContext dbContext) : GenericRepository<InvoiceItem>(dbContext), IInvoiceItemRepository
    {
        public async Task<List<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _dbSet
                .Where(i => i.InvoiceId == invoiceId && !i.IsDeleted)
                .ToListAsync();
        }
        public void DeleteRange(IEnumerable<InvoiceItem> items)
        {
            _dbSet.RemoveRange(items);
        }
    }
}