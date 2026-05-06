using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.Contracts.Services.Files;
using Core_API.Application.DTOs.Email.Requests;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Entities.Settings;
using Core_API.Domain.Enums;
using Core_API.Domain.Models.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Core_API.Infrastructure.Services.Communication.Email
{
    /// <summary>
    /// Main implementation of the email service provider.
    /// Handles sending OTPs, welcome emails, admin notifications, customer emails, 
    /// invoice emails, and manages email settings with proper logging and error handling.
    /// </summary>
    public class EmailServiceProvider(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IEmailTemplateService templateService,
        IConfiguration configuration,
        IPdfService pdfService,
        ICustomerStatementPdfService customerStatementPdfService,
        ILogger<EmailServiceProvider> logger) : IEmailServiceProvider
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IEmailTemplateService _templateService = templateService;
        private readonly IPdfService _pdfService = pdfService;
        private readonly ICustomerStatementPdfService _customerStatementPdfService = customerStatementPdfService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<EmailServiceProvider> _logger = logger;

        #region Public Email Methods

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task SendOtpEmailAsync(string email, string otpCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(email);
            ArgumentException.ThrowIfNullOrEmpty(otpCode);

            var emailMessage = new EmailMessage
            {
                To = [email],
                Subject = "Your OTP Code - CoreInvoice",
                Body = _templateService.RenderOtpTemplate(otpCode),
                IsHtml = true
            };

            await SendEmailAsync(emailMessage, "OTP");
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task SendResetPasswordEmailAsync(string email, string subject, string htmlMessage)
        {
            ArgumentException.ThrowIfNullOrEmpty(email);

            var emailMessage = new EmailMessage
            {
                To = [email],
                Subject = subject,
                Body = htmlMessage,
                IsHtml = true
            };

            await SendEmailAsync(emailMessage, "Password Reset");
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task SendWelcomeEmailAsync(WelcomeEmailRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var emailData = new WelcomeEmailData
            {
                Name = request.Name,
                Email = request.Email,
                TemporaryPassword = request.TemporaryPassword,
                LoginLink = request.HtmlMessage ?? "#"
            };

            var emailMessage = new EmailMessage
            {
                To = [request.Email],
                Subject = request.Subject ?? "Welcome to CoreInvoice",
                Body = _templateService.RenderWelcomeTemplate(emailData),
                IsHtml = true
            };

            await SendEmailAsync(emailMessage, "Welcome");
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task SendCompanyRequestToAdminAsync(CompanyRequest companyRequest)
        {
            ArgumentNullException.ThrowIfNull(companyRequest);

            try
            {
                var adminEmail = _configuration["AdminSettings:Email"]
                    ?? "alagappanmuthukumar1998@gmail.com";

                var baseUrl = _configuration["AppSettings:BaseUrl"]
                    ?? "http://localhost:4200";

                var reviewLink = $"{baseUrl}/admin/company-requests/{companyRequest.Id}";
                var allRequestsLink = $"{baseUrl}/admin/company-requests";

                var subject = $"New Company Registration Request - {companyRequest.CompanyName}";

                var htmlBody = _templateService.RenderCompanyRequestAdminTemplate(companyRequest, reviewLink, allRequestsLink);

                var emailMessage = new EmailMessage
                {
                    To = new List<string> { adminEmail },
                    Subject = subject,
                    Body = htmlBody,
                    IsHtml = true
                };

                await SendEmailAsync(emailMessage, "Company Request Notification");

                _logger.LogInformation("Company request notification sent to admin for request {RequestId} | Company: {CompanyName}",
                    companyRequest.Id, companyRequest.CompanyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send company request notification to admin for request {RequestId}",
                    companyRequest.Id);
            }
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task SendCleanupReportEmailAsync(CleanupReportEmailRequest request)
        {
            // Create the email message
            var emailMessage = new EmailMessage
            {
                To = (List<string>)request.ToEmails,
                Subject = "User Cleanup Report - CoreInvoice",
                Body = _templateService.RenderCleanupReportTemplate(request.Report),
                IsHtml = true
            };

            await _emailSender.SendAsync(emailMessage);
            //_logger.LogInformation("Cleanup report email sent to {Count} recipients", request.ToEmails.Count);
        }

        #endregion

        #region Email Settings

        /// <inheritdoc/>
        public async Task<OperationResult<EmailSettingsDto>> GetEmailSettingsAsync(OperationContext operationContext)
        {
            try
            {
                if (!operationContext.CompanyId.HasValue)
                    return OperationResult<EmailSettingsDto>.FailureResult("Company ID is required.");

                var settings = await _unitOfWork.EmailSettings.GetByCompanyIdAsync(operationContext.CompanyId.Value);
                if (settings == null)
                    return OperationResult<EmailSettingsDto>.FailureResult("Email settings not found.");

                return OperationResult<EmailSettingsDto>.SuccessResult(new EmailSettingsDto
                {
                    FromEmail = settings.FromEmail,
                    CompanyId = settings.CompanyId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email settings");
                return OperationResult<EmailSettingsDto>.FailureResult("Failed to retrieve email settings.");
            }
        }

        /// <inheritdoc/>
        public async Task<OperationResult<bool>> SaveEmailSettingsAsync(EmailSettingsDto dto, OperationContext operationContext)
        {
            try
            {
                if (!operationContext.CompanyId.HasValue)
                    return OperationResult<bool>.FailureResult("Company ID is required.");

                var settings = await _unitOfWork.EmailSettings.GetByCompanyIdAsync(operationContext.CompanyId.Value);

                if (settings == null)
                {
                    settings = new EmailSettings
                    {
                        CompanyId = operationContext.CompanyId.Value,
                        FromEmail = dto.FromEmail,
                        CreatedBy = operationContext.UserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.EmailSettings.AddAsync(settings);
                }
                else
                {
                    settings.FromEmail = dto.FromEmail;
                    settings.UpdatedBy = operationContext.UserId;
                    settings.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.EmailSettings.Update(settings);
                }

                await _unitOfWork.SaveChangesAsync();
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving email settings");
                return OperationResult<bool>.FailureResult("Failed to save email settings.");
            }
        }
        #endregion

        #region Customer & Invoice Emails

        /// <inheritdoc/>
        public async Task<OperationResult<bool>> SendCustomerEmailAsync(SendCustomerEmailRequest request, OperationContext operationContext)
        {
            try
            {
                // Validate
                if (!operationContext.CompanyId.HasValue)
                    return OperationResult<bool>.FailureResult("Company ID is required.");

                // Get customer
                var customer = await _unitOfWork.Customers.GetAsync(c =>
                        c.Id == request.CustomerId &&
                        c.CompanyId == operationContext.CompanyId.Value &&
                        !c.IsDeleted);

                if (customer == null)
                    return OperationResult<bool>.FailureResult("Customer not found.");

                // Get from email
                var fromEmail = await GetFromEmailAsync(operationContext);

                var emailMessage = new EmailMessage
                {
                    From = fromEmail,
                    To = [request.To],
                    Cc = ParseEmails(request.Cc),
                    Bcc = ParseEmails(request.Bcc),
                    Subject = request.Subject,
                    Body = _templateService.RenderCustomerEmailTemplate(new CustomerEmailData
                    {
                        CustomerName = customer.Name,
                        Content = request.Body,
                        Type = request.Type,
                        CompanyName = "" // Can be populated if needed
                    }),
                    IsHtml = true
                };

                // Add copy to self
                if (request.SendCopyToSelf && !string.IsNullOrEmpty(fromEmail))
                    emailMessage.Cc.Add(fromEmail);

                // Handle attachment
                AttachmentFile? attachment = null;
                if (request.AttachPdf)
                {
                    attachment = await GeneratePdfAttachmentAsync(request, operationContext, customer);
                }

                await SendEmailWithAttachmentAsync(emailMessage, attachment);

                await LogEmailCommunicationAsync(customer.Id, request, operationContext);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending customer email");
                return OperationResult<bool>.FailureResult("Failed to send email.");
            }
        }

        /// <inheritdoc/>
        public async Task<OperationResult<bool>> SendInvoiceEmailAsync(int invoiceId, EmailDataDto emailData, OperationContext operationContext)
        {
            try
            {
                if (!operationContext.CompanyId.HasValue)
                    return OperationResult<bool>.FailureResult("Company ID is required.");

                // Get invoice with customer
                var invoice = await _unitOfWork.Invoices.GetAsync(
                    i => i.Id == invoiceId && i.CompanyId == operationContext.CompanyId.Value && !i.IsDeleted,
                    "Customer");

                if (invoice == null || invoice.Customer == null)
                    return OperationResult<bool>.FailureResult("Invoice not found.");

                // Get from email
                var fromEmail = await GetFromEmailAsync(operationContext);

                // Prepare PDF attachment
                AttachmentFile? attachment = null;
                if (emailData.AttachPdf)
                {
                    var pdfResult = await _pdfService.GenerateInvoicePdfAsync(invoiceId, operationContext);
                    if (pdfResult.IsSuccess && pdfResult.Data?.PdfStream != null)
                    {
                        attachment = new AttachmentFile
                        {
                            FileName = $"invoice_{invoice.InvoiceNumber}.pdf",
                            Content = pdfResult.Data.PdfStream.ToArray(),
                            ContentType = "application/pdf"
                        };
                    }
                }

                // Prepare email message
                var emailMessage = new EmailMessage
                {
                    From = fromEmail,
                    To = emailData.To.Where(e => !string.IsNullOrWhiteSpace(e)).ToList(),
                    Cc = emailData.Cc?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>(),
                    Bcc = emailData.Bcc?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>(),
                    Subject = emailData.Subject ?? $"Invoice {invoice.InvoiceNumber}",
                    Body = _templateService.RenderInvoiceTemplate(new InvoiceEmailData
                    {
                        InvoiceNumber = invoice.InvoiceNumber,
                        AmountDue = invoice.TotalAmount,
                        DueDate = invoice.DueDate,
                        CustomerName = invoice.Customer.Name,
                        Content = emailData.Message ?? GenerateDefaultInvoiceMessage(),
                        HasAttachment = attachment != null
                    }),
                    IsHtml = true
                };

                // Add copy to self
                if (emailData.SendCopyToSelf && !string.IsNullOrEmpty(fromEmail))
                    emailMessage.Cc.Add(fromEmail);

                // Send email
                if (attachment != null)
                    await _emailSender.SendWithAttachmentAsync(emailMessage, attachment);
                else
                    await _emailSender.SendAsync(emailMessage);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice email");
                return OperationResult<bool>.FailureResult("Failed to send email.");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Core helper method to send emails with consistent logging and error handling.
        /// </summary>
        private async System.Threading.Tasks.Task SendEmailAsync(EmailMessage message, string emailType)
        {
            try
            {
                await _emailSender.SendAsync(message);
                _logger.LogInformation("{EmailType} email sent successfully to {Recipients}",
                    emailType, string.Join(", ", message.To));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {EmailType} email to {Recipients}",
                    emailType, string.Join(", ", message.To));
                throw;
            }
        }

        /// <summary>
        /// Sends an email that may include an attachment.
        /// </summary>
        private async System.Threading.Tasks.Task SendEmailWithAttachmentAsync(EmailMessage message, AttachmentFile? attachment)
        {
            if (attachment == null)
            {
                await SendEmailAsync(message, "Customer");
                return;
            }

            try
            {
                await _emailSender.SendWithAttachmentAsync(message, attachment);
                _logger.LogInformation("Email with attachment sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email with attachment");
                throw;
            }
        }

        /// <summary>
        /// Parses a comma-separated string of email addresses into a list.
        /// </summary>
        private static List<string> ParseEmails(string? emailList)
        {
            if (string.IsNullOrWhiteSpace(emailList))
                return new List<string>();

            return emailList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .ToList();
        }

        /// <summary>
        /// Retrieves the configured "From" email address for the current company.
        /// </summary>
        private async Task<string> GetFromEmailAsync(OperationContext context)
        {
            if (!context.CompanyId.HasValue)
                return "noreply@coreinvoice.com";

            var settings = await _unitOfWork.EmailSettings.GetByCompanyIdAsync(context.CompanyId.Value);

            return !string.IsNullOrWhiteSpace(settings?.FromEmail)
                ? settings.FromEmail
                : "noreply@coreinvoice.com";
        }

        /// <summary>
        /// Generates PDF attachment based on request type (invoice or statement).
        /// </summary>
        private async Task<AttachmentFile?> GeneratePdfAttachmentAsync(SendCustomerEmailRequest request, OperationContext operationContext, Domain.Entities.Customers.Customer customer)
        {
            if (request.Type == "invoice" && request.InvoiceId.HasValue)
            {
                var pdfResult = await _pdfService.GenerateInvoicePdfAsync(request.InvoiceId.Value, operationContext);
                if (pdfResult.IsSuccess && pdfResult.Data?.PdfStream != null)
                {
                    return new AttachmentFile
                    {
                        FileName = $"invoice_{pdfResult.Data.InvoiceNumber}.pdf",
                        Content = pdfResult.Data.PdfStream.ToArray(),
                        ContentType = "application/pdf"
                    };
                }
            }
            else if (request.Type == "statement")
            {
                // Generate customer statement PDF
                var pdfStream = await _customerStatementPdfService.GenerateStatementAsync(customer, operationContext);
                return new AttachmentFile
                {
                    FileName = $"customer_statement_{customer.Id}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                    Content = pdfStream.ToArray(),
                    ContentType = "application/pdf"
                };
            }

            return null;
        }

        /// <summary>
        /// Logs the email communication record for auditing purposes.
        /// </summary>
        private async System.Threading.Tasks.Task LogEmailCommunicationAsync(int customerId, SendCustomerEmailRequest request, OperationContext operationContext)
        {
            var communication = new CustomerCommunication
            {
                CustomerId = customerId,
                Type = "email",
                Subject = request.Subject,
                Content = request.Body,
                Direction = "outbound",
                SentAt = DateTime.UtcNow,
                SentBy = operationContext.UserId,
                Status = "sent"
            };

            await _unitOfWork.CustomerCommunications.AddAsync(communication);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a default message for invoice emails when no custom message is provided.
        /// </summary>
        private static string GenerateDefaultInvoiceMessage()
        {
            return @"<p>Please find attached your invoice for your reference.</p>
                     <p>Thank you for your business!</p>";
        }
        #endregion
    }
}