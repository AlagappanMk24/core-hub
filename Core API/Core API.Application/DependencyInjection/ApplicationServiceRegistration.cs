using AutoMapper;
using Core_API.Application.Common.Behaviours;
using Core_API.Application.Common.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Core_API.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ContextInjectionBehavior<,>));
            });

            // Register FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register AutoMapper with proper configuration
            services.AddAutoMapper((serviceProvider, config) =>
            {
                // Add all profiles from the executing assembly
                config.AddMaps(Assembly.GetExecutingAssembly());

                // Optional: Configure AutoMapper settings
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;
            
            }, Assembly.GetExecutingAssembly());

            return services;
        }
    }
}