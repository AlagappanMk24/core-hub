using Core_API.Application.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Background
{
    public class UserCleanupService(IServiceProvider serviceProvider, ILogger<UserCleanupService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<UserCleanupService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting user cleanup at {Time}", DateTime.UtcNow);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        await userService.CleanupSoftDeletedUsersAsync();
                    }

                    _logger.LogInformation("User cleanup completed. Next run in 24 hours.");
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Run daily For Production

                    //_logger.LogInformation("User cleanup completed. Next run in 10 minutes.");
                    //await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Run every 10 mins for testing
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during user cleanup");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Retry after 5 minutes on error
                }
            }
        }
    }
}
