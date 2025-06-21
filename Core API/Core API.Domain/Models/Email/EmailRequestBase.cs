using Core_API.Domain.Entities;

namespace Core_API.Domain.Models.Email
{
    /// <summary>
    /// Base model for all email requests
    /// </summary>
    public abstract class EmailRequestBase
    {
        public string Email { get; set; }
        public string Subject { get; set; }
    }

    /// <summary>
    /// Model for basic email requests
    /// </summary>
    public class EmailRequest : EmailRequestBase
    {
        public string HtmlMessage { get; set; }
    }

    /// <summary>
    /// Model for 2FA code email requests
    /// </summary>
    public class TwoFactorAuthEmailRequest : EmailRequestBase
    {
        public string Token { get; set; }

        public TwoFactorAuthEmailRequest()
        {
            Subject = "Your Two-Factor Authentication Code";
        }
    }

    /// <summary>
    /// Model for order confirmation email requests
    /// </summary>
    public class OrderConfirmationEmailRequest : EmailRequestBase
    {
        public OrderHeader OrderHeader { get; set; }
    }

    /// <summary>
    /// Model for cleanup report email requests
    /// </summary>
    public class CleanupReportEmailRequest
    {
        public IEnumerable<string> ToEmails { get; set; }
        public CleanupReportModel Report { get; set; }
        public CleanupReportEmailRequest()
        {
            // Default subject
            ToEmails = new List<string>();
        }
    }

    /// <summary>
    /// Model for clean up report modal
    /// </summary>
    public class CleanupReportModel
    {
        public int DeletedCount { get; set; }
        public DateTime CleanupTime { get; set; }
        public int RetentionDays { get; set; }
        public DateTime Threshold { get; set; }
    }

    /// <summary>
    /// Model for welcome email requests
    /// </summary>
    public class WelcomeEmailRequest : EmailRequestBase
    {
        public string Name { get; set; }
        public string TemporaryPassword { get; set; }
        public string HtmlMessage { get; set; }
        public WelcomeEmailRequest()
        {
            Subject = "Welcome to Angular Core Hub - Login to Set Your Password";
        }
    }
}
