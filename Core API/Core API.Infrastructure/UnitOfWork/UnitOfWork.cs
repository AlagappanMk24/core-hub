using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Persistence.Auth;
using Core_API.Application.Contracts.Persistence.Companies;
using Core_API.Application.Contracts.Persistence.Customers;
using Core_API.Application.Contracts.Persistence.Email;
using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Application.Contracts.Persistence.RecurringInvoice;
using Core_API.Application.Contracts.Persistence.Tasks;
using Core_API.Application.Contracts.Persistence.Taxes;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories.Auth;
using Core_API.Infrastructure.Repositories.Companies;
using Core_API.Infrastructure.Repositories.Customers;
using Core_API.Infrastructure.Repositories.EmailSettings;
using Core_API.Infrastructure.Repositories.Invoice;
using Core_API.Infrastructure.Repositories.Invoices;
using Core_API.Infrastructure.Repositories.RecurringInvoice;
using Core_API.Infrastructure.Repositories.Tasks;
using Core_API.Infrastructure.Repositories.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Core_API.Infrastructure.UnitOfWork
{
    public class UnitOfWork(CoreInvoiceDbContext dbContext, UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider) : IUnitOfWork
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private IDbContextTransaction? _currentTransaction;

        private IAuthRepository? _authUsers;
        private IUserRepository? _users;
        private ICustomerRepository? _customers;
        private IInvoiceRepository? _invoices;
        private IInvoiceTaxDetailRepository? _invoiceTaxDetails;
        private IInvoiceDiscountRepository? _invoiceDiscounts;
        private IInvoicePaymentRepository? _invoicePayments;
        private IInvoiceAuditLogRepository? _invoiceAuditLogs;
        private IInvoiceItemRepository? _invoiceItems;
        private IInvoiceAttachmentRepository? _invoiceAttachments;
        private ITaxTypeRepository? _taxTypes;
        private IInvoiceSettingsRepository? _invoiceSettings;
        private IEmailSettingsRepository? _emailSettings;
        private ICompanyRepository? _companies;
        private ICompanyRequestRepository? _companyRequests;
        private IRecurringInvoiceRepository? _recurringInvoices;
        private IRecurringInvoiceInstanceRepository? _recurringInvoiceInstances;
        private IRecurringInvoiceAuditLogRepository? _recurringInvoiceAuditLogs;
        private ITaskRepository? _taskItems;
        private ITaskCommentRepository? _taskComments;
        private ITaskAttachmentRepository? _taskAttachments;
        private ITaskAuditLogRepository? _taskAuditLogs;
        private ICustomerCommunicationRepository? _customerCommunications;
        public IAuthRepository AuthUsers => _authUsers ??= new AuthRepository(_dbContext, _userManager);
        public IUserRepository Users => _users ??= new UserRepository(_dbContext);
        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_dbContext);
        public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_dbContext);
        public IInvoiceAttachmentRepository InvoiceAttachments => _invoiceAttachments ??= new InvoiceAttachmentRepository(_dbContext);
        public IInvoiceTaxDetailRepository InvoiceTaxDetails => _invoiceTaxDetails ??= new InvoiceTaxDetailRepository(_dbContext);
        public IInvoiceDiscountRepository InvoiceDiscounts => _invoiceDiscounts ??= new InvoiceDiscountRepository(_dbContext);
        public IInvoicePaymentRepository InvoicePayments => _invoicePayments ??= new InvoicePaymentRepository(_dbContext);
        public IInvoiceAuditLogRepository InvoiceAuditLogs => _invoiceAuditLogs ??= new InvoiceAuditLogRepository(_dbContext);
        public IInvoiceItemRepository InvoiceItems => _invoiceItems ??= new InvoiceItemRepository(_dbContext);
        public ITaxTypeRepository TaxTypes => _taxTypes ??= new TaxTypeRepository(_dbContext);
        public IInvoiceSettingsRepository InvoiceSettings => _invoiceSettings ??= new InvoiceSettingsRepository(_dbContext);
        public IEmailSettingsRepository EmailSettings => _emailSettings ??= new EmailSettingsRepository(_dbContext);
        public ICompanyRepository Companies => _companies ??= new CompanyRepository(_dbContext);
        public ICompanyRequestRepository CompanyRequests => _companyRequests ??= new CompanyRequestRepository(_dbContext);
        public IRecurringInvoiceRepository RecurringInvoices =>
           _recurringInvoices ??= new RecurringInvoiceRepository(
               _dbContext,
               _serviceProvider.GetRequiredService<ILogger<RecurringInvoiceRepository>>());

        public IRecurringInvoiceInstanceRepository RecurringInvoiceInstances =>
            _recurringInvoiceInstances ??= new RecurringInvoiceInstanceRepository(
                _dbContext,
                _serviceProvider.GetRequiredService<ILogger<RecurringInvoiceInstanceRepository>>());

        public IRecurringInvoiceAuditLogRepository RecurringInvoiceAuditLogs =>
            _recurringInvoiceAuditLogs ??= new RecurringInvoiceAuditLogRepository(
                _dbContext,
                _serviceProvider.GetRequiredService<ILogger<RecurringInvoiceAuditLogRepository>>());
        public ITaskRepository TaskItems => _taskItems ??= new TaskRepository(_dbContext);
        public ITaskCommentRepository TaskComments => _taskComments ??= new TaskCommentRepository(_dbContext);
        public ITaskAttachmentRepository TaskAttachments => _taskAttachments ??= new TaskAttachmentRepository(_dbContext);
        public ITaskAuditLogRepository TaskAuditLogs => _taskAuditLogs ??= new TaskAuditLogRepository(_dbContext);
        public ICustomerCommunicationRepository CustomerCommunications => _customerCommunications ??= new CustomerCommunicationRepository(_dbContext);
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
            return _currentTransaction;
        }
        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        // Implement IDisposable for proper resource management
        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}