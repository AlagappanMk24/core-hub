using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Events;
using Core_API.Infrastructure.RealTime.Hubs;

namespace Core_API.Infrastructure.Events;

/// <summary>
/// Handles invoice notifications and sends SignalR updates
/// </summary>
public class InvoiceNotificationHandler(
    IHubContext<NotificationHub> hubContext,
    ILogger<InvoiceNotificationHandler> logger) : INotificationHandler<InvoiceCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ILogger<InvoiceNotificationHandler> _logger = logger;

    public async Task Handle(InvoiceCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var customerGroupId = notification.CustomerId.ToString();

            await _hubContext.Clients.Group(customerGroupId).SendAsync(
                "ReceiveInvoiceNotification",
                new
                {
                    notification.InvoiceId,
                    notification.InvoiceNumber,
                    notification.Currency,
                    Message = $"New invoice {notification.InvoiceNumber} has been created",
                    Amount = notification.TotalAmount,
                    DueDate = notification.DueDate.ToString("dd MMM yyyy"),
                    notification.CustomerName,
                    notification.Timestamp
                },
                cancellationToken);

            _logger.LogInformation("Invoice notification sent to group {GroupId} for invoice {InvoiceNumber}",
                customerGroupId, notification.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification for invoice {InvoiceNumber}", notification.InvoiceNumber);
        }
    }
}