using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceAuditLogRepository : IGenericRepository<InvoiceAuditLog>
    {
        Task<List<InvoiceAuditLog>> GetByInvoiceIdAsync(int invoiceId);
    }
}