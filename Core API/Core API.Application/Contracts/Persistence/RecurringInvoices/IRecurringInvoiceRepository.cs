using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.RecurringInvoice.Request;

namespace Core_API.Application.Contracts.Persistence.RecurringInvoice
{
    public interface IRecurringInvoiceRepository : IGenericRepository<Domain.Entities.RecurringInvoices.RecurringInvoice>
    {
        Task<PaginatedResult<Domain.Entities.RecurringInvoices.RecurringInvoice>> GetPagedAsync(int companyId, RecurringInvoiceFilterDto filter);
        Task<List<Domain.Entities.RecurringInvoices.RecurringInvoice>> GetDueInvoicesAsync(DateTime asOfDate);
        Task<Domain.Entities.RecurringInvoices.RecurringInvoice> GetWithDetailsAsync(int id, int companyId);
        Task<bool> ExistsAsync(int companyId, string name, int? excludeId = null);
        Task<int> GetNextSequenceNumberAsync(int recurringInvoiceId);
        Task<List<Domain.Entities.RecurringInvoices.RecurringInvoice>> GetByCustomerAsync(int customerId, int companyId);
        Task<Dictionary<string, int>> GetStatusCountsAsync(int companyId);
    }
}