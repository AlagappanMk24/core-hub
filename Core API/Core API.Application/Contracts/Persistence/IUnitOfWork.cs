using Microsoft.EntityFrameworkCore.Storage;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthRepository AuthUsers {  get; }
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IInvoiceRepository Invoices { get; }
        IInvoiceAttachmentRepository InvoiceAttachments { get; }
        ITaxTypeRepository TaxTypes { get; }
        IInvoiceSettingsRepository InvoiceSettings { get; }
        IEmailSettingsRepository EmailSettings { get; }
        ICompanyRepository Companies { get; }
        ICompanyRequestRepository CompanyRequests { get; }
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(); 
        Task CommitTransactionAsync(); 
        Task RollbackTransactionAsync(); 
    }
}