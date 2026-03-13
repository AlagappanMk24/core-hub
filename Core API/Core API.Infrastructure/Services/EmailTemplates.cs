using Core_API.Domain.Entities;
using Core_API.Domain.Models.Email;

namespace Core_API.Infrastructure.Services
{
    public static class EmailTemplates
    {
        public static string CreateEmailTemplate(string content)
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
        public static string CreateInvoiceEmailTemplate(InvoiceEmailTemplateModel model)
        {
            string safeContent = string.IsNullOrWhiteSpace(model.Content) ? "Please find your invoice details below." : model.Content;
            string amountDueText = model.AmountDue.HasValue ? $"{model.AmountDue:C}" : "Not specified";
            string dueDateText = model.DueDate.HasValue ? $"{model.DueDate:MMMM dd, yyyy}" : "Not specified";
            string invoiceNumber = string.IsNullOrWhiteSpace(model.InvoiceNumber) ? "N/A" : model.InvoiceNumber;
            string logoUrl = string.IsNullOrWhiteSpace(model.LogoUrl) ? "https://angularcorehub.com/images/angularcore.png" : model.LogoUrl;

            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Invoice #{invoiceNumber} - Angular Core Hub</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', 'Helvetica Neue', Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f5f6fa;
                        color: #2d3436;
                    }}
                    .email-container {{
                        max-width: 700px;
                        width: 100%;
                        margin: 20px auto;
                        background: #ffffff;
                        border-radius: 16px;
                        overflow: hidden;
                        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15);
                    }}
                    .email-header {{
                        background: linear-gradient(135deg, #5b4ce0 0%, #8c7cf6 100%);
                        color: #ffffff;
                        text-align: center;
                        padding: 40px 20px;
                        position: relative;
                    }}
                    .email-header img {{
                        max-width: 160px;
                        width: 100%;
                        height: auto;
                        margin-bottom: 15px;
                        display: block;
                        margin-left: auto;
                        margin-right: auto;
                    }}
                    .email-header h1 {{
                        margin: 0;
                        font-size: 34px;
                        font-weight: 700;
                        letter-spacing: 0.5px;
                    }}
                    .email-header p {{
                        margin: 10px 0 0;
                        font-size: 16px;
                        opacity: 0.9;
                    }}
                    .email-body {{
                        padding: 30px;
                    }}
                    .invoice-details {{
                        background: #f8f9fa;
                        padding: 25px;
                        border-radius: 10px;
                        margin-bottom: 25px;
                        text-align: center;
                        border: 1px solid #dfe6e9;
                    }}
                    .invoice-details h2 {{
                        font-size: 24px;
                        color: #5b4ce0;
                        margin: 0 0 15px;
                        font-weight: 600;
                    }}
                    .invoice-details p {{
                        font-size: 16px;
                        margin: 10px 0;
                        line-height: 1.6;
                    }}
                    .highlight {{
                        font-size: 26px;
                        font-weight: bold;
                        color: #2d3436;
                        background: #dfe6e9;
                        padding: 10px 20px;
                        border-radius: 8px;
                        display: inline-block;
                    }}
                    .message-content {{
                        font-size: 16px;
                        line-height: 1.7;
                        margin: 25px 0;
                        white-space: pre-line;
                        color: #2d3436;
                        text-align: left;
                    }}
                    .btn {{
                        display: inline-block;
                        padding: 14px 30px;
                        background: linear-gradient(135deg, #5b4ce0 0%, #8c7cf6 100%);
                        color: #ffffff !important;
                        text-decoration: none;
                        border-radius: 50px;
                        font-weight: 600;
                        font-size: 16px;
                        margin: 20px auto;
                        display: block;
                        width: fit-content;
                        transition: transform 0.2s ease, opacity 0.2s ease;
                    }}
                    .btn:hover {{
                        opacity: 0.9;
                        transform: translateY(-2px);
                    }}
                    .attachment-notice {{
                        font-size: 14px;
                        color: #636e72;
                        margin-top: 20px;
                        text-align: center;
                        font-style: italic;
                    }}
                    .email-footer {{
                        text-align: center;
                        padding: 25px;
                        background: #f8f9fa;
                        font-size: 14px;
                        color: #636e72;
                        border-top: 1px solid #dfe6e9;
                    }}
                    .email-footer a {{
                        color: #5b4ce0;
                        text-decoration: none;
                        font-weight: 600;
                        margin: 0 10px;
                    }}
                    .email-footer a:hover {{
                        text-decoration: underline;
                    }}
                    /* Tablet */
                    @media (max-width: 768px) {{
                        .email-container {{
                            max-width: 90%;
                            margin: 15px auto;
                            border-radius: 12px;
                        }}
                        .email-header h1 {{
                            font-size: 28px;
                        }}
                        .email-header p {{
                            font-size: 15px;
                        }}
                        .email-body {{
                            padding: 25px;
                        }}
                        .invoice-details h2 {{
                            font-size: 22px;
                        }}
                        .highlight {{
                            font-size: 24px;
                        }}
                        .btn {{
                            padding: 12px 25px;
                            font-size: 15px;
                        }}
                    }}
                    /* Mobile */
                    @media (max-width: 480px) {{
                        .email-container {{
                            max-width: 95%;
                            margin: 10px auto;
                            border-radius: 10px;
                        }}
                        .email-header {{
                            padding: 30px 15px;
                        }}
                        .email-header h1 {{
                            font-size: 24px;
                        }}
                        .email-header p {{
                            font-size: 14px;
                        }}
                        .email-header img {{
                            max-width: 120px;
                        }}
                        .email-body {{
                            padding: 20px;
                        }}
                        .invoice-details h2 {{
                            font-size: 20px;
                        }}
                        .invoice-details p {{
                            font-size: 15px;
                        }}
                        .highlight {{
                            font-size: 20px;
                            padding: 8px 15px;
                        }}
                        .message-content {{
                            font-size: 15px;
                        }}
                        .btn {{
                            padding: 10px 20px;
                            font-size: 14px;
                        }}
                    }}
                    /* Small Mobile */
                    @media (max-width: 320px) {{
                        .email-container {{
                            max-width: 100%;
                            margin: 5px;
                            border-radius: 8px;
                        }}
                        .email-header h1 {{
                            font-size: 20px;
                        }}
                        .email-header p {{
                            font-size: 12px;
                        }}
                        .email-header img {{
                            max-width: 100px;
                        }}
                        .btn {{
                            padding: 8px 15px;
                            font-size: 13px;
                        }}
                    }}
                    @keyframes fadeIn {{
                        from {{ opacity: 0; transform: translateY(10px); }}
                        to {{ opacity: 1; transform: translateY(0); }}
                    }}
                    .invoice-details, .btn, .message-content {{
                        animation: fadeIn 0.5s ease-out;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <img src='{logoUrl}' alt='Angular Core Hub Logo'>
                        <h1>Your Invoice from Angular Core Hub</h1>
                        <p>Invoice #{invoiceNumber}</p>
                    </div>
                    <div class='email-body'>
                        <div class='invoice-details'>
                            <h2>Invoice Summary</h2>
                            <p><strong>Invoice Number:</strong> #{invoiceNumber}</p>
                            <p><strong>Amount Due:</strong> <span class='highlight'>{amountDueText}</span></p>
                            <p><strong>Due Date:</strong> {dueDateText}</p>
                        </div>
                        <div class='message-content'>
                            {safeContent}
                        </div>
                        <div style='text-align: center;'>
                            <a href='https://angularcorehub.com/invoice' class='btn'>View & Pay Invoice</a>
                        </div>
                        {(model.HasAttachment ? "<div class='attachment-notice'><p>A PDF copy of your invoice is attached for your records.</p></div>" : "")}
                    </div>
                    <div class='email-footer'>
                        <p>Thank you for your business!</p>
                        <p>
                            <a href='https://angularcorehub.com'>Visit Angular Core Hub</a> |
                            <a href='https://angularcorehub.com/support'>Contact Support</a>
                        </p>
                        <p>&copy; {DateTime.Now.Year} Angular Core Hub. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        public static string CreateOtpEmailTemplate(string code)
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
        public static string CreateCleanupReportEmailTemplate(CleanupReportModel report)
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
        public static string CreateWelcomeEmailTemplate(string email, WelcomeEmailRequest request)
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
        public static string GetCompanyRequestAdminEmail(string adminName, CompanyRequest request, string reviewLink)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>New Company Registration Request</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f5f6fa;
                    }}
                    .email-wrapper {{
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        border-radius: 16px;
                        overflow: hidden;
                        box-shadow: 0 10px 25px rgba(0,0,0,0.1);
                    }}
                    .email-header {{
                        background: linear-gradient(135deg, #8b5cf6, #6d28d9);
                        color: white;
                        padding: 30px;
                        text-align: center;
                    }}
                    .email-header h1 {{
                        margin: 0;
                        font-size: 28px;
                        font-weight: 600;
                    }}
                    .email-header p {{
                        margin: 10px 0 0;
                        opacity: 0.9;
                    }}
                    .email-body {{
                        padding: 30px;
                    }}
                    .request-card {{
                        background: #f8fafc;
                        border-radius: 12px;
                        padding: 25px;
                        margin-bottom: 25px;
                        border: 1px solid #e2e8f0;
                    }}
                    .detail-row {{
                        display: flex;
                        margin-bottom: 15px;
                        padding-bottom: 15px;
                        border-bottom: 1px solid #e2e8f0;
                    }}
                    .detail-row:last-child {{
                        border-bottom: none;
                        margin-bottom: 0;
                        padding-bottom: 0;
                    }}
                    .detail-label {{
                        width: 120px;
                        font-weight: 600;
                        color: #4b5563;
                    }}
                    .detail-value {{
                        flex: 1;
                        color: #1f2937;
                    }}
                    .badge {{
                        background: #fef3c7;
                        color: #92400e;
                        padding: 4px 12px;
                        border-radius: 20px;
                        font-size: 12px;
                        font-weight: 600;
                        display: inline-block;
                    }}
                    .action-buttons {{
                        text-align: center;
                        margin: 30px 0 20px;
                    }}
                    .btn-primary {{
                        display: inline-block;
                        background: #8b5cf6;
                        color: white !important;
                        text-decoration: none;
                        padding: 14px 30px;
                        border-radius: 8px;
                        font-weight: 600;
                        margin: 0 10px;
                        transition: background 0.3s;
                    }}
                    .btn-primary:hover {{
                        background: #7c3aed;
                    }}
                    .btn-secondary {{
                        display: inline-block;
                        background: #4b5563;
                        color: white !important;
                        text-decoration: none;
                        padding: 14px 30px;
                        border-radius: 8px;
                        font-weight: 600;
                        margin: 0 10px;
                        transition: background 0.3s;
                    }}
                    .btn-secondary:hover {{
                        background: #374151;
                    }}
                    .info-box {{
                        background: #e0f2fe;
                        border-left: 4px solid #0284c7;
                        padding: 15px;
                        border-radius: 8px;
                        margin-top: 25px;
                    }}
                    .info-box p {{
                        margin: 0;
                        color: #0369a1;
                        font-size: 14px;
                    }}
                    .email-footer {{
                        background: #f8fafc;
                        padding: 20px;
                        text-align: center;
                        border-top: 1px solid #e2e8f0;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                    @media (max-width: 480px) {{
                        .detail-row {{
                            flex-direction: column;
                        }}
                        .detail-label {{
                            width: 100%;
                            margin-bottom: 5px;
                        }}
                        .action-buttons a {{
                            display: block;
                            margin: 10px 0;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class='email-wrapper'>
                    <div class='email-header'>
                        <h1>🏢 New Company Request</h1>
                        <p>Action required: Review company registration request</p>
                    </div>
                    <div class='email-body'>
                        <div class='request-card'>
                            <h2 style='margin-top:0; color:#374151;'>Request Details</h2>
                            
                            <div class='detail-row'>
                                <div class='detail-label'>Request ID:</div>
                                <div class='detail-value'><strong>#{request.Id}</strong></div>
                            </div>
                            
                            <div class='detail-row'>
                                <div class='detail-label'>Requester:</div>
                                <div class='detail-value'>{request.FullName}</div>
                            </div>
                            
                            <div class='detail-row'>
                                <div class='detail-label'>Email:</div>
                                <div class='detail-value'>{request.Email}</div>
                            </div>
                            
                            <div class='detail-row'>
                                <div class='detail-label'>Company:</div>
                                <div class='detail-value'><strong>{request.CompanyName}</strong></div>
                            </div>
                            
                            <div class='detail-row'>
                                <div class='detail-label'>Requested:</div>
                                <div class='detail-value'>{request.RequestedAt:MMMM dd, yyyy hh:mm tt}</div>
                            </div>
                            
                            <div class='detail-row'>
                                <div class='detail-label'>Status:</div>
                                <div class='detail-value'><span class='badge'>PENDING</span></div>
                            </div>
                        </div>

                        <div class='action-buttons'>
                            <a href='{reviewLink}' class='btn-primary'>🔍 Review Request</a>
                            <a href='{reviewLink.Replace("/" + request.Id, "")}' class='btn-secondary'>📋 View All Requests</a>
                        </div>

                        <div class='info-box'>
                            <p>⏱️ This request is pending for {(DateTime.UtcNow - request.RequestedAt).Days} days. Please review and take appropriate action.</p>
                        </div>
                    </div>
                    <div class='email-footer'>
                        <p>Angular Core Hub - Company Registration System</p>
                        <p>© {DateTime.Now.Year} Angular Core Hub. All rights reserved.</p>
                        <p style='font-size:12px;'>If the button doesn't work, copy and paste this link: {reviewLink}</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        public static string GetCompanyApprovedEmail(string userName, string companyName, string loginLink)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Company Registration Approved</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f5f6fa;
                    }}
                    .email-wrapper {{
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        border-radius: 16px;
                        overflow: hidden;
                        box-shadow: 0 10px 25px rgba(0,0,0,0.1);
                    }}
                    .email-header {{
                        background: linear-gradient(135deg, #10b981, #059669);
                        color: white;
                        padding: 30px;
                        text-align: center;
                    }}
                    .email-header h1 {{
                        margin: 0;
                        font-size: 28px;
                    }}
                    .email-body {{
                        padding: 30px;
                        text-align: center;
                    }}
                    .success-icon {{
                        font-size: 60px;
                        margin-bottom: 20px;
                    }}
                    .company-name {{
                        background: #f0fdf4;
                        color: #059669;
                        padding: 10px 20px;
                        border-radius: 30px;
                        font-size: 18px;
                        font-weight: bold;
                        display: inline-block;
                        margin: 20px 0;
                    }}
                    .btn {{
                        display: inline-block;
                        background: #10b981;
                        color: white;
                        text-decoration: none;
                        padding: 14px 30px;
                        border-radius: 8px;
                        font-weight: 600;
                        margin-top: 20px;
                    }}
                    .btn:hover {{
                        background: #059669;
                    }}
                    .email-footer {{
                        background: #f8fafc;
                        padding: 20px;
                        text-align: center;
                        color: #6b7280;
                    }}
                </style>
            </head>
            <body>
                <div class='email-wrapper'>
                    <div class='email-header'>
                        <h1>✅ Company Registration Approved</h1>
                    </div>
                    <div class='email-body'>
                        <div class='success-icon'>🎉</div>
                        <h2>Congratulations, {userName}!</h2>
                        <p>Your request to register <span class='company-name'>{companyName}</span> has been approved.</p>
                        <p>You can now log in and start using Angular Core Hub with your company.</p>
                        <a href='{loginLink}' class='btn'>Log In to Your Account</a>
                        <p style='margin-top: 30px; color: #6b7280;'>If you have any questions, please contact our support team.</p>
                    </div>
                    <div class='email-footer'>
                        <p>Angular Core Hub - Where Business Meets Technology</p>
                    </div>
                </div>
            </body>
            </html>";
        }
        public static string GetCompanyRejectedEmail(string userName, string companyName, string reason, string supportLink)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Company Registration Update</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f5f6fa;
                    }}
                    .email-wrapper {{
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        border-radius: 16px;
                        overflow: hidden;
                        box-shadow: 0 10px 25px rgba(0,0,0,0.1);
                    }}
                    .email-header {{
                        background: linear-gradient(135deg, #ef4444, #dc2626);
                        color: white;
                        padding: 30px;
                        text-align: center;
                    }}
                    .email-header h1 {{
                        margin: 0;
                        font-size: 28px;
                    }}
                    .email-body {{
                        padding: 30px;
                        text-align: center;
                    }}
                    .reason-box {{
                        background: #fef2f2;
                        border: 1px solid #fee2e2;
                        border-radius: 8px;
                        padding: 20px;
                        margin: 20px 0;
                        text-align: left;
                    }}
                    .reason-box h3 {{
                        color: #991b1b;
                        margin-top: 0;
                    }}
                    .reason-box p {{
                        color: #7f1d1d;
                        margin-bottom: 0;
                    }}
                    .btn {{
                        display: inline-block;
                        background: #4b5563;
                        color: white;
                        text-decoration: none;
                        padding: 12px 25px;
                        border-radius: 8px;
                        font-weight: 600;
                        margin-top: 20px;
                    }}
                    .btn:hover {{
                        background: #374151;
                    }}
                    .email-footer {{
                        background: #f8fafc;
                        padding: 20px;
                        text-align: center;
                        color: #6b7280;
                    }}
                </style>
            </head>
            <body>
                <div class='email-wrapper'>
                    <div class='email-header'>
                        <h1>ℹ️ Company Registration Update</h1>
                    </div>
                    <div class='email-body'>
                        <p>Dear {userName},</p>
                        <p>Regarding your request to register <strong>{companyName}</strong>:</p>
                        
                        <div class='reason-box'>
                            <h3>Status: Not Approved</h3>
                            <p><strong>Reason:</strong> {reason}</p>
                        </div>
                        
                        <p>If you believe this is an error or would like to discuss further, please contact our support team.</p>
                        
                        <a href='{supportLink}' class='btn'>Contact Support</a>
                    </div>
                    <div class='email-footer'>
                        <p>Angular Core Hub - We're here to help</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}