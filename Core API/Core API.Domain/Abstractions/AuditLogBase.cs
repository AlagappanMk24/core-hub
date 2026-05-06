using Core_API.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Abstractions
{
    /// <summary>
    /// Shared audit log fields reused by both <c>InvoiceAuditLog</c> and
    /// <c>RecurringInvoiceAuditLog</c> to record who did what and when
    /// on any invoice-related entity.
    /// </summary>
    public abstract class AuditLogBase : BaseEntity
    {
        /// <summary>
        /// Short label identifying the action performed
        /// (e.g., "Created", "Sent", "Generated", "Voided").
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        /// <summary>
        /// Human-readable summary of what changed and the context behind the action.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// JSON-serialised snapshot of the specific field values that were modified.
        /// </summary>
        [StringLength(2000)]
        public string Changes { get; set; } // JSON of changes made

        // ── Request Context ───────────────────────────────────────────────────

        /// <summary>
        /// IP address of the client that triggered the action, used for security auditing.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User-agent string of the client browser or API consumer that made the request.
        /// </summary>
        public string UserAgent { get; set; }
    }
}