using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.RecurringInvoice.Request;
using Core_API.Application.DTOs.RecurringInvoice.Response;
using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Services
{
    /// <summary>
    /// Service interface for managing recurring invoice templates and their generation
    /// </summary>
    public interface IRecurringInvoiceService
    {
        #region CRUD Operations

        /// <summary>
        /// Creates a new recurring invoice template
        /// </summary>
        Task<OperationResult<RecurringInvoiceResponseDto>> CreateAsync(RecurringInvoiceCreateDto dto, OperationContext context);

        /// <summary>
        /// Updates an existing recurring invoice template
        /// </summary>
        Task<OperationResult<RecurringInvoiceResponseDto>> UpdateAsync(RecurringInvoiceUpdateDto dto, OperationContext context);

        /// <summary>
        /// Deletes (soft delete) a recurring invoice template
        /// </summary>
        Task<OperationResult<bool>> DeleteAsync(int id, OperationContext context);

        /// <summary>
        /// Retrieves a recurring invoice template by ID with all details
        /// </summary>
        Task<OperationResult<RecurringInvoiceResponseDto>> GetByIdAsync(int id, OperationContext context);

        /// <summary>
        /// Retrieves a paged list of recurring invoice templates
        /// </summary>
        Task<OperationResult<PaginatedResult<RecurringInvoiceResponseDto>>> GetPagedAsync(OperationContext context, RecurringInvoiceFilterDto filter);

        #endregion

        #region Status Management

        /// <summary>
        /// Activates a draft recurring invoice template
        /// </summary>
        Task<OperationResult<bool>> ActivateAsync(int id, OperationContext context);

        /// <summary>
        /// Pauses an active recurring invoice template
        /// </summary>
        Task<OperationResult<bool>> PauseAsync(int id, OperationContext context);

        /// <summary>
        /// Resumes a paused recurring invoice template
        /// </summary>
        Task<OperationResult<bool>> ResumeAsync(int id, OperationContext context);

        /// <summary>
        /// Cancels an active or paused recurring invoice template
        /// </summary>
        Task<OperationResult<bool>> CancelAsync(int id, OperationContext context);

        #endregion

        #region Generation Operations

        /// <summary>
        /// Manually generates an invoice from a recurring template
        /// </summary>
        Task<OperationResult<InvoiceResponseDto>> GenerateInvoiceManuallyAsync(GenerateManualDto dto, OperationContext context);

        /// <summary>
        /// Updates the next invoice date for a recurring template
        /// </summary>
        Task<OperationResult<bool>> UpdateNextInvoiceDateAsync(int id, DateTime nextDate, OperationContext context);

        /// <summary>
        /// Gets all due recurring invoices for background processing
        /// </summary>
        Task<List<RecurringInvoice>> GetDueRecurringInvoicesAsync(DateTime asOfDate);

        /// <summary>
        /// Processes due invoices (called by background service)
        /// </summary>
        Task<OperationResult<int>> ProcessDueInvoicesAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Customer-Specific Operations

        /// <summary>
        /// Retrieves recurring invoices for a specific customer
        /// </summary>
        Task<OperationResult<List<RecurringInvoiceSummaryDto>>> GetCustomerRecurringInvoicesAsync(int customerId, OperationContext context);

        #endregion

        #region Statistics and Dashboard

        /// <summary>
        /// Retrieves status counts for recurring invoices
        /// </summary>
        Task<OperationResult<Dictionary<string, int>>> GetStatusCountsAsync(OperationContext context);

        /// <summary>
        /// Retrieves statistics for recurring invoices dashboard
        /// </summary>
        Task<OperationResult<RecurringInvoiceStatsDto>> GetStatsAsync(OperationContext context);

        #endregion
    }
}