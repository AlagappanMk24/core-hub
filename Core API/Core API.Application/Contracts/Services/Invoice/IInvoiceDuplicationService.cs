using Core_API.Application.Common.Models;
using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Services.Invoice
{
    /// <summary>
    /// Service for duplicating invoices
    /// </summary>
    public interface IInvoiceDuplicationService
    {
        /// <summary>
        /// Creates a deep copy of an invoice with all related data
        /// </summary>
        Task<Domain.Entities.Invoice> DuplicateInvoiceAsync(Domain.Entities.Invoice originalInvoice, OperationContext operationContext);

        /// <summary>
        /// Copies line items from original invoice to duplicate
        /// </summary>
        void CopyInvoiceItems(Domain.Entities.Invoice original, Domain.Entities.Invoice duplicate, OperationContext operationContext);

        /// <summary>
        /// Copies tax details from original invoice to duplicate
        /// </summary>
        void CopyTaxDetails(Domain.Entities.Invoice original, Domain.Entities.Invoice duplicate, OperationContext operationContext);

        /// <summary>
        /// Copies discounts from original invoice to duplicate
        /// </summary>
        void CopyDiscounts(Domain.Entities.Invoice original, Domain.Entities.Invoice duplicate, OperationContext operationContext);

        /// <summary>
        /// Copies attachments from original invoice to duplicate
        /// </summary>
        Task<List<InvoiceAttachment>> CopyAttachmentsAsync(List<InvoiceAttachment> originalAttachments, int companyId, string userId);
    }
}
