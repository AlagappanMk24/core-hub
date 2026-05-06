using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.RecurringInvoice.Request
{
    /// <summary>
    /// DTO for manually generating an invoice from a recurring template
    /// </summary>
    public class GenerateManualDto
    {
        /// <summary>
        /// ID of the recurring invoice template to generate from
        /// </summary>
        [Required(ErrorMessage = "Recurring invoice ID is required")]
        public int RecurringInvoiceId { get; set; }

        /// <summary>
        /// Optional invoice date (defaults to today)
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Optional due date (calculated from payment terms if not provided)
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Whether to send the invoice immediately after generation
        /// </summary>
        public bool SendImmediately { get; set; } = false;

        /// <summary>
        /// Whether to update the next invoice date after generation
        /// </summary>
        public bool OverrideNextDate { get; set; } = true;

        /// <summary>
        /// Optional notes to record with this generation
        /// </summary>
        [StringLength(500)]
        public string? GenerationNotes { get; set; }
    }
}