using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities.Invoices;
using InvoiceStatus = Core_API.Domain.Enums.InvoiceStatus;
using PaymentStatus = Core_API.Domain.Enums.PaymentStatus;

namespace Core_API.Application.Features.Invoices.Commands.UpdateInvoice;

/// <summary>
/// Handler for UpdateInvoiceCommand
/// </summary>
public sealed class UpdateInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateInvoiceCommandHandler> logger,
    IInvoiceNumberService invoiceNumberService,
    IInvoiceCalculationService invoiceCalculationService,
    IInvoiceAttachmentService invoiceAttachmentService) : IRequestHandler<UpdateInvoiceCommand, OperationResult<InvoiceResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UpdateInvoiceCommandHandler> _logger = logger;
    private readonly IInvoiceNumberService _invoiceNumberService = invoiceNumberService;
    private readonly IInvoiceCalculationService _invoiceCalculationService = invoiceCalculationService;
    private readonly IInvoiceAttachmentService _invoiceAttachmentService = invoiceAttachmentService;

    private static readonly HashSet<string> _supportedCurrencies = new()
    {
        "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "INR", "NZD"
    };

    public async Task<OperationResult<InvoiceResponseDto>> Handle(
        UpdateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        // Start transaction
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Build query with proper company filtering
            IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query()
                .Where(i => !i.IsDeleted);

            // Super Admin can update any invoice
            if (context.IsSuperAdmin)
            {
                _logger.LogInformation("Super Admin updating invoice {InvoiceId}", request.Id);
                // No company filter for Super Admin
            }
            else if (context.CompanyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == context.CompanyId.Value);
            }
            else
            {
                return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
            }

            // Get existing invoice with all related data
            var invoice = await query
                .Include(i => i.InvoiceItems)
                .Include(i => i.TaxDetails)
                .Include(i => i.Discounts)
                .Include(i => i.InvoiceAttachments)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (invoice == null)
            {
                return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
            }

            var companyId = invoice.CompanyId;

            // Validate customer
            var customer = await _unitOfWork.Customers.GetAsync(
                c => c.Id == request.CustomerId && c.CompanyId == companyId && !c.IsDeleted,
                tracked: true);

            if (customer == null)
            {
                return OperationResult<InvoiceResponseDto>.FailureResult("Customer not found or does not belong to your company.");
            }

            invoice.Customer = customer;

            // Validate invoice number uniqueness
            if (!request.IsAutomated)
            {
                if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number is required for non-automated creation.");
                }

                if (await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, request.InvoiceNumber, request.Id))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number already exists.");
                }
            }

            // Validate currency
            if (!_supportedCurrencies.Contains(request.Currency))
            {
                return OperationResult<InvoiceResponseDto>.FailureResult(
                    $"Invalid currency. Supported currencies: {string.Join(", ", _supportedCurrencies)}.");
            }

            // Validate tax types
            var taxTypes = await _unitOfWork.TaxTypes.Query()
                .Where(t => t.CompanyId == companyId && !t.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items.Where(i => !string.IsNullOrEmpty(i.TaxType)))
            {
                if (!string.IsNullOrEmpty(item.TaxType) && !taxTypes.Any(t => t.Name == item.TaxType))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid tax type: {item.TaxType}.");
                }
            }

            foreach (var tax in request.TaxDetails.Where(t => !string.IsNullOrEmpty(t.TaxName)))
            {
                if (!taxTypes.Any(t => t.Name == tax.TaxName))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid tax type: {tax.TaxName}.");
                }
            }

            // Validate discounts
            if (request.Discounts.Any(d => d.Amount < 0))
            {
                return OperationResult<InvoiceResponseDto>.FailureResult("Discount amount cannot be negative.");
            }

            // Store old values for audit
            var oldInvoice = JsonSerializer.Serialize(new
            {
                invoice.InvoiceNumber,
                invoice.InvoiceStatus,
                invoice.PaymentStatus,
                invoice.TotalAmount,
                invoice.AmountPaid,
            });

            // Preserve payment information
            var existingAmountPaid = invoice.AmountPaid;
            var existingAmountRefunded = invoice.AmountRefunded;

            // Map DTO to existing entity (AutoMapper updates simple properties)
            _mapper.Map(request, invoice);

            // Restore payment information
            invoice.AmountPaid = existingAmountPaid;
            invoice.AmountRefunded = existingAmountRefunded;

            // Generate invoice number if automated
            invoice.InvoiceNumber = request.IsAutomated
                ? await _invoiceNumberService.GenerateUniqueInvoiceNumberAsync(companyId)
                : request.InvoiceNumber ?? invoice.InvoiceNumber;

            invoice.UpdatedBy = context.UserId;
            invoice.UpdatedDate = DateTime.UtcNow;

            // Update collections using AutoMapper
            // Items
            invoice.InvoiceItems.Clear();
            var mappedItems = _mapper.Map<List<InvoiceItem>>(request.Items);
            _invoiceCalculationService.CalculateItemAmounts(mappedItems, taxTypes);
            foreach (var item in mappedItems)
            {
                item.CreatedBy = context.UserId;
                item.CreatedDate = DateTime.UtcNow;
                invoice.InvoiceItems.Add(item);
            }

            // Update tax details
            invoice.TaxDetails.Clear();
            var mappedTaxDetails = _mapper.Map<List<InvoiceTaxDetail>>(request.TaxDetails);
            _invoiceCalculationService.CalculateTaxDetails(invoice.InvoiceItems, mappedTaxDetails, taxTypes);
            foreach (var tax in mappedTaxDetails)
            {
                tax.CreatedBy = context.UserId;
                tax.CreatedDate = DateTime.UtcNow;
                invoice.TaxDetails.Add(tax);
            }

            // Update discount details
            invoice.Discounts.Clear();
            var mappedDiscounts = _mapper.Map<List<InvoiceDiscount>>(request.Discounts);
            _invoiceCalculationService.CalculateDiscount(mappedDiscounts, invoice.Subtotal);
            foreach (var discount in mappedDiscounts)
            {
                discount.CreatedBy = context.UserId;
                discount.CreatedDate = DateTime.UtcNow;
                invoice.Discounts.Add(discount);
            }

            // Recalculate totals
            _invoiceCalculationService.CalculateInvoiceTotals(invoice);
            invoice.AmountDue = _invoiceCalculationService.CalculateAmountDue(invoice.TotalAmount, invoice.AmountPaid);

            // Handle attachments
            if (request.Attachments?.Any() == true)
            {
                var newAttachments = request.Attachments.Where(a => a.FileContent != null).ToList();

                if (newAttachments.Any())
                {
                    var addedAttachments = await _invoiceAttachmentService.HandleAttachmentsAsync(
                        newAttachments, companyId, invoice.Id, context.UserId);

                    foreach (var attachment in addedAttachments)
                    {
                        invoice.InvoiceAttachments.Add(attachment);
                    }
                }
            }

            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create audit log
            var auditLog = new InvoiceAuditLog
            {
                InvoiceId = invoice.Id,
                Action = "Updated",
                Description = $"Invoice updated by {context.UserId}",
                Changes = JsonSerializer.Serialize(new { Old = oldInvoice, New = request }),
                IpAddress = "system",
                UserAgent = "system",
                CreatedBy = context.UserId,
                CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            var response = _mapper.Map<InvoiceResponseDto>(invoice);
            response.Customer = _mapper.Map<CustomerResponseDto>(customer);

            _logger.LogInformation("Invoice {InvoiceId} updated successfully", invoice.Id);

            return OperationResult<InvoiceResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating invoice {InvoiceId} for company {CompanyId}",
                request.Id, context.CompanyId);
            return OperationResult<InvoiceResponseDto>.FailureResult("Failed to update invoice: " + ex.Message);
        }
    }
}