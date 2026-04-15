using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;

namespace Core_API.Application.Contracts.Services.Invoice
{
    /// <summary>
    /// Service for generating invoice numbers
    /// </summary>
    public interface IInvoiceNumberService
    {
        /// <summary>
        /// Generates a unique invoice number for new invoices
        /// </summary>
        Task<string> GenerateUniqueInvoiceNumberAsync(int companyId);

        /// <summary>
        /// Generates a duplicate invoice number based on original invoice
        /// </summary>
        Task<string> GenerateDuplicateInvoiceNumberAsync(int companyId, Domain.Entities.Invoice originalInvoice);

        /// <summary>
        /// Gets the next available invoice number from settings
        /// </summary>
        Task<OperationResult<string>> GetNextInvoiceNumberAsync(OperationContext operationContext);
    }
}
