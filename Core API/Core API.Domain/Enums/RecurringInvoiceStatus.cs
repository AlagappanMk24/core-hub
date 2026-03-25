namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the operational lifecycle status of a recurring invoice template.
    /// </summary>
    public enum RecurringInvoiceStatus
    {
        /// <summary>Template is being configured; no invoices generated yet.</summary>
        Draft = 0,

        /// <summary>Template is live and actively generating invoices on schedule.</summary>
        Active = 1,

        /// <summary>Generation has been temporarily suspended; can be resumed.</summary>
        Paused = 2,

        /// <summary>All configured occurrences have been generated successfully.</summary>
        Completed = 3,

        /// <summary>Template has been permanently stopped; no further invoices will be generated.</summary>
        Cancelled = 4,

        /// <summary>The configured end date has passed; the schedule has naturally lapsed.</summary>
        Expired = 5
    }
}