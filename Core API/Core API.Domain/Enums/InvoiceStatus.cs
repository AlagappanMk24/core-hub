namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a single issued invoice.
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>Invoice created but not yet sent to the customer.</summary>
        Draft = 0,

        /// <summary>Invoice has been delivered to the customer and is awaiting payment.</summary>
        Sent = 1,

        /// <summary>Customer has opened and viewed the invoice.</summary>
        Viewed = 2,

        /// <summary>One or more payments received, but the full amount is not yet settled.</summary>
        PartiallyPaid = 3,

        /// <summary>Invoice has been paid in full.</summary>
        Paid = 4,

        /// <summary>Due date has passed without full payment being received.</summary>
        Overdue = 5,

        /// <summary>Invoice has been cancelled; no longer collectible.</summary>
        Void = 6,

        /// <summary>Outstanding balance has been written off as irrecoverable bad debt.</summary>
        WriteOff = 7,

        /// <summary>A credit note has been issued against this invoice.</summary>
        CreditNote = 8,

        /// <summary>The full amount paid has been refunded to the customer.</summary>
        Refunded = 9
    }
}
