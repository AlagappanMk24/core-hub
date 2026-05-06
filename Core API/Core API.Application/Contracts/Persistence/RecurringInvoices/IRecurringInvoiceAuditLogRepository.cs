using Core_API.Domain.Entities.RecurringInvoices;

namespace Core_API.Application.Contracts.Persistence.RecurringInvoice
{
    public interface IRecurringInvoiceAuditLogRepository : IGenericRepository<RecurringInvoiceAuditLog>
    {
        Task<List<RecurringInvoiceAuditLog>> GetByRecurringInvoiceIdAsync(int recurringInvoiceId);
    }
}