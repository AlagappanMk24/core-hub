namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the payment collection status of an invoice or transaction.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>No payment has been received yet.</summary>
        Pending = 0,

        /// <summary>A partial payment has been received; balance still outstanding.</summary>
        PartiallyPaid = 1,

        /// <summary>The full invoiced amount has been collected.</summary>
        Paid = 2,

        /// <summary>Payment is past the due date and has not been fully received.</summary>
        Overdue = 3,

        /// <summary>The full payment has been returned to the customer.</summary>
        Refunded = 4,

        /// <summary>A portion of the payment has been returned; partial balance remains.</summary>
        PartiallyRefunded = 5,

        /// <summary>A payment attempt was made but was declined or rejected.</summary>
        Failed = 6,

        /// <summary>The payment was cancelled before processing completed.</summary>
        Cancelled = 7
    }
}