namespace Core_API.Application.DTOs.RecurringInvoice.Response
{
    /// <summary>
    /// DTO for a single generated invoice instance from a recurring template
    /// </summary>
    public class RecurringInvoiceInstanceDto
    {
        /// <summary>ID of the instance record</summary>
        public int Id { get; set; }

        /// <summary>ID of the generated invoice</summary>
        public int InvoiceId { get; set; }

        /// <summary>Invoice number of the generated invoice</summary>
        public string InvoiceNumber { get; set; }

        /// <summary>Date when the invoice was actually generated</summary>
        public DateTime GeneratedDate { get; set; }

        /// <summary>Date when generation was originally scheduled</summary>
        public DateTime ScheduledGenerationDate { get; set; }

        /// <summary>Sequence number indicating which occurrence in the series</summary>
        public int SequenceNumber { get; set; }

        /// <summary>Total monetary amount of the generated invoice</summary>
        public decimal Amount { get; set; }

        /// <summary>Current status of the generated invoice</summary>
        public string InvoiceStatus { get; set; }

        /// <summary>Current payment status of the generated invoice</summary>
        public string PaymentStatus { get; set; }

        /// <summary>Issue date of the generated invoice</summary>
        public DateTime? IssueDate { get; set; }

        /// <summary>Due date of the generated invoice</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>Outcome of the generation attempt</summary>
        public string GenerationStatus { get; set; }

        /// <summary>Error message if generation failed</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Number of retry attempts</summary>
        public int RetryCount { get; set; }

        /// <summary>Optional notes recorded at generation time</summary>
        public string? Notes { get; set; }
    }
}
