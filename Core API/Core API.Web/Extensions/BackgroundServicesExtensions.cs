using Core_API.Infrastructure.Services.Background;

namespace Core_API.Web.Extensions
{
    /// <summary>
    /// Provides extension methods for registering background services (hosted services).
    /// </summary>
    /// <remarks>
    /// Background services run as long-running tasks in the background and are ideal for:
    /// - Scheduled jobs
    /// - Queue processing
    /// - Cleanup operations
    /// - Long-running operations that shouldn't block HTTP requests
    /// </remarks>
    public static class BackgroundServicesExtensions
    {
        #region Public Methods

        /// <summary>
        /// Registers background services (hosted services) with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// The following background services are registered:
        /// <list type="bullet">
        /// <item><description><see cref="UserCleanupService"/> - Periodically removes inactive or expired user accounts</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var builder = WebApplication.CreateBuilder(args);
        /// builder.Services.AddBackgroundServices();
        /// </code>
        /// </example>
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            #region Hosted Services Registration

            // Add UserCleanupService as a hosted background service
            // This service will run in the background and perform periodic user cleanup operations
            services.AddHostedService<UserCleanupService>();

            #endregion

            return services;
        }
        #endregion
    }
}
