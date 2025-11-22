using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Email.EmailSettings;

namespace Core_API.Application.Contracts.Services
{
    public interface IEmailService
    {
        Task<OperationResult<EmailSettingsDto>> GetEmailSettingsAsync(OperationContext operationContext);
        Task<OperationResult<bool>> SaveEmailSettingsAsync(EmailSettingsDto dto, OperationContext operationContext);
        Task<string> GetFromEmailAsync(OperationContext operationContext);
    }
}