using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core_API.Application.DI
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            // Configuration of AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
