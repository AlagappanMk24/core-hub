using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoiceTaxDetailRepository(CoreInvoiceDbContext dbContext) : GenericRepository<InvoiceTaxDetail>(dbContext), IInvoiceTaxDetailRepository
    {
        public async Task<List<InvoiceTaxDetail>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await dbset
                .Where(t => t.InvoiceId == invoiceId && !t.IsDeleted)
                .ToListAsync();
        }
        public void DeleteRange(IEnumerable<InvoiceTaxDetail> taxDetails)
        {
            dbset.RemoveRange(taxDetails);
        }
    }
}