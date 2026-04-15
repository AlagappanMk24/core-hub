using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Email;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.User.Response;

namespace Core_API.Application.Contracts.Services
{
    /// <summary>
    /// Main invoice service interface for core CRUD operations
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Creates a new invoice
        /// </summary>
        Task<OperationResult<InvoiceResponseDto>> CreateAsync(InvoiceCreateDto dto, OperationContext operationContext);

        /// <summary>
        /// Updates an existing invoice
        /// </summary>
        Task<OperationResult<InvoiceResponseDto>> UpdateAsync(InvoiceUpdateDto dto, OperationContext operationContext);

        /// <summary>
        /// Deletes an invoice (soft delete)
        /// </summary>
        Task<OperationResult<bool>> DeleteAsync(int id, OperationContext operationContext);

        /// <summary>
        /// Retrieves an invoice by ID
        /// </summary>
        Task<OperationResult<InvoiceResponseDto>> GetByIdAsync(int id, OperationContext operationContext);

        /// <summary>
        /// Retrieves a paged list of invoices
        /// </summary>
        Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetPagedAsync(
          OperationContext operationContext,
          InvoiceFilterRequestDto filter);

        /// <summary>
        /// Duplicates an existing invoice
        /// </summary>
        Task<OperationResult<InvoiceResponseDto>> DuplicateAsync(int id, OperationContext operationContext);

        /// <summary>
        /// Sends an invoice email to customer
        /// </summary>
        Task<OperationResult<bool>> SendInvoiceAsync(int id, OperationContext operationContext, EmailDataDto emailData);

        /// <summary>
        /// Maps to InvoiceResponseDto from Invoice entity
        /// </summary>
        InvoiceResponseDto MapToInvoiceResponseDto(Domain.Entities.Invoice invoice);
    }
}