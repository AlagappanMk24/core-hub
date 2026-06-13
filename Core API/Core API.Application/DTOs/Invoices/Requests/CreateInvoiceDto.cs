using Core_API.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Invoices.Requests
{
    /// <summary>
    /// DTO for creating a new invoice
    /// </summary>
    public class CreateInvoiceDto
    {
        // ── Identity ─────────────────────────────────────────────────────────
        /// <summary>
        /// Unique invoice number. Optional for automated invoices.
        /// </summary>
        [Required(ErrorMessage = "Invoice number is required")]
        [StringLength(50, ErrorMessage = "Invoice number cannot exceed 50 characters")]
        public string? InvoiceNumber { get; set; }

        // ── Dates ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Date the invoice is issued
        /// </summary>
        [Required(ErrorMessage = "Issue date is required")]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date by which payment is expected
        /// </summary>
        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

        // ── Status ────────────────────────────────────────────────────────────
        /// <summary>
        /// Invoice type (Standard, Recurring, Proforma, CreditNote, etc.)
        /// </summary>
        [Required(ErrorMessage = "Invoice type is required")]
        public InvoiceType InvoiceType { get; set; } = InvoiceType.Standard;

        // ── Financial Adjustments ─────────────────────────────────────────────

        /// <summary>
        /// Shipping or freight charge
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Shipping amount must be positive")]
        public decimal ShippingAmount { get; set; }

        /// <summary>
        /// Small rounding or manual adjustment applied to the invoice total
        /// </summary>
        [Range(-999999.99, 999999.99, ErrorMessage = "Adjustment amount must be within range")]
        public decimal AdjustmentAmount { get; set; }

        /// <summary>
        /// Human-readable reason for the adjustment amount
        /// </summary>
        [StringLength(200, ErrorMessage = "Adjustment description cannot exceed 200 characters")]
        public string? AdjustmentDescription { get; set; }

        // ── Payment Gateway ───────────────────────────────────────────────────
        /// <summary>
        /// Payment gateway used for this invoice
        /// </summary>
        [StringLength(50, ErrorMessage = "Payment gateway cannot exceed 50 characters")]
        public string? PaymentGateway { get; set; }

        // ── Automation ────────────────────────────────────────────────────────
        /// <summary>
        /// Whether this invoice is auto-generated
        /// </summary>
        public bool IsAutomated { get; set; }

        // ── Recurring Reference ───────────────────────────────────────────────
        /// <summary>
        /// Parent recurring invoice ID if this is a recurring instance
        /// </summary>
        public int? RecurringInvoiceId { get; set; }

        // ── Source / Integration ──────────────────────────────────────────────
        /// <summary>
        /// Originating system (API, Manual, Import)
        /// </summary>
        [StringLength(200, ErrorMessage = "Source system cannot exceed 200 characters")]
        public string? SourceSystem { get; set; }

        // ── Parties ───────────────────────────────────────────────────────────
        /// <summary>
        /// ID of the customer being billed
        /// </summary>
        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        // ── Addresses ─────────────────────────────────────────────────────────
        /// <summary>
        /// Billing address ID (optional, uses customer default if not provided)
        /// </summary>
        public int? BillingAddressId { get; set; }

        /// <summary>
        /// Shipping address ID (optional)
        /// </summary>
        public int? ShippingAddressId { get; set; }

        // ── Currency ──────────────────────────────────────────────────────────
        /// <summary>
        /// ISO 4217 currency code
        /// </summary>
        [Required(ErrorMessage = "Currency is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Exchange rate relative to base currency
        /// </summary>
        [Range(0.0001, 999999.9999, ErrorMessage = "Currency rate must be positive")]
        public decimal CurrencyRate { get; set; } = 1;

        // ── Reference Numbers ─────────────────────────────────────────────────
        /// <summary>
        /// Customer's purchase order number
        /// </summary>
        [StringLength(50, ErrorMessage = "PO number cannot exceed 50 characters")]
        public string? PONumber { get; set; }

        // ── Notes & Terms ─────────────────────────────────────────────────────
        /// <summary>
        /// Notes visible to the customer
        /// </summary>
        [StringLength(1000, ErrorMessage = "Customer notes cannot exceed 1000 characters")]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Internal notes (staff only)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Internal notes cannot exceed 1000 characters")]
        public string? InternalNotes { get; set; }

        /// <summary>
        /// Terms and conditions
        /// </summary>
        [StringLength(500, ErrorMessage = "Terms and conditions cannot exceed 500 characters")]
        public string? TermsAndConditions { get; set; }

        /// <summary>
        /// Footer note
        /// </summary>
        [StringLength(500, ErrorMessage = "Footer note cannot exceed 500 characters")]
        public string? FooterNote { get; set; }

        /// <summary>
        /// Project reference details
        /// </summary>
        [StringLength(500, ErrorMessage = "Project detail cannot exceed 500 characters")]
        public string? ProjectDetail { get; set; }

        // ── Payment Terms ─────────────────────────────────────────────────────
        /// <summary>
        /// Preferred payment method
        /// </summary>
        [StringLength(100, ErrorMessage = "Payment method cannot exceed 100 characters")]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Payment terms (e.g., "Net 30", "Due on Receipt")
        /// </summary>
        [StringLength(100, ErrorMessage = "Payment terms cannot exceed 100 characters")]
        public string? PaymentTerms { get; set; }

        // ── Status (Optional - will be set server-side) ───────────────────────
        /// <summary>
        /// Current status of the invoice (optional, defaults to Draft)
        /// </summary>
        public InvoiceStatus? InvoiceStatus { get; set; }

        /// <summary>
        /// Current payment status (optional, defaults to Pending)
        /// </summary>
        public PaymentStatus? PaymentStatus { get; set; }

        // ── Financial Summary (calculated, not required in DTO) ───────────────
        // Note: Subtotal, DiscountTotal, TaxTotal, ShippingAmount, TotalAmount
        // are calculated on the server side

        // ── Collections ───────────────────────────────────────────────────────
        /// <summary>
        /// Line items for this invoice
        /// </summary>
        [Required(ErrorMessage = "At least one invoice item is required")]
        [MinLength(1, ErrorMessage = "At least one invoice item is required")]
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
    }

    /// <summary>
    /// DTO for invoice line items
    /// </summary>
    public class InvoiceItemDto
    {
        /// <summary>
        /// Existing item ID for updates (optional for new items)
        /// </summary>
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a positive value")]
        public decimal UnitPrice { get; set; }

        [StringLength(50, ErrorMessage = "Tax type cannot exceed 50 characters")]
        public string? TaxType { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public decimal Amount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
        public decimal TaxAmount { get; set; }
    }
    /// <summary>
    /// DTO for invoice tax details (matches InvoiceTaxDetail entity)
    /// </summary>
    public class InvoiceTaxDetailDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tax name is required")]
        [StringLength(50, ErrorMessage = "Tax name cannot exceed 50 characters")]
        public string TaxName { get; set; }


        [Required(ErrorMessage = "Tax rate is required")]
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100%")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Tax amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
        public decimal TaxAmount { get; set; }
    }
    public class InvoiceDiscountDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Discount type is required")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }
    }
    public class TaxTypeDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Rate { get; set; }
    }
    public class TaxTypeCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [Range(0, 100)]
        public decimal Rate { get; set; }
    }
    /// <summary>
    /// DTO for invoice attachments (matches InvoiceAttachment entity)
    /// </summary>
    public class InvoiceAttachmentDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "File name is required")]
        [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters")]
        public string FileName { get; set; }

        /// <summary>
        /// File content for new attachments
        /// </summary>
        public IFormFile? FileContent { get; set; }

        /// <summary>
        /// File URL for existing attachments
        /// </summary>
        [StringLength(500, ErrorMessage = "File URL cannot exceed 500 characters")]
        public string? FileUrl { get; set; }

        /// <summary>
        /// MIME type of the file (e.g., "application/pdf", "image/png")
        /// </summary>
        [StringLength(100, ErrorMessage = "Content type cannot exceed 100 characters")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long? FileSize { get; set; }
    }
}