using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Application.Contracts.Persistence.RecurringInvoice;
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
        IInvoiceTaxDetailRepository InvoiceTaxDetails { get; }
        IInvoiceDiscountRepository InvoiceDiscounts { get; }
        IInvoicePaymentRepository InvoicePayments { get; }
        IInvoiceAuditLogRepository InvoiceAuditLogs { get; }
        IInvoiceItemRepository InvoiceItems { get; }
        ITaxTypeRepository TaxTypes { get; }
        IRecurringInvoiceRepository RecurringInvoices { get; }
        IRecurringInvoiceInstanceRepository RecurringInvoiceInstances { get; }
        IRecurringInvoiceAuditLogRepository RecurringInvoiceAuditLogs { get; }
        IInvoiceSettingsRepository InvoiceSettings { get; }
        IEmailSettingsRepository EmailSettings { get; }
        ICompanyRepository Companies { get; }
        ICompanyRequestRepository CompanyRequests { get; }
        ITaskRepository TaskItems { get; }
        ITaskAttachmentRepository TaskAttachments { get; }
        ITaskCommentRepository TaskComments { get; }
        ITaskAuditLogRepository TaskAuditLogs { get; }
        Task SaveChangesAsync();
        Task SaveChangesAsync(CancellationToken cancellationToken);
        Task<IDbContextTransaction> BeginTransactionAsync(); 
        Task CommitTransactionAsync(); 
        Task RollbackTransactionAsync(); 
    }
}