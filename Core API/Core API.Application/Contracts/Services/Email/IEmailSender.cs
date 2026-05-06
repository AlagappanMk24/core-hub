namespace Core_API.Application.Contracts.Services.Email
{
    /// <summary>
    /// Low-level email sender abstraction
    /// </summary>
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message);
        Task SendWithAttachmentAsync(EmailMessage message, AttachmentFile attachment);
    }
    public class EmailMessage
    {
        public string From { get; set; } = string.Empty;
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
    }
    public class AttachmentFile
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/pdf";
    }
}
