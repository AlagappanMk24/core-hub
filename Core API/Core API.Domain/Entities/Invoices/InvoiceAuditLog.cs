using Core_API.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Invoices
{
    /// <summary>
    /// Immutable audit log entry recording every significant action
    /// performed on an invoice for compliance and traceability.
    /// </summary>
    public class InvoiceAuditLog : BaseEntity
    {
        // ── Invoice Reference ─────────────────────────────────────────────────

        /// <summary>
        /// Foreign key to the invoice this log entry belongs to.
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="Invoice"/>.
        /// </summary>
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        // ── Log Details ───────────────────────────────────────────────────────

        /// <summary>
        /// Short label for the action performed (e.g., "Created", "Sent", "Paid", "Voided").
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        /// <summary>
        /// Human-readable description of what happened and why.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// JSON-serialised snapshot of field-level changes made during this action.
        /// </summary>
        [StringLength(2000)]
        public string Changes { get; set; }

        // ── Request Context ───────────────────────────────────────────────────

        /// <summary>
        /// IP address of the client that triggered this action, for security auditing.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User-agent string of the client browser or API consumer.
        /// </summary>
        public string UserAgent { get; set; }
    }
}
