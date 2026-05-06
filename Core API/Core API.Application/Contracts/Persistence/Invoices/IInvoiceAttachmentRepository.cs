using Core_API.Domain.Entities.Invoices;
using System.Linq.Expressions;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceAttachmentRepository
    {
        Task<InvoiceAttachment> GetAsync(Expression<Func<InvoiceAttachment, bool>> predicate);
        void Update(InvoiceAttachment attachment);
    }
}