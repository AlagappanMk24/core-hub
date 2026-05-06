using Core_API.Domain.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.RecurringInvoices
{
    /// <summary>
    /// Immutable audit log entry for a recurring invoice template,
    /// recording every configuration change or lifecycle event
    /// for compliance and traceability.
    /// </summary>
    public class RecurringInvoiceAuditLog : AuditLogBase
    {
        /// <summary>
        /// Foreign key to the recurring invoice template this log entry belongs to.
        /// </summary>
        [Required]
        public int RecurringInvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="RecurringInvoice"/> template.
        /// </summary>
        [ForeignKey("RecurringInvoiceId")]
        public RecurringInvoice RecurringInvoice { get; set; }
    }
}
