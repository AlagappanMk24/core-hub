using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities.Invoices;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Core_API.Infrastructure.Repositories.Invoice;

public class InvoiceSettingsRepository(CoreInvoiceDbContext dbContext) : IInvoiceSettingsRepository
{
    private readonly CoreInvoiceDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    public async Task<InvoiceSettings> GetByCompanyIdAsync(int companyId)
    {
        return await _dbContext.InvoiceSettings
            .FirstOrDefaultAsync(s => s.CompanyId == companyId);
    }
    public async Task AddAsync(InvoiceSettings settings)
    {
        await _dbContext.InvoiceSettings.AddAsync(settings);
    }
    public void Update(InvoiceSettings settings)
    {
        _dbContext.InvoiceSettings.Update(settings);
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
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CompanyId == companyId);

        if (settings == null || !settings.IsAutomated)
        {
            return "";
        }

        var currentYear = DateTime.UtcNow.Year;
        var prefix = settings.InvoicePrefix;
        var separator = settings.Separator;
        var padding = settings.NumberPadding;
        var maxRetries = 5;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                // Use a transaction to ensure atomicity
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    int nextNumber;

                    // First, try to get existing sequence with row lock
                    var sequence = await _dbContext.InvoiceSequences
                        .FromSqlRaw("SELECT * FROM InvoiceSequences WITH (UPDLOCK, ROWLOCK) WHERE CompanyId = {0}", companyId)
                        .FirstOrDefaultAsync();

                    if (sequence == null)
                    {
                        // Insert new sequence
                        nextNumber = settings.InvoiceStartingNumber;

                        var insertSql = @"
                        INSERT INTO InvoiceSequences (CompanyId, CurrentNumber, CurrentYear, UpdatedDate, Version)
                        VALUES ({0}, {1}, {2}, GETUTCDATE(), 1)";

                        await _dbContext.Database.ExecuteSqlRawAsync(insertSql, companyId, nextNumber, currentYear);
                    }
                    else
                    {
                        // Check if year reset is needed
                        if (settings.IncludeYear && sequence.CurrentYear != currentYear)
                        {
                            nextNumber = settings.InvoiceStartingNumber;

                            var updateSql = @"
                            UPDATE InvoiceSequences 
                            SET CurrentNumber = {0}, 
                                CurrentYear = {1}, 
                                UpdatedDate = GETUTCDATE(),
                                Version = Version + 1
                            WHERE CompanyId = {2}";

                            await _dbContext.Database.ExecuteSqlRawAsync(updateSql, nextNumber, currentYear, companyId);
                        }
                        else
                        {
                            // Increment existing sequence
                            nextNumber = sequence.CurrentNumber + 1;

                            var updateSql = @"
                            UPDATE InvoiceSequences 
                            SET CurrentNumber = CurrentNumber + 1, 
                                UpdatedDate = GETUTCDATE(),
                                Version = Version + 1
                            WHERE CompanyId = {0}
                            AND CurrentNumber = {1}";

                            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(updateSql, companyId, sequence.CurrentNumber);

                            if (rowsAffected == 0)
                            {
                                // Conflict, retry
                                await transaction.RollbackAsync();
                                continue;
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    // Format the number
                    var formattedNumber = nextNumber.ToString($"D{padding}");
                    var newInvoiceNumber = settings.IncludeYear
                        ? $"{prefix}{separator}{currentYear}{separator}{formattedNumber}"
                        : $"{prefix}{separator}{formattedNumber}";

                    return newInvoiceNumber;
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                if (retry >= maxRetries - 1) throw;
                await Task.Delay(50 * (retry + 1));
            }
            catch (SqlException ex) when (ex.Number == 1205) // Deadlock
            {
                if (retry >= maxRetries - 1) throw;
                await Task.Delay(100 * (retry + 1));
            }
        }

        throw new InvalidOperationException("Failed to generate unique invoice number after multiple attempts");
    }
}