using Core_API.Application.Common.Results;
using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<PaginatedResult<Invoice>> GetPagedAsync(
              int companyId,
              int pageNumber,
              int pageSize,
              string? search = null,
              string? invoiceStatus = null,
              string? paymentStatus = null,
              int? customerId = null,
              int? taxType = null,
              decimal? minAmount = null,
              decimal? maxAmount = null,
              string? invoiceNumberFrom = null,
              string? invoiceNumberTo = null,
              DateTime? issueDateFrom = null,
              DateTime? issueDateTo = null,
              DateTime? dueDateFrom = null,
              DateTime? dueDateTo = null);
        Task<bool> InvoiceNumberExistsAsync(int companyId, string invoiceNumber, int? excludeInvoiceId = null);
    }
}