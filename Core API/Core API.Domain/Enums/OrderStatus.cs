namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the fulfilment lifecycle status of a sales order.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>Order has been placed but not yet reviewed or confirmed.</summary>
        Pending,

        /// <summary>Order has been reviewed and approved for processing.</summary>
        Approved,

        /// <summary>Order is actively being prepared or assembled.</summary>
        Processing,

        /// <summary>Order has been dispatched and is in transit to the customer.</summary>
        Shipped,

        /// <summary>Order was cancelled before fulfilment completed.</summary>
        Cancelled,

        /// <summary>Payment for the order has been refunded to the customer.</summary>
        Refunded,

        /// <summary>Order has been successfully delivered to the customer.</summary>
        Delivered
    }
}
