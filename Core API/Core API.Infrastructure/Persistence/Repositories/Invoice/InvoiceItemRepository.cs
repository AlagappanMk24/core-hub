using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoiceItemRepository(CoreAPIDbContext dbContext) : GenericRepository<InvoiceItem>(dbContext), IInvoiceItemRepository
    {
        public async Task<List<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await dbset
                .Where(i => i.InvoiceId == invoiceId && !i.IsDeleted)
                .ToListAsync();
        }
        public void DeleteRange(IEnumerable<InvoiceItem> items)
        {
            dbset.RemoveRange(items);
        }
    }
}