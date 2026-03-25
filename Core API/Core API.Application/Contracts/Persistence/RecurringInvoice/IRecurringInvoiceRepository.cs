using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.RecurringInvoice.Request;

namespace Core_API.Application.Contracts.Persistence.RecurringInvoice
{
    public interface IRecurringInvoiceRepository : IGenericRepository<Core_API.Domain.Entities.RecurringInvoice>
    {
        Task<PaginatedResult<Core_API.Domain.Entities.RecurringInvoice>> GetPagedAsync(int companyId, RecurringInvoiceFilterDto filter);
        Task<List<Core_API.Domain.Entities.RecurringInvoice>> GetDueInvoicesAsync(DateTime asOfDate);
        Task<Core_API.Domain.Entities.RecurringInvoice> GetWithDetailsAsync(int id, int companyId);
        Task<bool> ExistsAsync(int companyId, string name, int? excludeId = null);
        Task<int> GetNextSequenceNumberAsync(int recurringInvoiceId);
        Task<List<Core_API.Domain.Entities.RecurringInvoice>> GetByCustomerAsync(int customerId, int companyId);
        Task<Dictionary<string, int>> GetStatusCountsAsync(int companyId);
    }
}