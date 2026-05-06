using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoicePaymentRepository : IGenericRepository<InvoicePayment>
    {
        Task<List<InvoicePayment>> GetByInvoiceIdAsync(int invoiceId);
        Task<decimal> GetTotalPaidByInvoiceIdAsync(int invoiceId);
    }
}