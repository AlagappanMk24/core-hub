using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities.Invoices;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Invoice
{
    public class InvoiceAuditLogRepository(CoreInvoiceDbContext dbContext) : GenericRepository<InvoiceAuditLog>(dbContext), IInvoiceAuditLogRepository
    {
        public async Task<List<InvoiceAuditLog>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _dbSet
                .Where(l => l.InvoiceId == invoiceId && !l.IsDeleted)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }
    }
}