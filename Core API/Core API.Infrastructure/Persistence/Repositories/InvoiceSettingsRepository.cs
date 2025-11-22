using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories;

public class InvoiceSettingsRepository(CoreAPIDbContext dbContext) : IInvoiceSettingsRepository
{
    private readonly CoreAPIDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    public async Task<InvoiceSettings> GetByCompanyIdAsync(int companyId)
    {
        return await _dbContext.InvoiceSettings
            .FirstOrDefaultAsync(s => s.CompanyId == companyId);
    }
    public async Task SaveAsync(InvoiceSettings settings)
    {
        var existingSettings = await _dbContext.InvoiceSettings
            .FirstOrDefaultAsync(s => s.CompanyId == settings.CompanyId);

        if (existingSettings == null)
        {
            _dbContext.InvoiceSettings.Add(settings);
        }
        else
        {
            _dbContext.Entry(existingSettings).CurrentValues.SetValues(settings);
            existingSettings.UpdatedDate = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
    }
    public async Task<string> GetNextInvoiceNumberAsync(int companyId)
    {
        var settings = await _dbContext.InvoiceSettings
            .FirstOrDefaultAsync(s => s.CompanyId == companyId);

        if (settings == null || !settings.IsAutomated)
        {
            return "";
        }

        var lastInvoice = await _dbContext.Invoices
            .Where(i => i.CompanyId == companyId && i.InvoiceNumber.StartsWith(settings.InvoicePrefix))
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        int nextNumber = settings.InvoiceStartingNumber;
        if (lastInvoice != null)
        {
            var numberPart = lastInvoice.InvoiceNumber.Replace(settings.InvoicePrefix, "");
            if (int.TryParse(numberPart, out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{settings.InvoicePrefix}{nextNumber:D4}";
    }
}
