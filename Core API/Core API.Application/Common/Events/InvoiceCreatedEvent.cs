using MediatR;

namespace Core_API.Application.Common.Events
{
    /// <summary>
    /// Domain event fired when an invoice is successfully created
    /// </summary>
    public class InvoiceCreatedEvent : INotification
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}