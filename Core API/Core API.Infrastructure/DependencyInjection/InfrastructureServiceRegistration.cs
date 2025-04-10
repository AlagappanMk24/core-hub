using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Infrastructure.Persistence.UnitOfWork;
using Core_API.Infrastructure.Service;
using Core_API.Infrastructure.Service.Email;
using Microsoft.Extensions.DependencyInjection;

namespace Core_API.Infrastructure.DI
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            // Register Unit Of Work and Generic Repository and Services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}
