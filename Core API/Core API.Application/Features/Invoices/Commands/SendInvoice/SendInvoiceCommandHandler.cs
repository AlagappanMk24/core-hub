using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Domain.Entities.Invoices;
using InvoiceStatus = Core_API.Domain.Enums.InvoiceStatus;

namespace Core_API.Application.Features.Invoices.Commands.SendInvoice;

/// <summary>
/// Handler for SendInvoiceCommand
/// </summary>
public sealed class SendInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<SendInvoiceCommandHandler> logger,
    IInvoiceEmailService invoiceEmailService) : IRequestHandler<SendInvoiceCommand, OperationResult<bool>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<SendInvoiceCommandHandler> _logger = logger;
    private readonly IInvoiceEmailService _invoiceEmailService = invoiceEmailService;

    public async Task<OperationResult<bool>> Handle(
        SendInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        // Validate CompanyId
        if (!context.CompanyId.HasValue && !context.IsSuperAdmin)
        {
            _logger.LogWarning("Company ID is required for sending an invoice.");
            return OperationResult<bool>.FailureResult("Company ID is required.");
        }

        try
        {
            _logger.LogInformation("Sending invoice {InvoiceId} for UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}, CompanyId: {CompanyId}",
                request.Id, context.UserId, context.IsSuperAdmin, context.CompanyId);

            // Build query with proper filtering
            IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query()
                .Where(i => !i.IsDeleted);

            // Super Admin can send any invoice
            if (context.IsSuperAdmin)
            {
                _logger.LogInformation("Super Admin sending invoice {InvoiceId}", request.Id);
                // No company filter for Super Admin
            }
            else if (context.CompanyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == context.CompanyId.Value);
            }
            else
            {
                return OperationResult<bool>.FailureResult("Company ID is required.");
            }

            // Fetch invoice with related data
            var invoice = await query
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .Include(i => i.TaxDetails)
                .Include(i => i.Discounts)
                .Include(i => i.InvoiceAttachments)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to company {CompanyId}",
                    request.Id, context.CompanyId);
                return OperationResult<bool>.FailureResult("Invoice not found or does not belong to your company.");
            }

            // Validate customer exists
            if (invoice.Customer == null || invoice.Customer.IsDeleted)
            {
                _logger.LogWarning("Invoice {InvoiceId} has invalid or deleted customer", request.Id);
                return OperationResult<bool>.FailureResult("Invoice has an invalid customer. Cannot send invoice.");
            }

            // Check if invoice is already sent
            if (invoice.InvoiceStatus == InvoiceStatus.Sent && invoice.SentDate.HasValue)
            {
                _logger.LogWarning("Invoice {InvoiceId} has already been sent on {SentDate}",
                    request.Id, invoice.SentDate);
                // Allow resending - just log warning
            }

            // Validate email data
            if (request.EmailData.To == null || request.EmailData.To.Count == 0 ||
                request.EmailData.To.All(e => string.IsNullOrWhiteSpace(e)))
            {
                _logger.LogWarning("No valid 'To' email addresses provided for invoice {InvoiceId}", request.Id);
                return OperationResult<bool>.FailureResult("At least one valid 'To' email address is required.");
            }

            // Send email
            var emailSent = await _invoiceEmailService.SendInvoiceEmailAsync(invoice, request.EmailData, context);

            if (!emailSent)
            {
                _logger.LogError("Failed to send email for invoice {InvoiceId}", request.Id);
                return OperationResult<bool>.FailureResult("Failed to send email. Please check email configuration and try again.");
            }

            // Update invoice
            invoice.SentDate = DateTime.UtcNow;
            invoice.InvoiceStatus = InvoiceStatus.Sent;
            invoice.UpdatedBy = context.UserId;
            invoice.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create audit log
            var auditLog = new InvoiceAuditLog
            {
                InvoiceId = invoice.Id,
                Action = "Sent",
                Description = $"Invoice sent to {string.Join(", ", request.EmailData.To)}",
                Changes = JsonSerializer.Serialize(new
                {
                    SentDate = DateTime.UtcNow,
                    Recipients = request.EmailData.To,
                    Cc = request.EmailData.Cc,
                    Bcc = request.EmailData.Bcc,
                    Subject = request.EmailData.Subject,
                    AttachPdf = request.EmailData.AttachPdf,
                    SendCopyToSelf = request.EmailData.SendCopyToSelf
                }),
                IpAddress = "system",
                UserAgent = "system",
                CreatedBy = context.UserId,
                CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Invoice {InvoiceId} sent successfully to {Email} for company {CompanyId}",
                invoice.Id, string.Join(", ", request.EmailData.To), invoice.CompanyId);

            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invoice {InvoiceId} for company {CompanyId}",
                request.Id, context.CompanyId);
            return OperationResult<bool>.FailureResult("Failed to send invoice: " + ex.Message);
        }
    }
}