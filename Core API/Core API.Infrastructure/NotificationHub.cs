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
            try
            {
                var customerId = Context.User?.FindFirst("customerId")?.Value;
                if (!string.IsNullOrEmpty(customerId) && int.TryParse(customerId, out var parsedCustomerId))
                {
                    _logger.LogInformation("Client connected to NotificationHub with ConnectionId {ConnectionId} and CustomerId {CustomerId}", Context.ConnectionId, customerId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, parsedCustomerId.ToString());

                    // Also add to a general "customers" group for broadcast messages
                    //await Groups.AddToGroupAsync(Context.ConnectionId, "customers");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for ConnectionId {ConnectionId}", Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var customerId = Context.User?.FindFirst("customerId")?.Value;
                if (!string.IsNullOrEmpty(customerId) && int.TryParse(customerId, out var parsedCustomerId))
                {
                    _logger.LogInformation("Client disconnected from NotificationHub with ConnectionId {ConnectionId} and CustomerId {CustomerId}", Context.ConnectionId, customerId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, parsedCustomerId.ToString());
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "customers");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for ConnectionId {ConnectionId}", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}