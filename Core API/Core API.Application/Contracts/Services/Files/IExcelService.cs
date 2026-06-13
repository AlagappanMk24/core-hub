using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Request;

namespace Core_API.Application.Contracts.Services.Files
{
    public interface IExcelService
    {
        //Task<OperationResult<byte[]>> ExportInvoicesExcelAsync(OperationContext operationContext, InvoiceFilterRequestDto invoiceFilterRequestDto);
        byte[] GenerateImportTemplate();
    }
}
