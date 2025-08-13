using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.Contracts.Services.File.Excel;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Infrastructure.Data.Initializers;
using Core_API.Infrastructure.External.SMS;
using Core_API.Infrastructure.Persistence;
using Core_API.Infrastructure.Service;
using Core_API.Infrastructure.Services;
using Core_API.Infrastructure.Services.Admin;
using Core_API.Infrastructure.Services.Authentication;
using Core_API.Infrastructure.Services.Authorization;
using Core_API.Infrastructure.Services.File.Excel;
using Core_API.Infrastructure.Services.File.Pdf;
using Microsoft.Extensions.DependencyInjection;

namespace Core_API.Infrastructure.DI
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            // Register foundational services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbInitializer, DbInitializer>();

            // Register security and authentication services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IPermissionService, PermissionService>();

            // Register core business services
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<ITaxService, TaxService>();

            // Register utility and helper services
            services.AddScoped<IEmailSendingService, EmailSendingService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IContactUsService, ContactUsService>();

            // Register state management services
            services.AddScoped<IAuthStateService, AuthStateService>();

            //services.AddScoped<IProductService, ProductService>();
            //services.AddScoped<ICartService, CartService>();
            //services.AddScoped<ISmsSender, TwilioService>();

            return services;
        }
    }
}
