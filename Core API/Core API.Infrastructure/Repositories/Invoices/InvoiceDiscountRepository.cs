using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities.Invoices;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Invoice
{
    public class InvoiceDiscountRepository(CoreInvoiceDbContext dbContext) : GenericRepository<InvoiceDiscount>(dbContext), IInvoiceDiscountRepository
    {
        public async Task<List<InvoiceDiscount>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _dbSet
                .Where(d => d.InvoiceId == invoiceId && !d.IsDeleted)
                .ToListAsync();
        }
        public void DeleteRange(IEnumerable<InvoiceDiscount> discounts)
        {
            _dbSet.RemoveRange(discounts);
        }
    }
}