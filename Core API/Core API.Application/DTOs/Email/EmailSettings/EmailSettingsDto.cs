using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Email.EmailSettings;

public class EmailSettingsDto
{
    [Required]
    [EmailAddress]
    public string FromEmail { get; set; }
}
