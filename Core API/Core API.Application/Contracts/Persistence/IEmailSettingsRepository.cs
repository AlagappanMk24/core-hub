using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IEmailSettingsRepository
    {
        Task<EmailSettings> GetByCompanyIdAsync(int companyId);
        Task SaveAsync(EmailSettings settings);
    }
}