using Core_API.Application.Contracts.Services.Email;
using Core_API.Infrastructure.Configuration.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Core_API.Infrastructure.Services.Communication.Email
{
    public class EmailSender(IOptions<EmailSettings> options, ILogger<EmailSender> logger) : IEmailSender
    {
        private readonly EmailSettings _emailSettings = options.Value;
        private readonly ILogger<EmailSender> _logger = logger;
        public async Task SendAsync(EmailMessage message)
        {
            await SendWithAttachmentAsync(message, null);
        }
        public async Task SendWithAttachmentAsync(EmailMessage message, AttachmentFile? attachment)
        {
            try
            {
                var mimeMessage = BuildMimeMessage(message, attachment);
                await SendMimeMessageAsync(mimeMessage);
                _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", message.To));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", message.To));
                throw;
            }
        }
        private MimeMessage BuildMimeMessage(EmailMessage message, AttachmentFile? attachment)
        {
            var mimeMessage = new MimeMessage();

            // Set sender and from
            var fromAddress = MailboxAddress.Parse(string.IsNullOrEmpty(message.From) ? _emailSettings.Email : message.From);
            mimeMessage.From.Add(fromAddress);
            mimeMessage.Sender = fromAddress;

            // Add recipients
            foreach (var to in message.To)
                mimeMessage.To.Add(MailboxAddress.Parse(to));

            foreach (var cc in message.Cc)
                mimeMessage.Cc.Add(MailboxAddress.Parse(cc));

            foreach (var bcc in message.Bcc)
                mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc));

            mimeMessage.Subject = message.Subject;

            var builder = new BodyBuilder();

            if (message.IsHtml)
                builder.HtmlBody = message.Body;
            else
                builder.TextBody = message.Body;

            if (attachment != null)
                builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));

            mimeMessage.Body = builder.ToMessageBody();

            return mimeMessage;
        }
        private async Task SendMimeMessageAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}