using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Core_API.Infrastructure.Services.Invoicing.InvoiceNumber
{
    /// <summary>
    /// Implementation of invoice number generation service
    /// </summary>
    public class InvoiceNumberService(IUnitOfWork unitOfWork, ILogger<InvoiceNumberService> logger) : IInvoiceNumberService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ILogger<InvoiceNumberService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<OperationResult<string>> GetNextInvoiceNumberAsync(OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving next invoice number.");
                    return OperationResult<string>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                // ✅ Get the current settings to check if automated numbering is enabled
                var settings = await _unitOfWork.InvoiceSettings.GetByCompanyIdAsync(companyId);

                if (settings == null || !settings.IsAutomated)
                {
                    // If settings don't exist or automated numbering is disabled,
                    // we shouldn't generate an automated number
                    _logger.LogWarning("Automated numbering is not enabled for company {CompanyId}", companyId);
                    return OperationResult<string>.FailureResult("Automated numbering is not enabled.");
                }
                var nextNumber = await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId);
                return OperationResult<string>.SuccessResult(nextNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next invoice number for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<string>.FailureResult("Failed to retrieve next invoice number.");
            }
        }
        public async Task<string> GenerateUniqueInvoiceNumberAsync(int companyId)
        {
            int retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    string invoiceNumber = await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId);

                    if (!await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, invoiceNumber))
                        return invoiceNumber;

                    retryCount++;
                }
                catch (Exception ex) when (ex.Message.Contains("duplicate"))
                {
                    retryCount++;
                    if (retryCount >= maxRetries) throw;
                    await Task.Delay(50 * retryCount);
                }
            }
            throw new Exception("Could not generate a unique invoice number after multiple attempts.");
        }
        public async Task<string> GenerateDuplicateInvoiceNumberAsync(int companyId, Domain.Entities.Invoices.Invoice originalInvoice)
        {
            // CASE 1: Original is AUTOMATED - Use next number in sequence
            if (originalInvoice.IsAutomated)
            {
                // Automated invoices: Use next number in sequence
                _logger.LogInformation("Original invoice {InvoiceNumber} is automated. Using next sequence number for duplicate.",
                    originalInvoice.InvoiceNumber);

                string nextNumber = await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId);

                // Ensure uniqueness (rare, but possible with race conditions)
                if (await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, nextNumber))
                {
                    _logger.LogWarning("Next sequence number {NextNumber} already exists. Retrying...", nextNumber);
                    return await GenerateUniqueInvoiceNumberAsync(companyId);
                }
                return nextNumber;
            }

            // CASE 2: Original is MANUAL - Add/Increment "-COPY" suffix
            else
            {
                _logger.LogInformation("Original invoice {InvoiceNumber} is manual. Processing duplicate number.",
                    originalInvoice.InvoiceNumber);

                // Extract the base number (remove any existing copy suffixes)
                string baseNumber = ExtractBaseInvoiceNumber(originalInvoice.InvoiceNumber);

                // Determine the next copy number
                string newNumber = await GetNextCopyNumberAsync(companyId, baseNumber);

                _logger.LogInformation("Generated duplicate number: {OriginalNumber} → {NewNumber}",
                    originalInvoice.InvoiceNumber, newNumber);

                return newNumber;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Extracts the base invoice number by removing -COPY and -COPY-X suffixes
        /// </summary>
        private string ExtractBaseInvoiceNumber(string invoiceNumber)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
                return invoiceNumber;

            // Remove common copy patterns (case insensitive)
            var patterns = new[]
            {
                @"-COPY(?:-\d+)?$",
                @"\s*\(copy\s*\d*\)$",
                @"\s*copy\s*\d*$",
                @"-copy(?:-\d+)?$",
                @"_COPY(?:_\d+)?$",
                @"\s+COPY\s*\d*$"
            };

            var cleaned = invoiceNumber;
            foreach (var pattern in patterns)
            {
                cleaned = Regex.Replace(cleaned, pattern, "", RegexOptions.IgnoreCase);
            }

            return cleaned.Trim();
        }

        /// <summary>
        /// Gets the next available copy number for a base invoice
        /// </summary>
        private async Task<string> GetNextCopyNumberAsync(int companyId, string baseNumber)
        {
            // Get all invoices that start with this base number
            var existingCopies = await _unitOfWork.Invoices.Query()
                .Where(i => i.CompanyId == companyId && !i.IsDeleted &&
                            i.InvoiceNumber.StartsWith(baseNumber))
                .Select(i => i.InvoiceNumber)
                .ToListAsync();

            if (!existingCopies.Any())
            {
                // No copies exist yet
                return $"{baseNumber}-COPY";
            }

            // Find the highest copy number
            int maxCopyNumber = 0;
            var copyNumberPattern = new Regex(@"-COPY-(\d+)$", RegexOptions.IgnoreCase);

            foreach (var copy in existingCopies)
            {
                var match = copyNumberPattern.Match(copy);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var number))
                {
                    if (number > maxCopyNumber)
                        maxCopyNumber = number;
                }
                else if (copy.Equals($"{baseNumber}-COPY", StringComparison.OrdinalIgnoreCase))
                {
                    // Found the base copy without number
                    if (maxCopyNumber < 1)
                        maxCopyNumber = 1;
                }
            }

            // Next copy number
            int nextNumber = maxCopyNumber + 1;

            // If we have "-COPY" and no numbered copies, next should be "-COPY-2"
            if (maxCopyNumber == 1 && existingCopies.Any(c => c.Equals($"{baseNumber}-COPY", StringComparison.OrdinalIgnoreCase)))
            {
                return $"{baseNumber}-COPY-2";
            }

            return $"{baseNumber}-COPY-{nextNumber}";
        }

        #endregion
    }
}

//public async Task<OperationResult<string>> GetNextInvoiceNumberAsync(OperationContext operationContext)
//{
//    try
//    {
//        // Validate CompanyId
//        if (!operationContext.CompanyId.HasValue)
//        {
//            _logger.LogWarning("Company ID is required for retrieving next invoice number.");
//            return OperationResult<string>.FailureResult("Company ID is required.");
//        }
//        int companyId = operationContext.CompanyId.Value;
//        var nextNumber = await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId);
//        return OperationResult<string>.SuccessResult(nextNumber);
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error retrieving next invoice number for company {CompanyId}", operationContext.CompanyId);
//        return OperationResult<string>.FailureResult("Failed to retrieve next invoice number.");
//    }
//}