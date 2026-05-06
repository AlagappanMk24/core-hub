using Core_API.Domain.Common;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Invoices
{
    /// <summary>
    /// Represents a payment made against an invoice
    /// </summary>
    public class InvoicePayment : BaseEntity
    {
        // ── Identity & References ──────────────────────────────────────────

        /// <summary>
        /// Unique payment number (e.g., PAY-2024-0001)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PaymentNumber { get; set; }

        /// <summary>
        /// Foreign key to the invoice being paid
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Foreign key to the customer making the payment
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        /// <summary>
        /// Foreign key to the company receiving the payment
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        // ── Payment Details ────────────────────────────────────────────────

        /// <summary>
        /// Amount paid
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment date (when the payment was processed)
        /// </summary>
        [Required]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Payment method (Credit Card, Bank Transfer, PayPal, Cash, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Payment status (Pending, Completed, Failed, Refunded, Cancelled)
        /// </summary>
        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// Payment type (Full, Partial, Advance, Deposit, Final)
        /// </summary>
        [Required]
        public PaymentType PaymentType { get; set; }

        // ── Transaction Details ───────────────────────────────────────────

        /// <summary>
        /// Transaction ID from payment gateway (Stripe, PayPal, etc.)
        /// </summary>
        [StringLength(200)]
        public string TransactionId { get; set; }

        /// <summary>
        /// Reference number from bank or payment processor
        /// </summary>
        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Check number (for check payments)
        /// </summary>
        [StringLength(50)]
        public string CheckNumber { get; set; }

        /// <summary>
        /// Bank name (for bank transfer payments)
        /// </summary>
        [StringLength(100)]
        public string BankName { get; set; }

        /// <summary>
        /// Card last 4 digits (for credit card payments)
        /// </summary>
        [StringLength(4)]
        public string CardLast4 { get; set; }

        /// <summary>
        /// Card brand (Visa, Mastercard, Amex, etc.)
        /// </summary>
        [StringLength(20)]
        public string CardBrand { get; set; }

        // ── Notes & Additional Info ───────────────────────────────────────

        /// <summary>
        /// Customer notes for this payment
        /// </summary>
        [StringLength(500)]
        public string CustomerNotes { get; set; }

        /// <summary>
        /// Internal notes for staff
        /// </summary>
        [StringLength(500)]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Applied payment (how much of this payment was applied to the invoice)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AppliedAmount { get; set; }

        /// <summary>
        /// Unapplied amount (excess payment that can be applied to other invoices)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnappliedAmount { get; set; }

        /// <summary>
        /// Whether this payment is a refund
        /// </summary>
        public bool IsRefund { get; set; }

        /// <summary>
        /// Original payment ID (for refunds)
        /// </summary>
        public int? OriginalPaymentId { get; set; }

        // ── Audit & Tracking ─────────────────────────────────────────────

        /// <summary>
        /// Who processed the payment
        /// </summary>
        [StringLength(100)]
        public string ProcessedBy { get; set; }

        /// <summary>
        /// Payment gateway response (JSON)
        /// </summary>
        public string GatewayResponse { get; set; }

        /// <summary>
        /// Date when payment was reconciled
        /// </summary>
        public DateTime? ReconciledDate { get; set; }

        /// <summary>
        /// Bank account ID (if deposited to a specific account)
        /// </summary>
        public int? BankAccountId { get; set; }
    }
}
