using Core_API.Domain.Common;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.RecurringInvoices
{
    /// <summary>
    /// Tracks a single generation event — the link between a
    /// <see cref="RecurringInvoice"/> template and the <see cref="Invoice"/>
    /// it produced on a particular run.
    /// </summary>
    public class RecurringInvoiceInstance : BaseEntity
    {
        // ── References ────────────────────────────────────────────────────────

        /// <summary>
        /// Foreign key to the parent recurring template that triggered this generation.
        /// </summary>
        [Required]
        public int RecurringInvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="RecurringInvoice"/> template.
        /// </summary>
        [ForeignKey("RecurringInvoiceId")]
        public RecurringInvoice RecurringInvoice { get; set; }

        /// <summary>
        /// Foreign key to the <see cref="Invoice"/> produced during this generation run.
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the generated <see cref="Invoice"/>.
        /// </summary>
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        // ── Generation Details ────────────────────────────────────────────────

        /// <summary>
        /// Actual date and time the invoice was generated.
        /// </summary>
        [Required]
        public DateTime GeneratedDate { get; set; }

        /// <summary>
        /// The date on which generation was originally scheduled, before any delays or retries.
        /// </summary>
        public DateTime ScheduledGenerationDate { get; set; }

        /// <summary>
        /// One-based counter indicating which occurrence in the series this instance represents.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Invoice number assigned to the generated invoice (e.g., "INV-2024-0042").
        /// </summary>
        [Required]
        [StringLength(50)]
        public string GeneratedInvoiceNumber { get; set; }

        /// <summary>
        /// Total monetary amount of the generated invoice.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Optional notes recorded at the time of generation (e.g., manual overrides applied).
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        // ── Generation Status ─────────────────────────────────────────────────

        /// <summary>
        /// Outcome of the generation attempt (Success, Failed, RetryPending, Skipped).
        /// </summary>
        public GenerationStatus GenerationStatus { get; set; } = GenerationStatus.Success;

        /// <summary>
        /// Descriptive error message captured when <see cref="GenerationStatus"/> is Failed.
        /// Null on successful runs.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of times generation has been retried after an initial failure.
        /// </summary>
        public int RetryCount { get; set; } = 0;
    }
}
