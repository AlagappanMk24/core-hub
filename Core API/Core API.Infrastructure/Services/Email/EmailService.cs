using Core_API.Application.Contracts.Service;
using Core_API.Domain.Entities;
using Core_API.Domain.Models.Email;
using Core_API.Infrastructure.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Core_API.Infrastructure.Service.Email
{
    public class EmailService(IOptions<EmailSettings> options) : IEmailService
    {
        private readonly EmailSettings emailSettings = options.Value;
        public async Task SendEmailAsync(EmailRequest request)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(emailSettings.Email);
            message.To.Add(MailboxAddress.Parse(request.Email));
            message.Subject = request.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = CreateEmailTemplate(request.HtmlMessage)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendWelcomeEmailAsync(WelcomeEmailRequest request)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(emailSettings.Email);
            message.To.Add(MailboxAddress.Parse(request.Email));
            message.Subject = request.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = CreateWelcomeEmailTemplate(request.Email, request)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendOtpEmailAsync(string email, string otpCode)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(emailSettings.Email);
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Your OTP Code - Angular Core Hub";

            var builder = new BodyBuilder
            {
                HtmlBody = CreateOtpEmailTemplate(otpCode)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendOrderConfirmEmailAsync(OrderConfirmationEmailRequest request)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(emailSettings.Email);
            message.To.Add(MailboxAddress.Parse(request.Email));
            message.Subject = request.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = OrderConfirmEmailTemplate(request.OrderHeader)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
        }
        public async Task SendCleanupReportEmailAsync(CleanupReportEmailRequest request)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(emailSettings.Email);

            foreach (var email in request.ToEmails)
            {
                message.To.Add(MailboxAddress.Parse(email));
            }

            message.Subject = "User Cleanup Report - Angular Core Hub";

            var builder = new BodyBuilder
            {
                HtmlBody = CreateCleanupReportEmailTemplate(request.Report)
            };

            message.Body = builder.ToMessageBody();

            await SendEmailMessageAsync(message);
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
        private string CreateEmailTemplate(string content)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Email</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f4f4f9;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background: #ffffff;
                            padding: 20px;
                            border: 1px solid #e1e1e1;
                            border-radius: 8px;
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }}
                        .email-header {{
                            text-align: center;
                            background: #4caf50;
                            color: #ffffff;
                            padding: 10px 0;
                            border-top-left-radius: 8px;
                            border-top-right-radius: 8px;
                        }}
                        .email-body {{
                            padding: 20px;
                            color: #333333;
                            line-height: 1.6;
                        }}
                        .email-footer {{
                            text-align: center;
                            font-size: 12px;
                            color: #888888;
                            margin-top: 20px;
                            border-top: 1px solid #dddddd;
                            padding-top: 10px;
                        }}
                        a {{
                            color: #4caf50;
                            text-decoration: none;
                        }}
                        a:hover {{
                            text-decoration: underline;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>Angular Core Hub</h1>
                        </div>
                        <div class='email-body'>
                            {content}
                        </div>
                        <div class='email-footer'>
                            <p>Thank you for using our services!</p>
                            <p><a href='https://yourwebsite.com'>Visit our website</a></p>
                        </div>
                    </div>
                </body>
                </html>
                ";
        }
        private string CreateOtpEmailTemplate(string code)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Two-Factor Authentication</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f8f9fa;
                        color: #333333;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        padding: 20px;
                        border: 1px solid #e1e1e1;
                        border-radius: 8px;
                        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        text-align: center;
                        background: #007bff;
                        color: #ffffff;
                        padding: 20px;
                        border-top-left-radius: 8px;
                        border-top-right-radius: 8px;
                    }}
                    .header h1 {{
                        margin: 0;
                        font-size: 24px;
                    }}
                    .body {{
                        padding: 20px;
                        text-align: center;
                    }}
                    .body p {{
                        font-size: 16px;
                        line-height: 1.6;
                        margin: 10px 0;
                    }}
                    .code {{
                        font-size: 24px;
                        font-weight: bold;
                        background: #f8f9fa;
                        padding: 10px 20px;
                        border: 1px dashed #007bff;
                        display: inline-block;
                        margin: 20px 0;
                    }}
                    .footer {{
                        margin-top: 20px;
                        text-align: center;
                        font-size: 14px;
                        color: #888888;
                        border-top: 1px solid #dddddd;
                        padding-top: 10px;
                    }}
                    a {{
                        color: #007bff;
                        text-decoration: none;
                        font-weight: bold;
                    }}
                    a:hover {{
                        text-decoration: underline;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Secure Your Account</h1>
                    </div>
                    <div class='body'>
                        <p>Hello,</p>
                        <p>We have received a request to access your account using two-factor authentication (2FA). Your 2FA code is:</p>
                        <div class='code'>{code}</div>
                        <p>This code will expire in 10 minutes. If you did not request this, please ignore this email or contact support immediately.</p>
                        <p><a href='https://yourwebsite.com/support'>Contact Support</a></p>
                    </div>
                    <div class='footer'>
                        <p>Thank you for keeping your account secure.</p>
                        <p><a href='https://yourwebsite.com'>Visit Our Website</a></p>
                    </div>
                </div>
            </body>
            </html>";
        }
        private string OrderConfirmEmailTemplate(OrderHeader orderHeader)
        {
            var orderItemsHtml = string.Join("", orderHeader.OrderDetails.Select(
                    item => $@"
                        <tr>
                            <td>{item.Product.Title}</td>
                            <td>{item.Count}</td>
                            <td>{item.Price:C}</td>
                            <td>{item.Count * item.Price:C}</td>
                        </tr>")
            );

            return $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f7f7f7;
                            margin: 0;
                            padding: 20px;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: auto;
                            background-color: #fff;
                            border-radius: 8px;
                            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                            padding: 20px;
                        }}
                        .header {{
                            text-align: center;
                            padding: 20px;
                            border-bottom: 2px solid #007bff;
                        }}
                        .header h1 {{
                            color: #007bff;
                        }}
                        .order-details {{
                            margin-top: 20px;
                        }}
                        .order-details p {{
                            font-size: 16px;
                            line-height: 1.6;
                        }}
                        .order-summary {{
                            width: 100%;
                            border-collapse: collapse;
                            margin-top: 20px;
                        }}
                        .order-summary th, .order-summary td {{
                            border: 1px solid #ddd;
                            padding: 10px;
                            text-align: left;
                        }}
                        .order-summary th {{
                            background-color: #007bff;
                            color: #fff;
                        }}
                        .footer {{
                            margin-top: 30px;
                            text-align: center;
                            font-size: 14px;
                            color: #777;
                        }}
                        @media (max-width: 600px) {{
                            .container {{
                                width: 100%;
                                padding: 10px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Order Confirmation</h1>
                            <p>Thank you for your purchase!</p>
                        </div>
                        <div class='order-details'>
                            <p>Your order has been successfully placed.</p>
                            <p><strong>Order Number:</strong> #{orderHeader.Id}</p>
                            <p><strong>Order Date:</strong> {orderHeader.OrderDate:MMMM dd, yyyy}</p>
                            <p><strong>Total Amount:</strong> {orderHeader.OrderTotal:C}</p>
                            <p><strong>Shipping Address:</strong> {orderHeader.ShipToAddress.ShippingAddress1}, {orderHeader.ShipToAddress.ShippingAddress2 ?? ""}</p>
                        </div>
                        <table class='order-summary'>
                            <thead>
                                <tr>
                                    <th>Product</th>
                                    <th>Quantity</th>
                                    <th>Price</th>
                                    <th>Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                {orderItemsHtml}
                            </tbody>
                        </table>
                        <div class='footer'>
                            <p>Thank you for shopping with us!</p>
                            <p>&copy; {DateTime.Now.Year} Your Company Name. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
        private string CreateCleanupReportEmailTemplate(CleanupReportModel report)
        {
            // Convert UTC time to IST
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var istCleanupTime = TimeZoneInfo.ConvertTimeFromUtc(report.CleanupTime, istTimeZone);
            var istThreshold = TimeZoneInfo.ConvertTimeFromUtc(report.Threshold, istTimeZone);

            return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>User Cleanup Report</title>
            <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
                    margin: 0;
                    padding: 0;
                    background-color: #f5f6fa;
                    color: #2d3436;
                }}
                .email-container {{
                     max-width: 600px;
                     margin: 20px auto;
                     background: #ffffff;
                     padding: 20px;
                     border: 1px solid #e1e1e1;
                     border-radius: 8px;
                     box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                }}
                .email-header {{
                    background: linear-gradient(135deg, #6c5ce7 0%, #a29bfe 100%);
                    color: #ffffff;
                    text-align: center;
                    padding: 30px 20px;
                    border-top-left-radius: 12px;
                    border-top-right-radius: 12px;
                }}
                .email-header h1 {{
                    margin: 0;
                    font-size: 28px;
                    font-weight: 600;
                }}
                .email-header p {{
                    margin: 5px 0 0;
                    font-size: 16px;
                    opacity: 0.9;
                }}
                .email-body {{
                    padding: 30px;
                }}
                .report-summary {{
                    background: #f8f9fa;
                    padding: 20px;
                    border-radius: 8px;
                    text-align: center;
                    margin-bottom: 20px;
                }}
                .report-summary h2 {{
                    font-size: 22px;
                    color: #6c5ce7;
                    margin: 0 0 10px;
                }}
                .report-summary p {{
                    font-size: 16px;
                    margin: 5px 0;
                    line-height: 1.6;
                }}
                .highlight {{
                    font-size: 24px;
                    font-weight: bold;
                    color: #2d3436;
                    display: inline-block;
                    padding: 5px 15px;
                    border-radius: 5px;
                    background: #dfe6e9;
                }}
                .details-section {{
                    margin-top: 20px;
                }}
                .details-section h3 {{
                    font-size: 18px;
                    color: #2d3436;
                    margin-bottom: 10px;
                    border-bottom: 2px solid #dfe6e9;
                    padding-bottom: 5px;
                }}
                .details-section p {{
                    font-size: 15px;
                    margin: 5px 0;
                    line-height: 1.6;
                }}
                .email-footer {{
                    text-align: center;
                    padding: 20px;
                    background: #f8f9fa;
                    font-size: 14px;
                    color: #636e72;
                    border-top: 1px solid #dfe6e9;
                }}
                .email-footer a {{
                    color: #6c5ce7;
                    text-decoration: none;
                    font-weight: 500;
                }}
                .email-footer a:hover {{
                    text-decoration: underline;
                }}
                @media (max-width: 600px) {{
                    .email-container {{
                        width: 100%;
                        margin: 10px;
                        border-radius: 8px;
                    }}
                    .email-header h1 {{
                        font-size: 24px;
                    }}
                    .email-header p {{
                        font-size: 14px;
                    }}
                    .email-body {{
                        padding: 20px;
                    }}
                    .report-summary h2 {{
                        font-size: 20px;
                    }}
                    .highlight {{
                        font-size: 20px;
                    }}
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='email-header'>
                    <h1>User Cleanup Report</h1>
                    <p>Angular Core Hub System Update</p>
                </div>
                <div class='email-body'>
                    <div class='report-summary'>
                        <h2>Cleanup Completed Successfully</h2>
                        <p>A scheduled user cleanup operation has been performed.</p>
                        <p>Total Users Deleted: <span class='highlight'>{report.DeletedCount}</span></p>
                    </div>
                    <div class='details-section'>
                        <h3>Details</h3>
                            <p><strong>Cleanup Time (IST):</strong> {istCleanupTime:dddd, MMMM dd, yyyy, hh:mm:ss tt}</p>
                            <p><strong>Retention Period:</strong> {report.RetentionDays} days</p>
                            <p><strong>Threshold Date:</strong> {istThreshold:dddd, MMMM dd, yyyy}</p>
                        <p>This cleanup removed user accounts that were soft-deleted before the threshold date as part of routine system maintenance.</p>
                    </div>
                </div>
                <div class='email-footer'>
                    <p>Thank you for overseeing Angular Core Hub operations.</p>
                    <p><a href='https://angularcorehub.com'>Visit Angular Core Hub</a> | <a href='https://angularcorehub.com/support'>Contact Support</a></p>
                    <p>&copy; {DateTime.Now.Year} Angular Core Hub. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>";
        }
        private string CreateWelcomeEmailTemplate(string email, WelcomeEmailRequest request)
        {
            return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Welcome to Angular Core Hub</title>
            <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
                    margin: 0;
                    padding: 0;
                    background-color: #f5f6fa;
                    color: #2d3436;
                }}
                .email-container {{
                    max-width: 650px;
                    margin: 20px auto;
                    background: #ffffff;
                    border-radius: 12px;
                    overflow: hidden;
                    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
                }}
                .email-header {{
                    background: linear-gradient(135deg, #6c5ce7 0%, #a29bfe 100%);
                    color: #ffffff;
                    text-align: center;
                    padding: 30px 20px;
                }}
                .email-header h1 {{
                    margin: 0;
                    font-size: 28px;
                    font-weight: 600;
                }}
                .email-body {{
                    padding: 30px;
                    text-align: center;
                }}
                .email-body p {{
                    font-size: 16px;
                    margin: 10px 0;
                    line-height: 1.6;
                }}
                .credential-box {{
                    background: #f8f9fa;
                    padding: 15px;
                    border-radius: 8px;
                    margin: 20px 0;
                }}
                .credential-box p {{
                    margin: 5px 0;
                }}
                .btn {{
                    display: inline-block;
                    padding: 12px 24px;
                    background: #6c5ce7;
                    color: #ffffff !important;
                    text-decoration: none;
                    border-radius: 25px;
                    font-weight: 500;
                    margin-top: 20px;
                }}
                .btn:hover {{
                    background: #a29bfe;
                }}
                .email-footer {{
                    text-align: center;
                    padding: 20px;
                    background: #f8f9fa;
                    font-size: 14px;
                    color: #636e72;
                    border-top: 1px solid #dfe6e9;
                }}
                @media (max-width: 600px) {{
                    .email-container {{
                        width: 100%;
                        margin: 10px;
                    }}
                    .email-header h1 {{
                        font-size: 24px;
                    }}
                    .email-body {{
                        padding: 20px;
                    }}
                }}
            </style>
        </head>
      <body>
            <div class='email-container'>
                <div class='email-header'>
                    <h1>Welcome to Angular Core Hub!</h1>
                </div>
                <div class='email-body'>
                    <p>Hello {request.Name},</p>
                    <p>Your account has been successfully created. Below are your login credentials:</p>
                    <div class='credential-box'>
                        <p><strong>Email:</strong> {email}</p>
                        <p><strong>Temporary Password:</strong> {request.TemporaryPassword}</p>
                    </div>
                    <div class='policy-box'>
                        <h4>Password Requirements</h4>
                        <ul>
                            <li>Minimum 8 characters</li>
                            <li>At least one uppercase letter</li>
                            <li>At least one lowercase letter</li>
                            <li>At least one number</li>
                            <li>At least one special character (e.g., !@#$%^&*)</li>
                        </ul>
                    </div>
                    <p>You will be prompted to reset your password upon first login for security.</p>
                    <a href={request.HtmlMessage} class='btn'>Log In Now</a>
                </div>
                <div class='email-footer'>
                    <p>Thank you for joining Angular Core Hub!</p>
                    <p>© {DateTime.Now.Year} Angular Core Hub. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>";
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
    }
}