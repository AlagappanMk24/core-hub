using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoiceAuditLogRepository(CoreInvoiceDbContext dbContext) : GenericRepository<InvoiceAuditLog>(dbContext), IInvoiceAuditLogRepository
    {
        public async Task<List<InvoiceAuditLog>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await dbset
                .Where(l => l.InvoiceId == invoiceId && !l.IsDeleted)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }
    }
}