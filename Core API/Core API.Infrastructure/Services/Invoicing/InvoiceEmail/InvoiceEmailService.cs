using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.Contracts.Services.Files;
using Core_API.Application.DTOs.Email.Requests;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Models.Email;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Invoicing.InvoiceEmail
{
    /// <summary>
    /// Implementation of invoice email service
    /// </summary>
    public class InvoiceEmailService(
        IEmailServiceProvider emailServiceProvider,
        IPdfService pdfService,
        ILogger<InvoiceEmailService> logger) : IInvoiceEmailService
    {
        private readonly IEmailServiceProvider _emailServiceProvider = emailServiceProvider;
        private readonly IPdfService _pdfService = pdfService;
        private readonly ILogger<InvoiceEmailService> _logger = logger;
        public async Task<bool> SendInvoiceEmailAsync(Domain.Entities.Invoices.Invoice invoice, EmailDataDto emailData, OperationContext operationContext)
        {
            try
            {
                var emailRequest = PrepareEmailRequest(invoice, emailData);
                MemoryStream pdfStream = null;

                if (emailData.AttachPdf)
                {
                    pdfStream = await GenerateInvoicePdfAttachmentAsync(invoice.Id, operationContext);
                }

                await _emailServiceProvider.SendInvoiceEmailAsync(invoice.Id, emailData, operationContext);

                _logger.LogInformation("Invoice {InvoiceNumber} email sent successfully", invoice.InvoiceNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice email for {InvoiceNumber}", invoice.InvoiceNumber);
                return false;
            }
        }

        public string GenerateDefaultEmailMessage(Domain.Entities.Invoices.Invoice invoice)
        {
            return $@"
            <html>
            <body>
                <h3>Invoice {invoice.InvoiceNumber}</h3>
                <p>Dear {invoice.Customer?.Name},</p>
                <p>Please find attached invoice {invoice.InvoiceNumber} for your reference.</p>
                <p><strong>Invoice Details:</strong></p>
                <ul>
                    <li>Invoice Number: {invoice.InvoiceNumber}</li>
                    <li>Issue Date: {invoice.IssueDate:dd MMM yyyy}</li>
                    <li>Due Date: {invoice.DueDate:dd MMM yyyy}</li>
                    <li>Total Amount: {invoice.TotalAmount:C} {invoice.Currency}</li>
                </ul>
                <p>Thank you for your business!</p>
            </body>
            </html>";
        }

        public InvoiceEmailRequest PrepareEmailRequest(Domain.Entities.Invoices.Invoice invoice, EmailDataDto emailData)
        {
            return new InvoiceEmailRequest
            {
                To = emailData.To.Where(e => !string.IsNullOrWhiteSpace(e)).ToList(),
                Cc = emailData.Cc?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>(),
                Bcc = emailData.Bcc?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>(),
                Subject = emailData.Subject ?? $"Invoice {invoice.InvoiceNumber}",
                HtmlMessage = emailData.Message ?? GenerateDefaultEmailMessage(invoice),
                InvoiceNumber = invoice.InvoiceNumber,
                AmountDue = invoice.TotalAmount,
                DueDate = invoice.DueDate
            };
        }

        public async Task<MemoryStream> GenerateInvoicePdfAttachmentAsync(int invoiceId, OperationContext operationContext)
        {
            var pdfResult = await _pdfService.GenerateInvoicePdfAsync(invoiceId, operationContext);

            if (!pdfResult.IsSuccess)
            {
                throw new InvalidOperationException($"Failed to generate PDF for invoice {invoiceId}");
            }

            return pdfResult.Data.PdfStream;
        }
    }
}