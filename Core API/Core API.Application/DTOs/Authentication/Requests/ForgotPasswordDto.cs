using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Auth.Requests
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "The email field is required.")]
        [EmailAddress(ErrorMessage = "The email field is not a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}