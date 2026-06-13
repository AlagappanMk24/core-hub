using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Account;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.Contracts.Services.Cache;
using Core_API.Application.Contracts.Services.Common;
using Core_API.Application.Contracts.Services.Companies;
using Core_API.Application.Contracts.Services.Contact;
using Core_API.Application.Contracts.Services.Customers;
using Core_API.Application.Contracts.Services.Dashboard;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.Contracts.Services.Exchange;
using Core_API.Application.Contracts.Services.Files;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.Contracts.Services.Invoices;
using Core_API.Application.Contracts.Services.RecurringInvoices;
using Core_API.Application.Contracts.Services.Tasks;
using Core_API.Application.Contracts.Services.Taxes;
using Core_API.Application.Contracts.Services.Users;
using Core_API.Infrastructure.Configuration.Settings;
using Core_API.Infrastructure.Seed;
using Core_API.Infrastructure.Services.Account;
using Core_API.Infrastructure.Services.Admin;
using Core_API.Infrastructure.Services.Authentication;
using Core_API.Infrastructure.Services.Authorization;
using Core_API.Infrastructure.Services.Background.RecurringInvoice;
using Core_API.Infrastructure.Services.Common;
using Core_API.Infrastructure.Services.Communication.Contact;
using Core_API.Infrastructure.Services.Communication.Email;
using Core_API.Infrastructure.Services.Company;
using Core_API.Infrastructure.Services.Customer;
using Core_API.Infrastructure.Services.Dashboard;
using Core_API.Infrastructure.Services.File.Excel;
using Core_API.Infrastructure.Services.File.Pdf;
using Core_API.Infrastructure.Services.Integration.ExchangeRate;
using Core_API.Infrastructure.Services.Invoicing;
using Core_API.Infrastructure.Services.Invoicing.CustomerInvoice;
using Core_API.Infrastructure.Services.Invoicing.InvoiceAttachment;
using Core_API.Infrastructure.Services.Invoicing.InvoiceCalculation;
using Core_API.Infrastructure.Services.Invoicing.InvoiceDuplication;
using Core_API.Infrastructure.Services.Invoicing.InvoiceEmail;
using Core_API.Infrastructure.Services.Invoicing.InvoiceNumber;
using Core_API.Infrastructure.Services.Invoicing.InvoiceSettings;
using Core_API.Infrastructure.Services.Invoicing.InvoiceStatistics;
using Core_API.Infrastructure.Services.Invoicing.RecurringInvoice;
using Core_API.Infrastructure.Services.Invoicing.Tax;
using Core_API.Infrastructure.Services.Tasks;
using Core_API.Infrastructure.Services.User;
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
            services.AddScoped<IUnitOfWork, Infrastructure.UnitOfWork.UnitOfWork>();
            //services.AddScoped<IDbInitializer, DbInitializer>();
        }
        private static void AddSecurityServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IAuthStateService, AuthStateService>();
            services.AddScoped<IExternalAuthUrlBuilder, ExternalAuthUrlBuilder>();

            // Register OTP Tracker as Scoped (per request)
            services.AddScoped<IOtpAttemptTracker, OtpAttemptTracker>();
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
            //services.AddScoped<IPaymentService, PaymentService>();
        }

        private static void AddUtilityServices(this IServiceCollection services)
        {
            // Email and Files
            //services.AddScoped<IEmailSendingService, EmailSendingService>();
            //services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IEmailServiceProvider, EmailServiceProvider>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<ICustomerStatementPdfService, CustomerStatementPdfService>();
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