using Core_API.Application.Contracts.Services.Email;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Models.Email;

namespace Core_API.Infrastructure.Services.Communication.Email
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string RenderInvoiceTemplate(InvoiceEmailData data)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Invoice #{data.InvoiceNumber}</title>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f6fa; }}
                    .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 28px; }}
                    .content {{ padding: 30px; }}
                    .invoice-details {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .amount {{ font-size: 32px; font-weight: bold; color: #667eea; }}
                    .footer {{ text-align: center; padding: 20px; background: #f8f9fa; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Invoice #{data.InvoiceNumber}</h1>
                    </div>
                    <div class='content'>
                        <p>Dear {data.CustomerName},</p>
                        <div class='invoice-details'>
                            <p><strong>Amount Due:</strong> <span class='amount'>${data.AmountDue:N2}</span></p>
                            <p><strong>Due Date:</strong> {data.DueDate:MMMM dd, yyyy}</p>
                        </div>
                        <div>{data.Content}</div>
                        {(data.HasAttachment ? "<p><strong>📎 A PDF copy of your invoice is attached.</strong></p>" : "")}
                    </div>
                    <div class='footer'>
                        <p>Thank you for your business!</p>
                        <p>&copy; {DateTime.Now.Year} {data.CompanyName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        public string RenderCustomerEmailTemplate(CustomerEmailData data)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Message from {data.CompanyName}</title>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f6fa; }}
                    .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                    .content {{ padding: 30px; }}
                    .footer {{ text-align: center; padding: 20px; background: #f8f9fa; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>{data.CompanyName}</h1>
                    </div>
                    <div class='content'>
                        <p>Dear {data.CustomerName},</p>
                        <div>{data.Content}</div>
                    </div>
                    <div class='footer'>
                        <p>&copy; {DateTime.Now.Year} {data.CompanyName}. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        public string RenderOtpTemplate(string otpCode)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Your OTP Code</title>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f6fa; }}
                    .container {{ max-width: 500px; margin: 50px auto; background: white; padding: 30px; border-radius: 12px; text-align: center; }}
                    .code {{ font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Your Verification Code</h2>
                    <div class='code'>{otpCode}</div>
                    <p>This code will expire in 10 minutes.</p>
                </div>
            </body>
            </html>";
        }
        public string RenderWelcomeTemplate(WelcomeEmailData data)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Welcome to CoreInvoice</title>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5f6fa; }}
                    .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                    .content {{ padding: 30px; }}
                    .credentials {{ background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0; }}
                    .btn {{ display: inline-block; background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to CoreInvoice!</h1>
                    </div>
                    <div class='content'>
                        <p>Dear {data.Name},</p>
                        <p>Your account has been successfully created.</p>
                        <div class='credentials'>
                            <p><strong>Email:</strong> {data.Email}</p>
                            <p><strong>Temporary Password:</strong> {data.TemporaryPassword}</p>
                        </div>
                        <a href='{data.LoginLink}' class='btn'>Login to Your Account</a>
                        <p style='margin-top: 20px;'>Please change your password after first login.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        public string RenderCleanupReportTemplate(CleanupReportModel report)
        {
            return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>User Cleanup Report</title>
        <style>
            body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5f6fa; margin: 0; padding: 0; }}
            .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
            .content {{ padding: 30px; }}
            .stats {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; text-align: center; }}
            .stat-number {{ font-size: 48px; font-weight: bold; color: #667eea; }}
            .footer {{ text-align: center; padding: 20px; background: #f8f9fa; font-size: 12px; color: #666; }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>User Cleanup Report</h1>
            </div>
            <div class='content'>
                <h2>Soft-Deleted Users Cleanup</h2>
                <div class='stats'>
                    <div class='stat-number'>{report.DeletedCount}</div>
                    <p>Users permanently deleted</p>
                </div>
                <p><strong>Cleanup Time:</strong> {report.CleanupTime:yyyy-MM-dd HH:mm:ss} UTC</p>
                <p><strong>Retention Period:</strong> {report.RetentionDays} days</p>
                <p><strong>Threshold Date:</strong> {report.Threshold:yyyy-MM-dd}</p>
                <p>Users soft-deleted before the threshold date have been permanently removed from the system.</p>
            </div>
            <div class='footer'>
                <p>CoreInvoice System - Automated Cleanup Report</p>
            </div>
        </div>
    </body>
    </html>";
        }
        public string RenderCompanyRequestAdminTemplate(CompanyRequest request, string reviewLink, string allRequestsLink)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>New Company Request</title>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .button {{ padding: 12px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 6px; display: inline-block; }}
            </style>
        </head>
        <body>
            <h2>New Company Registration Request</h2>
            <p><strong>Request ID:</strong> {request.Id}</p>
            <p><strong>Company Name:</strong> {request.CompanyName}</p>
            <p><strong>Requested By:</strong> {request.FullName} ({request.Email})</p>
            <p><strong>Requested At:</strong> {request.RequestedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
            
            <br/>
            <a href='{reviewLink}' class='button'>Review This Request</a>
            &nbsp;&nbsp;
            <a href='{allRequestsLink}' class='button' style='background-color: #6c757d;'>View All Requests</a>
            
            <br/><br/>
            <p>This is an automated notification from CoreInvoice System.</p>
        </body>
        </html>";
        }
    }
}