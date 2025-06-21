using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Application.Contracts.Services;
using Core_API.Infrastructure.Data.Initializers;
using Core_API.Infrastructure.External.SMS;
using Core_API.Infrastructure.Persistence;
using Core_API.Infrastructure.Service;
using Core_API.Infrastructure.Service.Email;
using Core_API.Infrastructure.Services;
using Core_API.Infrastructure.Services.Admin;
using Core_API.Infrastructure.Services.Authentication;
using Core_API.Infrastructure.Services.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Core_API.Infrastructure.DI
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            // Register Unit Of Work and Generic Repository and Services
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IContactUsService, ContactUsService>();
            //services.AddScoped<ISmsSender, TwilioService>();
            services.AddScoped<IAuthStateService, AuthStateService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IRolesService, RolesService>();

            return services;
        }
    }
}
