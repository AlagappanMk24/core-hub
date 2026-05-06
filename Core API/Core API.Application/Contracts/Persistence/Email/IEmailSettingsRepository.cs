using Core_API.Domain.Entities.Settings;

namespace Core_API.Application.Contracts.Persistence.Email
{
    public interface IEmailSettingsRepository : IGenericRepository<EmailSettings>
    {
        Task<EmailSettings> GetByCompanyIdAsync(int companyId);
        Task SaveAsync(EmailSettings settings);
    }
}