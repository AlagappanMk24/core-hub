namespace Core_API.Application.DTOs.Email;

public class EmailDataDto
{
    public List<string> To { get; set; } = new List<string>();
    public List<string> Cc { get; set; } = new List<string>();
    public string Subject { get; set; }
    public string Message { get; set; }
    public bool AttachPdf { get; set; }
    public bool SendCopyToSelf { get; set; }
}