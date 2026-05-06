using Core_API.Application.Common.Models;
using Core_API.Application.DTOs.Email.Requests;
using Core_API.Domain.Models.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_API.Application.Contracts.Services.Email
{
    /// <summary>
    /// Service for invoice email operations
    /// </summary>
    public interface IInvoiceEmailService
    {
        /// <summary>
        /// Sends invoice email to customer
        /// </summary>
        Task<bool> SendInvoiceEmailAsync(Domain.Entities.Invoices.Invoice invoice, EmailDataDto emailData, OperationContext operationContext);

        /// <summary>
        /// Generates default HTML email message for invoice
        /// </summary>
        string GenerateDefaultEmailMessage(Domain.Entities.Invoices.Invoice invoice);

        /// <summary>
        /// Prepares email request from invoice data
        /// </summary>
        InvoiceEmailRequest PrepareEmailRequest(Domain.Entities.Invoices.Invoice invoice, EmailDataDto emailData);

        /// <summary>
        /// Generates PDF attachment for invoice email
        /// </summary>
        Task<MemoryStream> GenerateInvoicePdfAttachmentAsync(int invoiceId, OperationContext operationContext);
    }
}
