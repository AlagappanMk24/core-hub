namespace Core_API.Application.Common.Events
{
    /// <summary>
    /// Interface for invoice notification handlers (implemented in Infrastructure)
    /// </summary>
    public interface IInvoiceNotificationHandler
    {
        Task HandleInvoiceCreated(InvoiceCreatedEvent @event);
    }
}