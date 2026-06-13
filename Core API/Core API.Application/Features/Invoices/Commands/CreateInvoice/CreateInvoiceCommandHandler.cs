using AutoMapper;
using Core_API.Application.Common.Events;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Exchange;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
using InvoiceStatus = Core_API.Domain.Enums.InvoiceStatus;
using PaymentStatus = Core_API.Domain.Enums.PaymentStatus;

namespace Core_API.Application.Features.Invoices.Commands.CreateInvoice;

/// <summary>
/// Handler for CreateInvoiceCommand
/// </summary>
public sealed class CreateInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateInvoiceCommandHandler> logger,
    IInvoiceNumberService invoiceNumberService,
    IInvoiceCalculationService invoiceCalculationService,
    IInvoiceAttachmentService invoiceAttachmentService,
    IExchangeRateService exchangeRateService,
   IPublisher publisher) : IRequestHandler<CreateInvoiceCommand, OperationResult<InvoiceResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger = logger;
    private readonly IInvoiceNumberService _invoiceNumberService = invoiceNumberService;
    private readonly IInvoiceCalculationService _invoiceCalculationService = invoiceCalculationService;
    private readonly IInvoiceAttachmentService _invoiceAttachmentService = invoiceAttachmentService;
    private readonly IExchangeRateService _exchangeRateService = exchangeRateService;
    private readonly IPublisher _publisher = publisher;

    private static readonly HashSet<string> _supportedCurrencies = new()
    {
        "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "INR", "NZD"
    };

    public async Task<OperationResult<InvoiceResponseDto>> Handle(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        // Validate CompanyId
        if (!context.CompanyId.HasValue)
        {
            _logger.LogWarning("Company ID is required for creating an invoice.");
            return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
        }

        var companyId = context.CompanyId.Value;

        // Generate invoice number BEFORE transaction
        string invoiceNumber;
        if (request.IsAutomated)
        {
            invoiceNumber = await _invoiceNumberService.GenerateUniqueInvoiceNumberAsync(companyId);
        }
        else
        {
            invoiceNumber = request.InvoiceNumber ?? $"INV{DateTime.UtcNow.Ticks}";
        }

        // Start transaction
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get company to get base currency
            var company = await _unitOfWork.Companies.GetAsync(c => c.Id == companyId);
            if (company == null)
            {
                return OperationResult<InvoiceResponseDto>.FailureResult("Company not found.");
            }

            // Validate customer
            var customer = await _unitOfWork.Customers.GetAsync(
                c => c.Id == request.CustomerId && c.CompanyId == companyId && !c.IsDeleted,
                tracked: true);

            if (customer == null)
            {
                return OperationResult<InvoiceResponseDto>.FailureResult(
                    "Customer not found or does not belong to your company.");
            }

            // Validate invoice number
            if (!request.IsAutomated)
            {
                if (string.IsNullOrWhiteSpace(invoiceNumber))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult(
                        "Invoice number is required for non-automated creation.");
                }

                if (await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, invoiceNumber))
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

            // Fetch current exchange rate from API
            decimal exchangeRate = await _exchangeRateService.GetExchangeRateAsync(
                company.BaseCurrency, request.Currency);

            if (exchangeRate <= 0)
            {
                _logger.LogError("Invalid exchange rate returned: {Rate} for {Base} to {Target}",
                    exchangeRate, company.BaseCurrency, request.Currency);
                return OperationResult<InvoiceResponseDto>.FailureResult(
                    "Unable to process invoice: Invalid exchange rate received.");
            }

            _logger.LogInformation("Fetched exchange rate from {BaseCurrency} to {InvoiceCurrency}: {Rate}",
                company.BaseCurrency, request.Currency, exchangeRate);

            // Validate tax types
            var taxTypes = await _unitOfWork.TaxTypes.Query()
                .Where(t => t.CompanyId == companyId && !t.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items)
            {
                if (!string.IsNullOrEmpty(item.TaxType) && !taxTypes.Any(t => t.Name == item.TaxType))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid tax type: {item.TaxType}.");
                }
            }

            // Validate discounts
            if (request.Discounts.Any(d => d.Amount < 0))
            {
                return OperationResult<InvoiceResponseDto>.FailureResult("Discount amount cannot be negative.");
            }

            // Map DTO to entity
            var invoice = _mapper.Map<Domain.Entities.Invoices.Invoice>(request);
            invoice.InvoiceNumber = invoiceNumber;
            invoice.CompanyId = companyId;
            invoice.CreatedBy = context.UserId;
            invoice.CreatedDate = DateTime.UtcNow;
            invoice.InvoiceStatus = InvoiceStatus.Draft;
            invoice.PaymentStatus = PaymentStatus.Pending;
            invoice.AmountPaid = 0;
            invoice.AmountRefunded = 0;
            invoice.AmountDue = 0;
            invoice.SourceSystem = request.SourceSystem ?? "Manual";
            invoice.Customer = customer;

            // Calculate item amounts and tax amounts
            _invoiceCalculationService.CalculateItemAmounts(invoice.InvoiceItems, taxTypes);

            // Calculate tax details from items
            _invoiceCalculationService.CalculateTaxDetails(invoice.InvoiceItems, invoice.TaxDetails, taxTypes);

            // Calculate discounts
            _invoiceCalculationService.CalculateDiscount(invoice.Discounts, invoice.Subtotal);

            // Calculate all totals
            _invoiceCalculationService.CalculateInvoiceTotals(invoice);

            // Convert to base currency for reporting
            invoice.BaseCurrencySubtotal = invoice.Subtotal / exchangeRate;
            invoice.BaseCurrencyTotalAmount = invoice.TotalAmount / exchangeRate;

            // Calculate AmountDue
            invoice.AmountDue = _invoiceCalculationService.CalculateAmountDue(invoice.TotalAmount, invoice.AmountPaid);
            invoice.BaseCurrencyAmountDue = invoice.BaseCurrencyTotalAmount - invoice.BaseCurrencyAmountPaid;

            // Add invoice
            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Handle attachments after invoice has ID
            if (request.Attachments?.Any() == true)
            {
                invoice.InvoiceAttachments = await _invoiceAttachmentService.HandleAttachmentsAsync(
                    request.Attachments, companyId, invoice.Id, context.UserId);

                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Create audit log
            var auditLog = CreateAuditLog(invoice.Id, request, invoice, context);
            await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            // Map to response DTO
            var response = _mapper.Map<InvoiceResponseDto>(invoice);
            response.Customer = _mapper.Map<CustomerResponseDto>(customer);

            // After successful creation, publish event
            await _publisher.Publish(new InvoiceCreatedEvent
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                Currency = invoice.Currency,
                TotalAmount = invoice.TotalAmount,
                DueDate = invoice.DueDate,
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation("Invoice {InvoiceNumber} created successfully with ID {InvoiceId}",
                invoice.InvoiceNumber, invoice.Id);

            return OperationResult<InvoiceResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating invoice for company {CompanyId}", companyId);
            return OperationResult<InvoiceResponseDto>.FailureResult("Failed to create invoice: " + ex.Message);
        }
    }

    #region Private Helper Methods

    private static InvoiceAuditLog CreateAuditLog(
        int invoiceId,
        CreateInvoiceCommand command,
        Domain.Entities.Invoices.Invoice invoice,
        OperationContext operationContext)
    {
        // Create a summary of the invoice for audit log (reduced size)
        var auditSummary = new
        {
            invoice.InvoiceNumber,
            command.CustomerId,
            command.IssueDate,
            command.DueDate,
            command.Currency,
            command.CurrencyRate,
            invoice.Subtotal,
            invoice.DiscountTotal,
            invoice.TaxTotal,
            command.ShippingAmount,
            command.AdjustmentAmount,
            invoice.TotalAmount,
            ItemsCount = command.Items?.Count ?? 0,
            DiscountsCount = command.Discounts?.Count ?? 0,
            TaxDetailsCount = command.TaxDetails?.Count ?? 0,
            command.IsAutomated,
            command.PaymentMethod,
            command.PaymentTerms,
            command.SourceSystem
        };

        var changesJson = JsonSerializer.Serialize(auditSummary);

        // Ensure we don't exceed the maximum length (2000 characters)
        const int maxLength = 2000;
        if (changesJson.Length > maxLength)
        {
            changesJson = changesJson[..(maxLength - 3)] + "...";
        }

        return new InvoiceAuditLog
        {
            InvoiceId = invoiceId,
            Action = "Created",
            Description = $"Invoice created by {operationContext.UserId}",
            Changes = changesJson,
            IpAddress = "system",
            UserAgent = "system",
            CreatedBy = operationContext.UserId,
            CreatedDate = DateTime.UtcNow
        };
    }
    #endregion
}