using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Email.Requests;

public class EmailSettingsDto
{
    [Required]
    [EmailAddress]
    public string FromEmail { get; set; }
    public int CompanyId { get; set; }
}