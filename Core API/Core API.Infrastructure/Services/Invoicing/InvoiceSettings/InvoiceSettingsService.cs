using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities.Invoices;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Invoicing.InvoiceSettings
{
    public class InvoiceSettingsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InvoiceSettingsService> logger) : IInvoiceSettingsService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<InvoiceSettingsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<OperationResult<InvoiceSettingsDto>> GetInvoiceSettingsAsync(OperationContext operationContext, int? companyId = null)
        {
            try
            {
                int targetCompanyId;
                // Super Admin can specify which company's settings to get
                if (operationContext.IsSuperAdmin && companyId.HasValue)
                {
                    targetCompanyId = companyId.Value;
                    _logger.LogInformation("Super Admin retrieving invoice settings for company {CompanyId}", targetCompanyId);
                }
                else if (operationContext.CompanyId.HasValue)
                {
                    targetCompanyId = operationContext.CompanyId.Value;
                    _logger.LogInformation("Regular Admin retrieving invoice settings for their company {CompanyId}", targetCompanyId);
                }
                else
                {
                    return OperationResult<InvoiceSettingsDto>.FailureResult("Company ID is required.");
                }

                var settings = await _unitOfWork.InvoiceSettings.GetByCompanyIdAsync(targetCompanyId);
                if (settings == null)
                {
                    // Create default settings if not exist
                    settings = new Domain.Entities.Invoices.InvoiceSettings
                    {
                        CompanyId = targetCompanyId,
                        InvoicePrefix = "INV",
                        Separator = "-",
                        IncludeYear = true,
                        NumberPadding = 4,
                        IsAutomated = true,
                        InvoiceStartingNumber = 1,
                        // Discount settings defaults
                        EnableItemLevelDiscounts = true,
                        EnableOverallDiscounts = false,      // Disabled by default for margin protection
                        DefaultDiscountType = "Percentage",
                        MaxDiscountPercentage = 25,          // Requires approval above 25%
                        MaxDiscountAmount = 500,              // Safety ceiling
                        AllowMultipleDiscounts = false,       // Stacking disabled by default
                        ApplyDiscountBeforeTax = true,        // Industry standard
                        ShowDiscountColumnOnInvoice = true    // Show discount to customer
                    };
                    await _unitOfWork.InvoiceSettings.AddAsync(settings);
                    await _unitOfWork.SaveChangesAsync();
                }
                var settingsDto = _mapper.Map<InvoiceSettingsDto>(settings);
                return OperationResult<InvoiceSettingsDto>.SuccessResult(settingsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<InvoiceSettingsDto>.FailureResult("Failed to retrieve invoice settings.");
            }
        }
        public async Task<OperationResult<bool>> SaveInvoiceSettingsAsync(InvoiceSettingsDto dto, OperationContext operationContext)
        {
            try
            {
                int targetCompanyId;

                // Super Admin can save settings for any company
                if (operationContext.IsSuperAdmin)
                {
                    targetCompanyId = dto.CompanyId;
                    _logger.LogInformation("Super Admin saving invoice settings for company {CompanyId}", targetCompanyId);
                }
                else if (operationContext.CompanyId.HasValue)
                {
                    targetCompanyId = operationContext.CompanyId.Value;

                    // Ensure regular Admin can only save settings for their own company
                    if (dto.CompanyId != targetCompanyId)
                    {
                        _logger.LogWarning("Admin attempted to save settings for a different company");
                        return OperationResult<bool>.FailureResult("You can only save settings for your own company.");
                    }
                }
                else
                {
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }

                var settings = await _unitOfWork.InvoiceSettings.GetByCompanyIdAsync(targetCompanyId);
                if (settings == null)
                {
                    settings = _mapper.Map<Domain.Entities.Invoices.InvoiceSettings>(dto);
                    settings.CompanyId = targetCompanyId;
                    settings.CreatedBy = operationContext.UserId;
                    settings.CreatedDate = DateTime.UtcNow;
                    await _unitOfWork.InvoiceSettings.AddAsync(settings);
                }
                else
                {
                    _mapper.Map(dto, settings);
                    settings.UpdatedBy = operationContext.UserId;
                    settings.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.InvoiceSettings.Update(settings);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice settings saved successfully for company {CompanyId}", targetCompanyId);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to save invoice settings.");
            }
        }
    }
}
