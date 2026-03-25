using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Abstract
{
    /// <summary>
    /// Shared header fields inherited by both one-time <c>Invoice</c> and
    /// recurring <c>RecurringInvoice</c> templates.
    /// Captures customer/company references, currency, financial totals,
    /// and document metadata in a single reusable base.
    /// </summary>
    public abstract class InvoiceHeaderBase : BaseEntity
    {
        // ── Parties ───────────────────────────────────────────────────────────
        /// <summary>
        /// Foreign key to the customer being billed.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Navigation property to the billing customer.
        /// </summary>
        public Customer? Customer { get; set; }

        /// <summary>
        /// Foreign key to the company issuing the invoice.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Navigation property to the issuing company.
        /// </summary>
        public Company? Company { get; set; }

        // ── Addresses ─────────────────────────────────────────────────────────

        /// <summary>
        /// Foreign key to the address used for billing purposes.
        /// May differ from the customer's primary address.
        /// </summary>
        public int? BillingAddressId { get; set; }

        /// <summary>
        /// Foreign key to the address used for shipping or service delivery.
        /// </summary>
        public int? ShippingAddressId { get; set; }

        // ── Currency ──────────────────────────────────────────────────────────

        /// <summary>
        /// ISO 4217 three-letter currency code for all monetary values on this document
        /// (e.g., "INR", "USD", "EUR"). Defaults to "INR".
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "INR";

        /// <summary>
        /// Exchange rate relative to the system's base currency at the time of issue.
        /// Defaults to 1 for single-currency setups.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyRate { get; set; } = 1;

        // ── Reference Numbers ─────────────────────────────────────────────────

        /// <summary>
        /// Customer's purchase order number, used for cross-referencing with their procurement system.
        /// </summary>
        [StringLength(50)]
        public string PONumber { get; set; }


        // ── Financial Summary ─────────────────────────────────────────────────

        /// <summary>
        /// Sum of all line item amounts before discounts, taxes, and shipping.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total discount amount deducted from the subtotal.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountTotal { get; set; }

        /// <summary>
        /// Total tax amount calculated across all applicable tax rules.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxTotal { get; set; }

        /// <summary>
        /// Shipping or freight charge added to the invoice.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingAmount { get; set; }

        /// <summary>
        /// Grand total payable by the customer
        /// (Subtotal − DiscountTotal + TaxTotal + ShippingAmount + Adjustments).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // ── Notes & Terms ─────────────────────────────────────────────────────

        /// <summary>
        /// Notes visible to the customer on the invoice (e.g., "Thank you for your business").
        /// </summary>
        [StringLength(1000)]
        public string CustomerNotes { get; set; }

        /// <summary>
        /// Internal notes visible only to staff; not printed on the customer-facing invoice.
        /// </summary>
        [StringLength(1000)]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Legal or contractual terms and conditions printed at the bottom of the invoice.
        /// </summary>
        [StringLength(500)]
        public string TermsAndConditions { get; set; }

        /// <summary>
        /// Short footer message displayed at the bottom of the printed or emailed invoice.
        /// </summary>
        [StringLength(500)]
        public string FooterNote { get; set; }

        /// <summary>
        /// Free-text field for referencing a project, milestone, or work order associated
        /// with this invoice.
        /// </summary>
        public string ProjectDetail { get; set; }

        // ── Payment Terms ─────────────────────────────────────────────────────

        /// <summary>
        /// Preferred payment method for settling this invoice (e.g., "Bank Transfer", "Credit Card").
        /// </summary>
        [StringLength(100)]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Agreed payment terms dictating when payment is due (e.g., "Net 30", "Due on Receipt").
        /// </summary>
        [StringLength(100)]
        public string PaymentTerms { get; set; }
    }
}