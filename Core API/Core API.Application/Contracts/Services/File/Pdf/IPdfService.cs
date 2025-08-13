using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;

namespace Core_API.Application.Contracts.Services.File.Pdf
{
    public interface IPdfService
    {
        Task<OperationResult<InvoiceResponseDto>> GenerateInvoicePdfAsync(int id, OperationContext operationContext);
        Task<OperationResult<byte[]>> ExportInvoicesPdfAsync(OperationContext operationContext, int pageNumber, int pageSize, string search = null, string status = null);
    }
}
