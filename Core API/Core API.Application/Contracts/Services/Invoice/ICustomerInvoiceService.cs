using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;

namespace Core_API.Application.Contracts.Services.Invoice
{
    public interface ICustomerInvoiceService
    {
        Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetCustomerInvoicesAsync(OperationContext context, int pageNumber, int pageSize, string status);
        Task<OperationResult<InvoiceResponseDto>> GetCustomerInvoiceByIdAsync(int id, OperationContext context);
    }
}