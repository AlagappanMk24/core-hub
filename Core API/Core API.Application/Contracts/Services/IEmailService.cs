using Core_API.Domain.Models.Email;

namespace Core_API.Application.Contracts.Service
{
    /// <summary>
    /// Interface for email service operations using model classes
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a generic email
        /// </summary>
        /// <param name="request">The email request containing recipient, subject, and content</param>
        Task SendEmailAsync(EmailRequest request);

        /// <summary>
        /// Sends an OTP code to the specified email address
        /// </summary>
        /// <param name="email">The recipient's email address</param>
        /// <param name="otpCode">The OTP code to send</param>
        Task SendOtpEmailAsync(string email, string otpCode);
        /// <summary>
        /// Sends a password reset email with reset link
        /// </summary>
        /// <param name="email">The recipient's email address</param>
        /// <param name="subject">The email subject</param>
        /// <param name="htmlMessage">The HTML message content with reset link</param>
        Task SendResetPasswordEmailAsync(string email, string subject, string htmlMessage);

        /// <summary>
        /// Sends an order confirmation email
        /// </summary>
        /// <param name="request">The order confirmation email request containing recipient, subject, and order details</param>
        Task SendOrderConfirmEmailAsync(OrderConfirmationEmailRequest request);

        /// <summary>
        /// Sends a cleanup report email to multiple recipients
        /// </summary>
        /// <param name="request">The cleanup report email request containing recipients and report data</param>
        Task SendCleanupReportEmailAsync(CleanupReportEmailRequest request);

        /// <summary>
        /// Sends a welcome email with temporary password
        /// </summary>
        /// <param name="request">The welcome email request containing recipient, name, and temporary password</param>
        Task SendWelcomeEmailAsync(WelcomeEmailRequest request);
    }
}
