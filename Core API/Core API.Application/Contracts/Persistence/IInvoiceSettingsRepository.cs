using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IInvoiceSettingsRepository
    {
        Task<InvoiceSettings> GetByCompanyIdAsync(int companyId);
        Task SaveAsync(InvoiceSettings settings);
        Task<string> GetNextInvoiceNumberAsync(int companyId);
    }
}