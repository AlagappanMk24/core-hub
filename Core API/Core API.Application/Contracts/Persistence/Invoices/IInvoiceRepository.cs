using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.Features.Invoices.Queries.GetInvoicesPaged;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceRepository : IGenericRepository<Domain.Entities.Invoices.Invoice>
    {
        Task<PaginatedResult<Domain.Entities.Invoices.Invoice>> GetPagedAsync(
         int? companyId, GetPagedInvoicesQuery filter, CancellationToken cancellationToken = default);
        Task<bool> InvoiceNumberExistsAsync(int companyId, string invoiceNumber, int? excludeInvoiceId = null);
    }
}