using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Email.Requests;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Models.Email;

namespace Core_API.Application.Contracts.Services.Email
{
    /// <summary>
    /// Defines the contract for email-related operations in the application.
    /// Provides methods for sending various types of emails and managing email settings.
    /// </summary>
    public interface IEmailServiceProvider
    {
        /// <summary>
        /// Sends an OTP code to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="otpCode">The one-time password to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendOtpEmailAsync(string email, string otpCode);

        /// <summary>
        /// Sends a password reset email to the user.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The HTML content of the reset email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendResetPasswordEmailAsync(string email, string subject, string htmlMessage);

        /// <summary>
        /// Sends a welcome email with temporary credentials to a newly created user.
        /// </summary>
        /// <param name="request">The welcome email request containing user details and temporary password.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendWelcomeEmailAsync(WelcomeEmailRequest request);

        /// <summary>
        /// Sends a notification to the admin about a new company registration request.
        /// </summary>
        /// <param name="companyRequest">The company request details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendCompanyRequestToAdminAsync(CompanyRequest companyRequest);

        /// <summary>
        /// Sends a cleanup report email to configured recipients.
        /// </summary>
        /// <param name="request">The cleanup report request containing report data and recipients.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendCleanupReportEmailAsync(CleanupReportEmailRequest request);

        /// <summary>
        /// Retrieves email settings for a specific company.
        /// </summary>
        /// <param name="operationContext">The current operation context containing company and user information.</param>
        /// <returns>An <see cref="OperationResult{T}"/> containing the email settings or failure details.</returns>
        Task<OperationResult<EmailSettingsDto>> GetEmailSettingsAsync(OperationContext operationContext);

        /// <summary>
        /// Saves or updates email settings for a company.
        /// </summary>
        /// <param name="dto">The email settings data to save.</param>
        /// <param name="operationContext">The current operation context.</param>
        /// <returns>An <see cref="OperationResult{T}"/> indicating success or failure.</returns>
        Task<OperationResult<bool>> SaveEmailSettingsAsync(EmailSettingsDto dto, OperationContext operationContext);

        /// <summary>
        /// Sends a custom email to a customer, optionally with PDF attachment.
        /// </summary>
        /// <param name="request">The customer email request details.</param>
        /// <param name="operationContext">The current operation context for authorization and company scoping.</param>
        /// <returns>An <see cref="OperationResult{T}"/> indicating whether the email was sent successfully.</returns>
        Task<OperationResult<bool>> SendCustomerEmailAsync(SendCustomerEmailRequest request, OperationContext operationContext);

        /// <summary>
        /// Sends an invoice email to the customer, optionally including the invoice as a PDF attachment.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to send.</param>
        /// <param name="emailData">Additional email data such as subject, message, and recipients.</param>
        /// <param name="operationContext">The current operation context.</param>
        /// <returns>An <see cref="OperationResult{T}"/> indicating success or failure.</returns>
        Task<OperationResult<bool>> SendInvoiceEmailAsync(int invoiceId, EmailDataDto emailData, OperationContext operationContext);
    }
}