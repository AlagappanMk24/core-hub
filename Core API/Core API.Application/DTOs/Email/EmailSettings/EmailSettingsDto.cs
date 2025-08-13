using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.EmailDto.EmailSettings
{
    public class EmailSettingsDto
    {
        [Required]
        [EmailAddress]
        public string FromEmail { get; set; }
    }
}
