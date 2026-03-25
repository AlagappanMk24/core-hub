using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceItemRepository : IGenericRepository<InvoiceItem>
    {
        Task<List<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId);
        void DeleteRange(IEnumerable<InvoiceItem> items);
    }
}