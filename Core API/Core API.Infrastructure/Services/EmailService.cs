using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Email.EmailSettings;
using Core_API.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core_API.Infrastructure.Services
{
    public class EmailService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EmailService> logger, IOptions<EmailSettings> options) : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<EmailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly EmailSettings _emailSettings = options.Value;
        public async Task<OperationResult<EmailSettingsDto>> GetEmailSettingsAsync(OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving email settings.");
                    return OperationResult<EmailSettingsDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                var settings = await _unitOfWork.EmailSettings.GetByCompanyIdAsync(companyId);
                if (settings == null)
                {
                    return OperationResult<EmailSettingsDto>.FailureResult("Email settings not found for this company.");
                }

                var settingsDto = _mapper.Map<EmailSettingsDto>(settings);
                return OperationResult<EmailSettingsDto>.SuccessResult(settingsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email settings for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<EmailSettingsDto>.FailureResult("Failed to retrieve email settings.");
            }
        }
        public async Task<OperationResult<bool>> SaveEmailSettingsAsync(EmailSettingsDto dto, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for saving email settings.");
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                var settings = _mapper.Map<EmailSettings>(dto);
                settings.CompanyId = companyId;
                settings.CreatedBy = operationContext.UserId;
                settings.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.EmailSettings.SaveAsync(settings);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving email settings for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to save email settings.");
            }
        }
        public async Task<string> GetFromEmailAsync(OperationContext operationContext)
        {
            try
            {
                var settingsResult = await GetEmailSettingsAsync(operationContext);
                if (settingsResult.IsSuccess && !string.IsNullOrEmpty(settingsResult.Data.FromEmail))
                {
                    return settingsResult.Data.FromEmail;
                }
                _logger.LogWarning("No dynamic email settings found for company {CompanyId}, using fallback email.", operationContext.CompanyId);
                return _emailSettings.FromEmail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dynamic email settings for company {CompanyId}, using fallback email.", operationContext.CompanyId);
                return _emailSettings.FromEmail;
            }
        }
    }
}