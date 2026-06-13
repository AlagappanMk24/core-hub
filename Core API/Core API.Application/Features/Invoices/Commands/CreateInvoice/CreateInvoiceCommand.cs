using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Domain.Enums;

namespace Core_API.Application.Features.Invoices.Commands.CreateInvoice;

/// <summary>
/// Command to create a new invoice
/// </summary>
public record CreateInvoiceCommand : BaseCommand<InvoiceResponseDto>
{
    /// <summary>
    /// Unique invoice number. Optional for automated invoices.
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Date the invoice is issued
    /// </summary>
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date by which payment is expected
    /// </summary>
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

    /// <summary>
    /// Invoice type (Standard, Recurring, Proforma, CreditNote, etc.)
    /// </summary>
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Standard;

    /// <summary>
    /// Shipping or freight charge
    /// </summary>
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Small rounding or manual adjustment applied to the invoice total
    /// </summary>
    public decimal AdjustmentAmount { get; set; }

    /// <summary>
    /// Human-readable reason for the adjustment amount
    /// </summary>
    public string? AdjustmentDescription { get; set; }

    /// <summary>
    /// Payment gateway used for this invoice
    /// </summary>
    public string? PaymentGateway { get; set; }

    /// <summary>
    /// Whether this invoice is auto-generated
    /// </summary>
    public bool IsAutomated { get; set; }

    /// <summary>
    /// Parent recurring invoice ID if this is a recurring instance
    /// </summary>
    public int? RecurringInvoiceId { get; set; }

    /// <summary>
    /// Originating system (API, Manual, Import)
    /// </summary>
    public string? SourceSystem { get; set; }

    /// <summary>
    /// ID of the customer being billed
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Billing address ID (optional, uses customer default if not provided)
    /// </summary>
    public int? BillingAddressId { get; set; }

    /// <summary>
    /// Shipping address ID (optional)
    /// </summary>
    public int? ShippingAddressId { get; set; }

    /// <summary>
    /// ISO 4217 currency code
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Exchange rate relative to base currency
    /// </summary>
    public decimal CurrencyRate { get; set; } = 1;

    /// <summary>
    /// Customer's purchase order number
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Notes visible to the customer
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Internal notes (staff only)
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Terms and conditions
    /// </summary>
    public string? TermsAndConditions { get; set; }

    /// <summary>
    /// Footer note
    /// </summary>
    public string? FooterNote { get; set; }

    /// <summary>
    /// Project reference details
    /// </summary>
    public string? ProjectDetail { get; set; }

    /// <summary>
    /// Preferred payment method
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Payment terms (e.g., "Net 30", "Due on Receipt")
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Current status of the invoice (optional, defaults to Draft)
    /// </summary>
    public InvoiceStatus? InvoiceStatus { get; set; }

    /// <summary>
    /// Current payment status (optional, defaults to Pending)
    /// </summary>
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// Line items for this invoice
    /// </summary>
    public List<InvoiceItemDto> Items { get; set; } = new();

    /// <summary>
    /// Tax details for this invoice
    /// </summary>
    public List<InvoiceTaxDetailDto> TaxDetails { get; set; } = new();

    /// <summary>
    /// Discounts applied to the invoice
    /// </summary>
    public List<InvoiceDiscountDto> Discounts { get; set; } = new();

    /// <summary>
    /// Attachments for this invoice
    /// </summary>
    public List<InvoiceAttachmentDto> Attachments { get; set; } = new();

    /// <summary>
    /// Creates a CreateInvoiceCommand from CreateInvoiceDto
    /// </summary>
    public static CreateInvoiceCommand FromDto(CreateInvoiceDto dto)
    {
        return new CreateInvoiceCommand
        {
            InvoiceNumber = dto.InvoiceNumber,
            IssueDate = dto.IssueDate,
            DueDate = dto.DueDate,
            InvoiceType = dto.InvoiceType,
            ShippingAmount = dto.ShippingAmount,
            AdjustmentAmount = dto.AdjustmentAmount,
            AdjustmentDescription = dto.AdjustmentDescription,
            PaymentGateway = dto.PaymentGateway,
            IsAutomated = dto.IsAutomated,
            RecurringInvoiceId = dto.RecurringInvoiceId,
            SourceSystem = dto.SourceSystem,
            CustomerId = dto.CustomerId,
            BillingAddressId = dto.BillingAddressId,
            ShippingAddressId = dto.ShippingAddressId,
            Currency = dto.Currency,
            CurrencyRate = dto.CurrencyRate,
            PONumber = dto.PONumber,
            CustomerNotes = dto.CustomerNotes,
            InternalNotes = dto.InternalNotes,
            TermsAndConditions = dto.TermsAndConditions,
            FooterNote = dto.FooterNote,
            ProjectDetail = dto.ProjectDetail,
            PaymentMethod = dto.PaymentMethod,
            PaymentTerms = dto.PaymentTerms,
            InvoiceStatus = dto.InvoiceStatus,
            PaymentStatus = dto.PaymentStatus,
            Items = dto.Items,
            TaxDetails = dto.TaxDetails,
            Discounts = dto.Discounts,
            Attachments = dto.Attachments
        };
    }
}