using Core_API.Domain.Entities.RecurringInvoices;

namespace Core_API.Application.Contracts.Persistence.RecurringInvoice
{
    public interface IRecurringInvoiceInstanceRepository : IGenericRepository<RecurringInvoiceInstance>
    {
        Task<List<RecurringInvoiceInstance>> GetByRecurringInvoiceIdAsync(int recurringInvoiceId, int companyId);
        Task<RecurringInvoiceInstance> GetByInvoiceIdAsync(int invoiceId, int companyId);
        Task<bool> IsInvoiceFromRecurringAsync(int invoiceId);
    }
}