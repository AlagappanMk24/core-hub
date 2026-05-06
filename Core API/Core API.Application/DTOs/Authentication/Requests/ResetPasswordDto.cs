using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Auth.Requests
{
    public class ResetPasswordDto : PasswordChangeBaseDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
