using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Models.Email;

namespace Core_API.Application.Contracts.Services.Email
{
    public interface IEmailTemplateService
    {
        string RenderInvoiceTemplate(InvoiceEmailData data);
        string RenderCustomerEmailTemplate(CustomerEmailData data);
        string RenderOtpTemplate(string otpCode);
        string RenderWelcomeTemplate(WelcomeEmailData data);
        string RenderCleanupReportTemplate(CleanupReportModel report);
        string RenderCompanyRequestAdminTemplate(CompanyRequest request, string reviewLink, string allRequestsLink);
    }
    public class InvoiceEmailData
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal? AmountDue { get; set; }
        public DateTime? DueDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool HasAttachment { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "CoreInvoice";
    }

    public class CustomerEmailData
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "CoreInvoice";
        public string Type { get; set; } = "custom";
    }

    public class WelcomeEmailData
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
        public string LoginLink { get; set; } = string.Empty;
    }
}
