using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Data.Context;
using Core_API.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
namespace Core_API.Infrastructure.Persistence
{
    public class UnitOfWork(CoreAPIDbContext dbContext, UserManager<ApplicationUser> userManager) : IUnitOfWork
    {
        private readonly CoreAPIDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private IDbContextTransaction? _currentTransaction;
        public IAuthRepository AuthUsers => new AuthRepository(_dbContext, _userManager);
        public IUserRepository Users => new UserRepository(_dbContext);
        public ICustomerRepository Customers => new CustomerRepository(_dbContext);
        public IInvoiceRepository Invoices => new InvoiceRepository(_dbContext);
        public IInvoiceAttachmentRepository InvoiceAttachments => new InvoiceAttachmentRepository(_dbContext);
        public ITaxTypeRepository TaxTypes => new TaxTypeRepository(_dbContext);
        public IInvoiceSettingsRepository InvoiceSettings => new InvoiceSettingsRepository(_dbContext);
        public IEmailSettingsRepository EmailSettings => new EmailSettingsRepository(_dbContext);
        public ICompanyRepository Companies => new CompanyRepository(_dbContext);
        public ICompanyRequestRepository CompanyRequests => new CompanyRequestRepository(_dbContext);
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
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