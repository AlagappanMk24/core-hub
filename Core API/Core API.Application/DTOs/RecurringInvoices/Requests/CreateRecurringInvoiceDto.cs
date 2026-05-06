using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.RecurringInvoice.Request
{
    /// <summary>
    /// DTO for creating a new recurring invoice template
    /// </summary>
    public class CreateRecurringInvoiceDto
    {
        // ── Identity ──────────────────────────────────────────────────────────

        /// <summary>
        /// Display name for this recurring template
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Optional description providing context about the purpose of this recurring schedule
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        // ── Parties ───────────────────────────────────────────────────────────

        /// <summary>
        /// ID of the customer being billed
        /// </summary>
        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Optional billing address ID (overrides customer's default)
        /// </summary>
        public int? BillingAddressId { get; set; }

        /// <summary>
        /// Optional shipping address ID
        /// </summary>
        public int? ShippingAddressId { get; set; }

        // ── Currency ──────────────────────────────────────────────────────────

        /// <summary>
        /// ISO 4217 three-letter currency code (e.g., "USD", "EUR", "INR")
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Exchange rate relative to base currency
        /// </summary>
        [Range(0.0001, 999999.9999, ErrorMessage = "Currency rate must be positive")]
        public decimal CurrencyRate { get; set; } = 1.0m;

        // ── Reference Numbers ─────────────────────────────────────────────────

        /// <summary>
        /// Customer's purchase order number
        /// </summary>
        [StringLength(50, ErrorMessage = "PO number cannot exceed 50 characters")]
        public string? PONumber { get; set; }

        // ── Frequency & Interval ──────────────────────────────────────────────

        /// <summary>
        /// Base recurrence unit (Daily, Weekly, Monthly, Quarterly, etc.)
        /// </summary>
        [Required(ErrorMessage = "Frequency is required")]
        public RecurringFrequency Frequency { get; set; }

        /// <summary>
        /// Multiplier applied to Frequency (e.g., 2 with Weekly = every 2 weeks)
        /// </summary>
        [Required]
        [Range(1, 365, ErrorMessage = "Frequency interval must be between 1 and 365")]
        public int FrequencyInterval { get; set; } = 1;

        // ── Schedule Specifics ────────────────────────────────────────────────

        /// <summary>
        /// Day of the month (1-31) for monthly schedules
        /// </summary>
        [Range(1, 31, ErrorMessage = "Day of month must be between 1 and 31")]
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// Day of the week for weekly schedules
        /// </summary>
        public DayOfWeek? DayOfWeek { get; set; }

        /// <summary>
        /// Ordinal week of the month (1-5) for "first Monday" style schedules
        /// </summary>
        [Range(1, 5, ErrorMessage = "Week of month must be between 1 and 5")]
        public int? WeekOfMonth { get; set; }

        /// <summary>
        /// Month of the year (1-12) for annual schedules
        /// </summary>
        [Range(1, 12, ErrorMessage = "Month of year must be between 1 and 12")]
        public int? MonthOfYear { get; set; }

        // ── Lifecycle Dates ───────────────────────────────────────────────────

        /// <summary>
        /// Date from which the recurring schedule becomes active
        /// </summary>
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date after which no further invoices are generated (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }

        // ── Occurrence Limits ─────────────────────────────────────────────────

        /// <summary>
        /// Maximum number of invoices to generate (null for unlimited)
        /// </summary>
        [Range(1, 9999, ErrorMessage = "Max occurrences must be between 1 and 9999")]
        public int? MaxOccurrences { get; set; }

        // ── Generation Settings ───────────────────────────────────────────────

        /// <summary>
        /// Number of days before due date to generate the invoice (0 = on due date)
        /// </summary>
        [Range(0, 90, ErrorMessage = "Generate in advance days must be between 0 and 90")]
        public int GenerateInAdvanceDays { get; set; } = 0;

        // ── Automation Flags ──────────────────────────────────────────────────

        /// <summary>
        /// Automatically send generated invoices to customer
        /// </summary>
        public bool AutoSend { get; set; } = false;

        /// <summary>
        /// Send email notification when invoice is generated
        /// </summary>
        public bool AutoEmail { get; set; } = false;

        /// <summary>
        /// Automatically charge customer's saved payment method
        /// </summary>
        public bool AutoCharge { get; set; } = false;

        /// <summary>
        /// Send payment reminder before due date
        /// </summary>
        public bool ReminderBeforeDue { get; set; } = false;

        /// <summary>
        /// Days before due date to send reminder (only if ReminderBeforeDue is true)
        /// </summary>
        [Range(1, 30, ErrorMessage = "Reminder days before must be between 1 and 30")]
        public int ReminderDaysBefore { get; set; } = 3;

        // ── Source Template ───────────────────────────────────────────────────

        /// <summary>
        /// ID of existing invoice to use as template (optional)
        /// </summary>
        public int? SourceInvoiceId { get; set; }

        // ── Template Overrides ────────────────────────────────────────────────

        /// <summary>
        /// Override the purchase order number on generated invoices
        /// </summary>
        [StringLength(50)]
        public string? OverridePONumber { get; set; }

        /// <summary>
        /// Override the customer-facing notes on generated invoices
        /// </summary>
        [StringLength(1000)]
        public string? OverrideCustomerNotes { get; set; }

        /// <summary>
        /// Override the terms and conditions on generated invoices
        /// </summary>
        [StringLength(500)]
        public string? OverrideTermsAndConditions { get; set; }

        /// <summary>
        /// Override the footer note on generated invoices
        /// </summary>
        [StringLength(500)]
        public string? OverrideFooterNote { get; set; }

        /// <summary>
        /// Override the project detail on generated invoices
        /// </summary>
        [StringLength(200)]
        public string? OverrideProjectDetail { get; set; }

        /// <summary>
        /// Override the payment method on generated invoices
        /// </summary>
        [StringLength(100)]
        public string? OverridePaymentMethod { get; set; }

        /// <summary>
        /// Override the payment terms (in days) on generated invoices
        /// </summary>
        [Range(0, 365, ErrorMessage = "Payment terms must be between 0 and 365 days")]
        public int? OverridePaymentTerms { get; set; }

        /// <summary>
        /// Override the shipping amount on generated invoices
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Shipping amount must be positive")]
        public decimal? OverrideShippingAmount { get; set; }

        /// <summary>
        /// Override the adjustment amount on generated invoices
        /// </summary>
        public decimal? OverrideAdjustmentAmount { get; set; }

        /// <summary>
        /// Override the adjustment description on generated invoices
        /// </summary>
        [StringLength(200)]
        public string? OverrideAdjustmentDescription { get; set; }

        // ── Notes & Terms (InvoiceHeaderBase fields) ──────────────────────────

        /// <summary>
        /// Default customer notes for generated invoices
        /// </summary>
        [StringLength(1000)]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Internal notes visible only to staff
        /// </summary>
        [StringLength(1000)]
        public string? InternalNotes { get; set; }

        /// <summary>
        /// Default terms and conditions for generated invoices
        /// </summary>
        [StringLength(500)]
        public string? TermsAndConditions { get; set; }

        /// <summary>
        /// Default footer note for generated invoices
        /// </summary>
        [StringLength(500)]
        public string? FooterNote { get; set; }

        /// <summary>
        /// Default project detail for generated invoices
        /// </summary>
        [StringLength(200)]
        public string? ProjectDetail { get; set; }

        /// <summary>
        /// Default payment method for generated invoices
        /// </summary>
        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Default payment terms for generated invoices (e.g., "Net 30")
        /// </summary>
        [StringLength(100)]
        public string? PaymentTerms { get; set; }
    }
}
