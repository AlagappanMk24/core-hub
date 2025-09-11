using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure
{
    [Authorize(Policy = "Customer")]
    public class NotificationHub(ILogger<NotificationHub> logger) : Hub
    {
        private readonly ILogger<NotificationHub> _logger = logger;
        public override async Task OnConnectedAsync()
        {
            var customerId = Context.User?.FindFirst("customerId")?.Value;
            if (!string.IsNullOrEmpty(customerId) && int.TryParse(customerId, out var parsedCustomerId))
            {
                _logger.LogInformation("Client connected to NotificationHub with ConnectionId {ConnectionId} and CustomerId {CustomerId}", Context.ConnectionId, customerId);
                await Groups.AddToGroupAsync(Context.ConnectionId, parsedCustomerId.ToString());
            }
            else
            {
                _logger.LogWarning("No valid customerId found for ConnectionId {ConnectionId}", Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var customerId = Context.User?.FindFirst("customerId")?.Value;
            if (!string.IsNullOrEmpty(customerId) && int.TryParse(customerId, out var parsedCustomerId))
            {
                _logger.LogInformation("Client disconnected from NotificationHub with ConnectionId {ConnectionId} and CustomerId {CustomerId}", Context.ConnectionId, customerId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, parsedCustomerId.ToString());
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
