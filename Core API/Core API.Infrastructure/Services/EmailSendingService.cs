using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Authentication.Request;
using Core_API.Domain.Models.Email;
using Core_API.Infrastructure.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Core_API.Infrastructure.Services
{
    public class EmailSendingService(IOptions<EmailSettings> options, IEmailService emailSettingsService, ILogger<EmailSendingService> logger) : IEmailSendingService
    {
        private readonly EmailSettings emailSettings = options.Value; 
        private readonly IEmailService _emailSettingsService = emailSettingsService ?? throw new ArgumentNullException(nameof(emailSettingsService));
        private readonly ILogger<EmailSendingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task SendEmailAsync(EmailRequest request)
        {
            var message = new MimeMessage
            {
                Sender = MailboxAddress.Parse(emailSettings.Email)
            };
            message.To.Add(MailboxAddress.Parse(request.Email));
            message.Subject = request.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = EmailTemplates.CreateEmailTemplate(request.HtmlMessage)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendWelcomeEmailAsync(WelcomeEmailRequest request)
        {
            var message = new MimeMessage
            {
                Sender = MailboxAddress.Parse(emailSettings.Email)
            };
            message.To.Add(MailboxAddress.Parse(request.Email));
            message.Subject = request.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = EmailTemplates.CreateWelcomeEmailTemplate(request.Email, request)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendOtpEmailAsync(string email, string otpCode)
        {
            var message = new MimeMessage
            {
                Sender = MailboxAddress.Parse(emailSettings.Email)
            };
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Your OTP Code - Angular Core Hub";

            var builder = new BodyBuilder
            {
                HtmlBody = EmailTemplates.CreateOtpEmailTemplate(otpCode)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendCleanupReportEmailAsync(CleanupReportEmailRequest request)
        {
            var message = new MimeMessage
            {
                Sender = MailboxAddress.Parse(emailSettings.Email)
            };

            foreach (var email in request.ToEmails)
            {
                message.To.Add(MailboxAddress.Parse(email));
            }

            message.Subject = "User Cleanup Report - Angular Core Hub";

            var builder = new BodyBuilder
            {
                HtmlBody = EmailTemplates.CreateCleanupReportEmailTemplate(request.Report)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendResetPasswordEmailAsync(string email, string subject, string htmlMessage)
        {
            var request = new EmailRequest
            {
                Email = email,
                Subject = subject,
                HtmlMessage = htmlMessage
            };

            await SendEmailAsync(request);
        }
        public async Task SendInvoiceEmailAsync(InvoiceEmailRequest request, OperationContext operationContext, MemoryStream pdfStream, string pdfFileName)
        {
            try
            {
                var fromEmail = await _emailSettingsService.GetFromEmailAsync(operationContext);
                var message = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(fromEmail)
                };
                message.From.Add(MailboxAddress.Parse(fromEmail));

                // Add multiple To recipients
                foreach (var toEmail in request.To.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    message.To.Add(MailboxAddress.Parse(toEmail));
                }

                // Add multiple Cc recipients
                foreach (var ccEmail in request.Cc.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    message.Cc.Add(MailboxAddress.Parse(ccEmail));
                }

                message.Subject = request.Subject;

                var templateModel = new InvoiceEmailTemplateModel
                {
                    Content = request.HtmlMessage,
                    InvoiceNumber = request.InvoiceNumber ?? "N/A",
                    AmountDue = request.AmountDue,
                    DueDate = request.DueDate,
                    HasAttachment = pdfStream != null,
                    LogoUrl = "https://img-c.udemycdn.com/course/750x422/5767074_7d5d_6.jpg"
                };

                var builder = new BodyBuilder
                {
                    HtmlBody = EmailTemplates.CreateInvoiceEmailTemplate(templateModel)
                };

                // Attach PDF only if provided
                if (pdfStream != null)
                {
                    pdfStream.Position = 0;
                    builder.Attachments.Add(pdfFileName, pdfStream.ToArray(), ContentType.Parse("application/pdf"));
                }

                message.Body = builder.ToMessageBody();

                await SendEmailMessageAsync(message);
                _logger.LogInformation("Invoice email sent successfully to {ToAddresses} with CC {CcAddresses}",
                                string.Join(", ", request.To), string.Join(", ", request.Cc));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice email to {ToAddress}", request.Email);
                throw;
            }
        }
        private async Task SendEmailMessageAsync(MimeMessage message)
        {
            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            smtp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(emailSettings.Email, emailSettings.Password);
            await smtp.SendAsync(message);
            smtp.Disconnect(true);
        }
    }
}