using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoiceDiscountRepository(CoreAPIDbContext dbContext) : GenericRepository<InvoiceDiscount>(dbContext), IInvoiceDiscountRepository
    {
        public async Task<List<InvoiceDiscount>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await dbset
                .Where(d => d.InvoiceId == invoiceId && !d.IsDeleted)
                .ToListAsync();
        }
        public void DeleteRange(IEnumerable<InvoiceDiscount> discounts)
        {
            dbset.RemoveRange(discounts);
        }
    }
}