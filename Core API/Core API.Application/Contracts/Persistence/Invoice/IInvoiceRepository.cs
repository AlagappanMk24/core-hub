using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceRepository : IGenericRepository<Core_API.Domain.Entities.Invoice>
    {
        Task<PaginatedResult<Core_API.Domain.Entities.Invoice>> GetPagedAsync(
         int? companyId,
          InvoiceFilterRequestDto filter);
        Task<bool> InvoiceNumberExistsAsync(int companyId, string invoiceNumber, int? excludeInvoiceId = null);
    }
}