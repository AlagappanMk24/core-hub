namespace Core_API.Application.DTOs.Email.Requests
{
    public class SendCustomerEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool AttachPdf { get; set; }
        public bool SendCopyToSelf { get; set; }
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public string Type { get; set; } = "custom";
    }
}