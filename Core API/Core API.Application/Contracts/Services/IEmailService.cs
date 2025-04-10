using Core_API.Domain.Models.Entities;

namespace Core_API.Application.Contracts.Service
{
    public interface IEmailService
    {
        Task SendResetPasswordEmailAsync(string email, string subject, string htmlMessage);
        Task SendOtpEmailAsync(string email, string otpCode);
    }
}
