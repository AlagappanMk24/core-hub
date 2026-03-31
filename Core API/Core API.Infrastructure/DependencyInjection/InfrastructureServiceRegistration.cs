using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.Contracts.Services.File.Excel;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Infrastructure.BackgroundServices;
using Core_API.Infrastructure.Data.Initializers;
using Core_API.Infrastructure.Persistence;
using Core_API.Infrastructure.Service;
using Core_API.Infrastructure.Services;
using Core_API.Infrastructure.Services.Admin;
using Core_API.Infrastructure.Services.Authentication;
using Core_API.Infrastructure.Services.Authorization;
using Core_API.Infrastructure.Services.Dashboard;
using Core_API.Infrastructure.Services.File.Excel;
using Core_API.Infrastructure.Services.File.Pdf;
using Microsoft.Extensions.DependencyInjection;

namespace Core_API.Infrastructure.DI
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            // Register memory cache 
            services.AddMemoryCache();

            // Register foundational services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbInitializer, DbInitializer>();

            // Register security and authentication services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IPermissionService, PermissionService>();

            services.AddScoped<IDashboardService, DashboardService>();

            // Register core business services
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IRecurringInvoiceService, RecurringInvoiceService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<ICompanyRequestService, CompanyRequestService>();

            // Register utility and helper services
            services.AddScoped<IEmailSendingService, EmailSendingService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IContactUsService, ContactUsService>();

            // Register state management services
            services.AddScoped<IAuthStateService, AuthStateService>();

            services.AddScoped<IExchangeRateService, ExchangeRateService>();

            // Cache Service
            services.AddScoped<ICacheService, MemoryCacheService>();

            //services.AddScoped<IProductService, ProductService>();
            //services.AddScoped<ICartService, CartService>();
            //services.AddScoped<ISmsSender, TwilioService>();
            services.AddHostedService<RecurringInvoiceBackgroundService>();

            return services;
        }
    }
}
