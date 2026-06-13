using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities.Invoices;
using InvoiceStatus = Core_API.Domain.Enums.InvoiceStatus;
using PaymentStatus = Core_API.Domain.Enums.PaymentStatus;

namespace Core_API.Application.Features.Invoices.Commands.DuplicateInvoice;

/// <summary>
/// Handler for DuplicateInvoiceCommand
/// </summary>
public sealed class DuplicateInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<DuplicateInvoiceCommandHandler> logger,
    IInvoiceNumberService invoiceNumberService,
    IInvoiceCalculationService invoiceCalculationService,
    IInvoiceAttachmentService invoiceAttachmentService) : IRequestHandler<DuplicateInvoiceCommand, OperationResult<InvoiceResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<DuplicateInvoiceCommandHandler> _logger = logger;
    private readonly IInvoiceNumberService _invoiceNumberService = invoiceNumberService;
    private readonly IInvoiceCalculationService _invoiceCalculationService = invoiceCalculationService;
    private readonly IInvoiceAttachmentService _invoiceAttachmentService = invoiceAttachmentService;

    public async Task<OperationResult<InvoiceResponseDto>> Handle(
        DuplicateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        _logger.LogInformation("Duplicating invoice {InvoiceId} for UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}, CompanyId: {CompanyId}",
            request.Id, context.UserId, context.IsSuperAdmin, context.CompanyId);

        // Step 1: Fetch the original invoice with all related data
        var invoiceQuery = _unitOfWork.Invoices.Query()
            .Include(i => i.InvoiceItems)
            .Include(i => i.TaxDetails)
            .Include(i => i.Discounts)
            .Include(i => i.InvoiceAttachments)
            .Include(i => i.Customer);

        Domain.Entities.Invoices.Invoice? originalInvoice = null;

        // Super Admin can duplicate any invoice across all companies
        if (context.IsSuperAdmin)
        {
            _logger.LogInformation("Super Admin attempting to duplicate invoice {InvoiceId}", request.Id);
            originalInvoice = await invoiceQuery.FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDeleted, cancellationToken);

            if (originalInvoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for Super Admin duplication", request.Id);
                return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found.");
            }
        }
        else
        {
            // Regular user must have CompanyId
            if (!context.CompanyId.HasValue)
            {
                _logger.LogWarning("Company ID is required for duplicating an invoice.");
                return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
            }

            var companyId = context.CompanyId.Value;

            originalInvoice = await invoiceQuery.FirstOrDefaultAsync(
                i => i.Id == request.Id && i.CompanyId == companyId && !i.IsDeleted,
                cancellationToken);

            if (originalInvoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to company {CompanyId}",
                    request.Id, companyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
            }
        }

        // Get the company ID from the original invoice
        var targetCompanyId = originalInvoice.CompanyId;

        // Step 2: Generate duplicate invoice number
        var duplicateInvoiceNumber = await _invoiceNumberService.GenerateDuplicateInvoiceNumberAsync(targetCompanyId, originalInvoice);

        // Step 3: Start transaction
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Step 4: Create a deep copy of the invoice
            var duplicatedInvoice = new Domain.Entities.Invoices.Invoice
            {
                // Copy basic properties
                InvoiceNumber = duplicateInvoiceNumber,
                IssueDate = DateTime.UtcNow,
                DueDate = _invoiceCalculationService.CalculateDueDateFromPaymentTerms(DateTime.UtcNow, originalInvoice.PaymentTerms),
                PONumber = originalInvoice.PONumber,
                ProjectDetail = originalInvoice.ProjectDetail,
                InvoiceType = originalInvoice.InvoiceType,
                Currency = originalInvoice.Currency,
                CurrencyRate = originalInvoice.CurrencyRate,
                CustomerId = originalInvoice.CustomerId,
                CustomerNotes = originalInvoice.CustomerNotes,
                InternalNotes = originalInvoice.InternalNotes,
                TermsAndConditions = originalInvoice.TermsAndConditions,
                FooterNote = originalInvoice.FooterNote,
                PaymentMethod = originalInvoice.PaymentMethod,
                PaymentTerms = originalInvoice.PaymentTerms,
                ShippingAmount = originalInvoice.ShippingAmount,
                AdjustmentAmount = originalInvoice.AdjustmentAmount,
                AdjustmentDescription = originalInvoice.AdjustmentDescription,

                // Set status as Draft
                InvoiceStatus = InvoiceStatus.Draft,
                PaymentStatus = PaymentStatus.Pending,

                // Reset payment-related fields
                AmountPaid = 0,
                AmountRefunded = 0,
                AmountDue = 0,
                SentDate = null,
                PaidDate = null,
                PaymentGateway = null,
                PaymentTransactionId = null,

                // Set system fields
                CompanyId = targetCompanyId,
                IsAutomated = originalInvoice.IsAutomated,
                SourceSystem = "Duplicate",
                CreatedBy = context.UserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            // Step 5: Copy line items
            foreach (var item in originalInvoice.InvoiceItems)
            {
                duplicatedInvoice.InvoiceItems.Add(new InvoiceItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxType = item.TaxType,
                    TaxPercentage = item.TaxPercentage,
                    TaxAmount = item.TaxAmount,
                    Amount = item.Amount,
                    TotalAmount = item.TotalAmount,
                    IsTaxable = item.IsTaxable,
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Step 6: Copy tax details
            foreach (var tax in originalInvoice.TaxDetails)
            {
                duplicatedInvoice.TaxDetails.Add(new InvoiceTaxDetail
                {
                    TaxName = tax.TaxName,
                    Rate = tax.Rate,
                    TaxAmount = tax.TaxAmount,
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Step 7: Copy discounts
            foreach (var discount in originalInvoice.Discounts)
            {
                duplicatedInvoice.Discounts.Add(new InvoiceDiscount
                {
                    Description = discount.Description,
                    DiscountType = discount.DiscountType,
                    Amount = discount.Amount,
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Step 8: Calculate all financial totals
            _invoiceCalculationService.CalculateInvoiceTotals(duplicatedInvoice);
            duplicatedInvoice.AmountDue = duplicatedInvoice.TotalAmount - duplicatedInvoice.AmountPaid;

            // Step 9: Handle attachments (copy files if needed)
            if (originalInvoice.InvoiceAttachments.Any())
            {
                duplicatedInvoice.InvoiceAttachments = _invoiceAttachmentService.CopyAttachments(
                    originalInvoice.InvoiceAttachments,
                    targetCompanyId,
                    context.UserId);
            }

            // Step 10: Save duplicated invoice
            await _unitOfWork.Invoices.AddAsync(duplicatedInvoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Step 11: Update attachment InvoiceIds after save
            if (duplicatedInvoice.InvoiceAttachments.Any())
            {
                foreach (var attachment in duplicatedInvoice.InvoiceAttachments)
                {
                    attachment.InvoiceId = duplicatedInvoice.Id;
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Step 12: Create audit log
            var auditLog = new InvoiceAuditLog
            {
                InvoiceId = duplicatedInvoice.Id,
                Action = "Duplicated",
                Description = $"Invoice duplicated from original invoice {originalInvoice.InvoiceNumber} (ID: {originalInvoice.Id}) by {context.UserId}",
                Changes = JsonSerializer.Serialize(new
                {
                    OriginalInvoiceId = originalInvoice.Id,
                    OriginalInvoiceNumber = originalInvoice.InvoiceNumber,
                    DuplicatedDate = DateTime.UtcNow,
                    DuplicatedBy = context.UserId
                }),
                IpAddress = "system",
                UserAgent = "system",
                CreatedBy = context.UserId,
                CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            // Step 13: Map to response DTO
            var response = _mapper.Map<InvoiceResponseDto>(duplicatedInvoice);
            response.Customer = _mapper.Map<CustomerResponseDto>(originalInvoice.Customer);

            _logger.LogInformation("Invoice {OriginalInvoiceNumber} duplicated successfully as {NewInvoiceNumber} (ID: {NewInvoiceId})",
                originalInvoice.InvoiceNumber, duplicatedInvoice.InvoiceNumber, duplicatedInvoice.Id);

            return OperationResult<InvoiceResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error duplicating invoice {InvoiceId} for company {CompanyId}",
                request.Id, context.CompanyId);
            return OperationResult<InvoiceResponseDto>.FailureResult("Failed to duplicate invoice: " + ex.Message);
        }
    }
}