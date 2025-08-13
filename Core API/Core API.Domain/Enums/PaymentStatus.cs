namespace Core_API.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,    // Payment expected but not initiated
        Processing = 1, // Payment initiated, awaiting confirmation
        Completed = 2,  // Payment fully received
        PartiallyPaid = 3, // Partial payment received
        Overdue = 4,    // Payment past due
        Failed = 5,     // Payment attempt failed
        Refunded = 6    // Payment was refunded
    }
}