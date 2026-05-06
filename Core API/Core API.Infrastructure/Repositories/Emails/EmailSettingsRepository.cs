using Core_API.Application.Contracts.Persistence.Email;
using Core_API.Domain.Entities.Settings;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.EmailSettings
{
    public class EmailSettingsRepository(CoreInvoiceDbContext context) : GenericRepository<Domain.Entities.Settings.EmailSettings>(context), IEmailSettingsRepository
    {
        private readonly CoreInvoiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        public async Task<Domain.Entities.Settings.EmailSettings> GetByCompanyIdAsync(int companyId)
        {
            return await _context.EmailSettings
                .FirstOrDefaultAsync(s => s.CompanyId == companyId);
        }
        public async Task SaveAsync(Domain.Entities.Settings.EmailSettings settings)
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