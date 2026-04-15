using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.Contracts.Services.File.Excel;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Application.Contracts.Services.Invoice;
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
using Core_API.Infrastructure.Services.Invoice;
using Microsoft.Extensions.DependencyInjection;

namespace Core_API.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Centralized registration for Infrastructure layer dependencies.
    /// Organized by functional domain to ensure maintainability.
    /// </summary>
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            // Core .NET Services - Register memory cache 
            services.AddMemoryCache();

            // Register by Functional Areas
            services.AddPersistenceServices();
            services.AddSecurityServices();
            services.AddBusinessServices();
            services.AddInvoiceModuleServices();
            services.AddUtilityServices();
            services.AddBackgroundServices();

            return services;
        }

        private static void AddPersistenceServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbInitializer, DbInitializer>();
        }
        private static void AddSecurityServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IAuthStateService, AuthStateService>();
        }
        private static void AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICompanyRequestService, CompanyRequestService>();
            services.AddScoped<ITaskService, TaskService>();
        }

        private static void AddInvoiceModuleServices(this IServiceCollection services)
        {
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IInvoiceNumberService, InvoiceNumberService>();
            services.AddScoped<IInvoiceCalculationService, InvoiceCalculationService>();
            services.AddScoped<IInvoiceEmailService, InvoiceEmailService>();
            services.AddScoped<IInvoiceStatisticsService, InvoiceStatisticsService>();
            services.AddScoped<IInvoiceDuplicationService, InvoiceDuplicationService>();
            services.AddScoped<IInvoiceAttachmentService, InvoiceAttachmentService>();
            services.AddScoped<IRecurringInvoiceService, RecurringInvoiceService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<ICustomerInvoiceService, CustomerInvoiceService>();
            services.AddScoped<IInvoiceSettingsService, InvoiceSettingsService>();
        }

        private static void AddUtilityServices(this IServiceCollection services)
        {
            // Email and Files
            services.AddScoped<IEmailSendingService, EmailSendingService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IExcelService, ExcelService>();

            // Integrations and Cache
            services.AddScoped<IContactUsService, ContactUsService>();
            services.AddScoped<IExchangeRateService, ExchangeRateService>();
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        private static void AddBackgroundServices(this IServiceCollection services)
        {
            // Hosted services are Singleton by nature
            services.AddHostedService<RecurringInvoiceBackgroundService>();
        }
    }
}