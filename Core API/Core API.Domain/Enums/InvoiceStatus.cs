namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the status of an invoice.
    /// </summary>
    public enum InvoiceStatus
    {
        Draft = 0,    // Invoice being prepared
        Sent = 1,     // Invoice sent to customer
        Approved = 2, // Invoice approved by customer
        Cancelled = 3 // Invoice voided
    }
}
