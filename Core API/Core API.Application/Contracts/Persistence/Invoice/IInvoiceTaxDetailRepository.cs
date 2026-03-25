using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceTaxDetailRepository : IGenericRepository<InvoiceTaxDetail>
    {
        Task<List<InvoiceTaxDetail>> GetByInvoiceIdAsync(int invoiceId);
        void DeleteRange(IEnumerable<InvoiceTaxDetail> taxDetails);
    }
}