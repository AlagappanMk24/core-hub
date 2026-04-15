using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Request;

namespace Core_API.Application.Contracts.Services.Invoice
{
    public interface IInvoiceSettingsService
    {
        Task<OperationResult<InvoiceSettingsDto>> GetInvoiceSettingsAsync(OperationContext operationContext, int? companyId = null);
        Task<OperationResult<bool>> SaveInvoiceSettingsAsync(InvoiceSettingsDto dto, OperationContext operationContext);
    }
}