using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class EmailSettingsRepository(CoreAPIDbContext context) : IEmailSettingsRepository
    {
        private readonly CoreAPIDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        public async Task<EmailSettings> GetByCompanyIdAsync(int companyId)
        {
            return await _context.EmailSettings
                .FirstOrDefaultAsync(s => s.CompanyId == companyId);
        }
        public async Task SaveAsync(EmailSettings settings)
        {
            var existingSettings = await _context.EmailSettings
                .FirstOrDefaultAsync(s => s.CompanyId == settings.CompanyId);

            if (existingSettings != null)
            {
                existingSettings.FromEmail = settings.FromEmail;
                existingSettings.CreatedBy = settings.CreatedBy;
                existingSettings.CreatedDate = settings.CreatedDate;
                _context.EmailSettings.Update(existingSettings);
            }
            else
            {
                await _context.EmailSettings.AddAsync(settings);
            }

            await _context.SaveChangesAsync();
        }
    }
}
