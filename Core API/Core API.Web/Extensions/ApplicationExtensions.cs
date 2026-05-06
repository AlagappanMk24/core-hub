using Core_API.Application.DependencyInjection;
using Core_API.Infrastructure.DependencyInjection;

namespace Core_API.Web.Extensions
{
    /// <summary>
    /// Provides extension methods for registering application and infrastructure layer dependencies.
    /// </summary>
    /// <remarks>
    /// This class centralizes the registration of all dependencies from the Application and Infrastructure layers,
    /// ensuring proper separation of concerns and maintainable dependency injection configuration.
    /// </remarks>
    public static class ApplicationExtensions
    {
        #region Public Methods

        /// <summary>
        /// Registers application and infrastructure layer dependencies with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// This method registers the following:
        /// <list type="bullet">
        /// <item><description>Application layer dependencies: MediatR, AutoMapper, FluentValidation</description></item>
        /// <item><description>Infrastructure layer dependencies: Repositories, Unit of Work, CurrentUserService, etc.</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var builder = WebApplication.CreateBuilder(args);
        /// builder.Services.AddApplicationAndInfrastructure();
        /// </code>
        /// </example>
        public static IServiceCollection AddApplicationAndInfrastructure(this IServiceCollection services)
        {
            #region Application Layer Registration

            // Registers MediatR for CQRS pattern, AutoMapper for object mapping, and FluentValidation for request validation
            services.AddApplicationDependencies();

            #endregion

            #region Infrastructure Layer Registration

            // Registers repositories, unit of work, background services, and infrastructure-level services
            services.AddInfrastructureDependencies();

            #endregion

            return services;
        }
        #endregion
    }
}