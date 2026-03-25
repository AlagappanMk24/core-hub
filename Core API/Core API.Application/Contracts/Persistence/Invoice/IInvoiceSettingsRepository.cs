using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceSettingsRepository
    {
        Task<InvoiceSettings> GetByCompanyIdAsync(int companyId);
        Task AddAsync(InvoiceSettings settings);
        void Update(InvoiceSettings settings);
        Task SaveAsync(InvoiceSettings settings);
        Task<string> GetNextInvoiceNumberAsync(int companyId);
    }
}