// Core_API.Application/Features/Invoices/Commands/DeleteInvoice/DeleteInvoiceCommandHandler.cs
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core_API.Application.Features.Invoices.Commands.DeleteInvoice;

/// <summary>
/// Handler for DeleteInvoiceCommand
/// </summary>
public sealed class DeleteInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<DeleteInvoiceCommandHandler> logger) : IRequestHandler<DeleteInvoiceCommand, OperationResult<bool>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<DeleteInvoiceCommandHandler> _logger = logger;

    public async Task<OperationResult<bool>> Handle(
        DeleteInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        _logger.LogInformation("Processing delete invoice request for InvoiceId: {InvoiceId}, UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}",
            request.Id, context.UserId, context.IsSuperAdmin);

        // Start transaction
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Build query with proper company filtering
            var invoice = await GetInvoiceWithAccessCheck(request.Id, context, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found or access denied for user {UserId}",
                    request.Id, context.UserId);
                return OperationResult<bool>.FailureResult("Invoice not found or you don't have permission to delete it.");
            }

            // Store invoice info for logging
            var invoiceNumber = invoice.InvoiceNumber;
            var customerId = invoice.CustomerId;

            // Perform soft delete
            PerformSoftDelete(invoice, context.UserId);

            // Update and save
            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create audit log
            await CreateAuditLog(invoice, context.UserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Invoice {InvoiceNumber} (ID: {InvoiceId}) deleted successfully by user {UserId}",
                invoiceNumber, invoice.Id, context.UserId);

            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error deleting invoice {InvoiceId} for user {UserId}",
                request.Id, context.UserId);
            return OperationResult<bool>.FailureResult($"Failed to delete invoice: {ex.Message}");
        }
    }

    #region Private Methods

    private async Task<Domain.Entities.Invoices.Invoice?> GetInvoiceWithAccessCheck(
        int invoiceId,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query()
            .Where(i => !i.IsDeleted)
            .Include(i => i.InvoiceAttachments)
            .Include(i => i.Customer);

        // Super Admin can access any invoice
        if (context.IsSuperAdmin)
        {
            return await query.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        }

        // Regular users can only access their company's invoices
        if (context.CompanyId.HasValue)
        {
            query = query.Where(i => i.CompanyId == context.CompanyId.Value);
            return await query.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        }

        return null;
    }

    private static void PerformSoftDelete(Domain.Entities.Invoices.Invoice invoice, string userId)
    {
        invoice.IsDeleted = true;
        invoice.UpdatedBy = userId;
        invoice.UpdatedDate = DateTime.UtcNow;

        // Soft delete attachments
        foreach (var attachment in invoice.InvoiceAttachments)
        {
            attachment.IsDeleted = true;
            attachment.UpdatedBy = userId;
            attachment.UpdatedDate = DateTime.UtcNow;
        }
    }

    private async Task CreateAuditLog(Domain.Entities.Invoices.Invoice invoice, string userId, CancellationToken cancellationToken)
    {
        var auditLog = new InvoiceAuditLog
        {
            InvoiceId = invoice.Id,
            Action = "Deleted",
            Description = $"Invoice deleted by {userId}",
            Changes = JsonSerializer.Serialize(new
            {
                DeletedDate = DateTime.UtcNow,
                DeletedBy = userId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                TotalAmount = invoice.TotalAmount
            }),
            IpAddress = "system",
            UserAgent = "system",
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
    }

    #endregion
}