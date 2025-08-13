using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Data.Context;
using Core_API.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence
{
    public class UnitOfWork(CoreAPIDbContext dbContext, UserManager<ApplicationUser> userManager) : IUnitOfWork
    {
        private readonly CoreAPIDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        public IAuthRepository AuthUsers => new AuthRepository(_dbContext, _userManager);
        public IUserRepository Users => new UserRepository(_dbContext);
        public ICustomerRepository Customers => new CustomerRepository(_dbContext);
        public IInvoiceRepository Invoices => new InvoiceRepository(_dbContext);
        public ITaxTypeRepository TaxTypes => new TaxTypeRepository(_dbContext);
        public IInvoiceSettingsRepository InvoiceSettings => new InvoiceSettingsRepository(_dbContext);
        public IEmailSettingsRepository EmailSettings => new EmailSettingsRepository(_dbContext);
        public ICompanyRepository Companies => new CompanyRepository(_dbContext);
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        // Implement IDisposable for proper resource management
        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}