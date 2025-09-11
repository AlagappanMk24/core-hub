using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.EmailDto;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;

namespace Core_API.Application.Contracts.Services
{
    public interface IInvoiceService
    {
        Task<OperationResult<InvoiceResponseDto>> CreateAsync(InvoiceCreateDto dto, OperationContext operationContext);
        Task<OperationResult<InvoiceResponseDto>> UpdateAsync(InvoiceUpdateDto dto, OperationContext operationContext);
        Task<OperationResult<bool>> DeleteAsync(int id, OperationContext operationContext);
        Task<OperationResult<InvoiceResponseDto>> GetByIdAsync(int id, OperationContext operationContext);
        //Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetPagedAsync(
        //     OperationContext operationContext,
        //     int pageNumber,
        //     int pageSize,
        //     string? search = null,
        //     string? invoiceStatus = null,
        //     string? paymentStatus = null,
        //     int? customerId = null,
        //     int? taxType = null,
        //     decimal? minAmount = null,
        //     decimal? maxAmount = null,
        //     string? invoiceNumberFrom = null,
        //     string? invoiceNumberTo = null,
        //     DateTime? issueDateFrom = null,
        //     DateTime? issueDateTo = null,
        //     DateTime? dueDateFrom = null,
        //     DateTime? dueDateTo = null);
        Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetPagedAsync(
          OperationContext operationContext,
          InvoiceFilterRequestDto filter);
        Task<OperationResult<InvoiceSettingsDto>> GetInvoiceSettingsAsync(OperationContext operationContext);
        Task<OperationResult<bool>> SendInvoiceAsync(int id, OperationContext operationContext, EmailDataDto emailData);
        Task<OperationResult<InvoiceStatsDto>> GetStatsAsync(OperationContext operationContext);
        Task<OperationResult<bool>> SaveInvoiceSettingsAsync(InvoiceSettingsDto dto, OperationContext operationContext);
        Task<OperationResult<string>> GetNextInvoiceNumberAsync(OperationContext operationContext);
        Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetCustomerInvoicesAsync(OperationContext context, int pageNumber, int pageSize, string status);
        Task<OperationResult<InvoiceResponseDto>> GetCustomerInvoiceByIdAsync(int id, OperationContext context);
    }
}