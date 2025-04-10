using Core_API.Application.Contracts.Service;
using Core_API.Domain.Models.Entities;
using Core_API.Infrastructure.Data.Context;
using Core_API.Infrastructure.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Core_API.Infrastructure.Service.Email
{
    public class EmailService(IOptions<EmailSettings> setting) : IEmailService
    {
        private readonly EmailSettings _settings = setting.Value;
        public async Task SendResetPasswordEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_settings.Email);
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = CreateEmailTemplate(htmlMessage)
            };

            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            smtp.Connect(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_settings.Email, _settings.Password);
            await smtp.SendAsync(message);
            smtp.Disconnect(true);
        }
        private string CreateEmailTemplate(string content)
        {
            return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Reset Your Password</title>
            <style>
                body {{
                    font-family: 'Arial', sans-serif;
                    background-color: #f4f4f9;
                    margin: 0;
                    padding: 0;
                    color: #333;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 30px auto;
                    background: #ffffff;
                    padding: 30px;
                    border-radius: 12px;
                    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15);
                    text-align: center;
                }}
                .email-header {{
                    background: linear-gradient(90deg, #007bff, #0056b3);
                    color: #ffffff;
                    padding: 20px;
                    font-size: 24px;
                    font-weight: bold;
                    border-top-left-radius: 12px;
                    border-top-right-radius: 12px;
                }}
                .email-body {{
                    padding: 25px;
                    font-size: 16px;
                    line-height: 1.6;
                    text-align: left;
                }}
                .email-body p {{
                    margin: 10px 0;
                }}
                .btn {{
                    display: inline-block;
                    padding: 14px 24px;
                    margin: 20px 0;
                    font-size: 18px;
                    font-weight: bold;
                    color: #ffffff;
                    background: linear-gradient(90deg, #007bff, #0056b3);
                    border-radius: 6px;
                    text-decoration: none;
                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                    transition: 0.3s ease-in-out;
                }}
                .btn:hover {{
                    background: linear-gradient(90deg, #0056b3, #003e80);
                    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.3);
                }}
                .email-footer {{
                    margin-top: 25px;
                    padding-top: 15px;
                    border-top: 1px solid #dddddd;
                    font-size: 12px;
                    color: #777;
                }}
                .security-tip {{
                    margin-top: 15px;
                    padding: 15px;
                    background: #f8f9fa;
                    border-radius: 8px;
                    font-size: 14px;
                    color: #555;
                }}
                .security-tip strong {{
                    color: #d9534f;
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='email-header'>
                    Password Reset Request
                </div>
                <div class='email-body'>
                    <p>Hi <strong> Dear User</strong>,</p>
                    <p>We received a request to reset your password.</p>
                    <div>
                      {content}
                    </div>
                    <p>If you didn’t request this, you can ignore this email. Your password won’t change unless you create a new one.</p>
                    <div class='security-tip'>
                        <strong>Security Tip:</strong> Never share your password or reset link with anyone. If you suspect any suspicious activity, contact our support team immediately.
                    </div>
                </div>
                <div class='email-footer'>
                    <p>Need help? Contact our support team at <a href='mailto:support@yourcompany.com'>support@yourcompany.com</a></p>
                    <p><a href='https://yourwebsite.com'>Visit our website</a></p>
                </div>
            </div>
        </body>
        </html>
    ";
        }
        public async Task SendOtpEmailAsync(string email, string otpCode)
        {
            try
            {
                var message = new MimeMessage();
                message.Sender = MailboxAddress.Parse(_settings.Email);
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Your OTP Code for Secure Login";

                var builder = new BodyBuilder
                {
                    HtmlBody = CreateOtpEmailTemplate(otpCode)
                };

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                smtp.Connect(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_settings.Email, _settings.Password);
                await smtp.SendAsync(message);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string CreateOtpEmailTemplate(string otpCode)
        {
            return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>OTP Verification</title>
            <style>
                body {{
                    font-family: 'Arial', sans-serif;
                    background-color: #f4f4f9;
                    margin: 0;
                    padding: 0;
                    color: #333;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 30px auto;
                    background: #ffffff;
                    padding: 30px;
                    border-radius: 12px;
                    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15);
                    text-align: center;
                }}
                .email-header {{
                    background: linear-gradient(90deg, #007bff, #0056b3);
                    color: #ffffff;
                    padding: 20px;
                    font-size: 24px;
                    font-weight: bold;
                    border-top-left-radius: 12px;
                    border-top-right-radius: 12px;
                }}
                .email-body {{
                    padding: 25px;
                    font-size: 16px;
                    line-height: 1.6;
                }}
                .otp-code {{
                    font-size: 24px;
                    font-weight: bold;
                    color: #007bff;
                    padding: 10px;
                    background: #f8f9fa;
                    display: inline-block;
                    border-radius: 6px;
                    margin: 15px 0;
                    letter-spacing: 2px;
                }}
                .btn {{
                    display: inline-block;
                    padding: 14px 24px;
                    margin: 20px 0;
                    font-size: 18px;
                    font-weight: bold;
                    color: #ffffff;
                    background: linear-gradient(90deg, #007bff, #0056b3);
                    border-radius: 6px;
                    text-decoration: none;
                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                    transition: 0.3s ease-in-out;
                }}
                .btn:hover {{
                    background: linear-gradient(90deg, #0056b3, #003e80);
                    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.3);
                }}
                .email-footer {{
                    margin-top: 25px;
                    padding-top: 15px;
                    border-top: 1px solid #dddddd;
                    font-size: 12px;
                    color: #777;
                }}
                .security-tip {{
                    margin-top: 15px;
                    padding: 15px;
                    background: #f8f9fa;
                    border-radius: 8px;
                    font-size: 14px;
                    color: #555;
                }}
                .security-tip strong {{
                    color: #d9534f;
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='email-header'>
                    OTP Verification
                </div>
                <div class='email-body'>
                    <p>Dear User,</p>
                    <p>Use the following OTP code to verify your login request:</p>
                    <div class='otp-code'>{otpCode}</div>
                    <p>This OTP is valid for only <strong>10 minutes</strong>. Do not share this code with anyone.</p>
                    <p>If you didn’t request this, you can ignore this email.</p>
                    <div class='security-tip'>
                        <strong>Security Tip:</strong> Never share your OTP code. If you suspect unauthorized access, change your password immediately.
                    </div>
                </div>
                <div class='email-footer'>
                    <p>Need help? Contact our support team at <a href='mailto:support@yourcompany.com'>support@yourcompany.com</a></p>
                    <p><a href='https://yourwebsite.com'>Visit our website</a></p>
                </div>
            </div>
        </body>
        </html>";
        }
    }
}