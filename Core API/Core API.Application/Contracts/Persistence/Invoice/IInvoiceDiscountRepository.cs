using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceDiscountRepository : IGenericRepository<InvoiceDiscount>
    {
        Task<List<InvoiceDiscount>> GetByInvoiceIdAsync(int invoiceId);
        void DeleteRange(IEnumerable<InvoiceDiscount> discounts);
    }
}