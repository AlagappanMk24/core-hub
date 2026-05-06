using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.RecurringInvoice.Request
{
    /// <summary>
    /// DTO for updating an existing recurring invoice template
    /// </summary>
    public class UpdateRecurringInvoiceDto : CreateRecurringInvoiceDto
    {
        /// <summary>
        /// ID of the recurring invoice template to update
        /// </summary>
        [Required(ErrorMessage = "Recurring invoice ID is required")]
        public int Id { get; set; }

        /// <summary>
        /// Current operational status of the recurring template
        /// </summary>
        [Required(ErrorMessage = "Status is required")]
        public RecurringInvoiceStatus Status { get; set; }

        /// <summary>
        /// Date and time when this schedule was paused (if applicable)
        /// </summary>
        public DateTime? PausedDate { get; set; }

        /// <summary>
        /// Date and time when this schedule was cancelled (if applicable)
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Running count of invoices successfully generated (read-only in update)
        /// </summary>
        public int OccurrencesGenerated { get; set; }

        /// <summary>
        /// Date of the most recent invoice generated (read-only in update)
        /// </summary>
        public DateTime? LastInvoiceDate { get; set; }
    }
}
