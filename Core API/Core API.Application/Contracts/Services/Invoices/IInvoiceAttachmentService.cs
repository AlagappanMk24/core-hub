using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Contracts.Services.Invoice
{
    public interface IInvoiceAttachmentService
    {
        Task<List<InvoiceAttachment>> HandleAttachmentsAsync(List<InvoiceAttachmentDto> attachmentDtos, int companyId, int invoiceId, string userId);
        Task<OperationResult<bool>> DeleteAttachmentAsync(int invoiceId, int attachmentId, OperationContext operationContext);
        List<InvoiceAttachment> CopyAttachments(List<InvoiceAttachment> originalAttachments, int companyId, string userId);

    }
}
