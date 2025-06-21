namespace Core_API.Domain.Entities
{
    public class OrderActivityLog
    {
        public int Id { get; set; } // Primary key
        public int OrderHeaderId { get; set; } // Foreign key to OrderHeader
        public DateTime Timestamp { get; set; } // When the action occurred
        public string? User { get; set; } // Who performed the action
        public ActivityType ActivityType { get; set; }
        public string? Description { get; set; } // Description of the action
        public string? Details { get; set; } // Optional extra details
    }
    public enum ActivityType
    {
        OrderCreated,
        OrderUpdated,
        StatusChanged,
        PaymentProcessed,
        PaymentFailed,
        ShippingUpdated,
        TrackingAdded,
        OrderCanceled,
        NoteAdded,
        InvoiceGenerated,
        SystemEvent // For automated or system-triggered actions
    }
}
