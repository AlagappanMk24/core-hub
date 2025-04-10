using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Application.Contracts.DTOs.Request
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(255, ErrorMessage = "Email must be less than 255 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(255, ErrorMessage = "Full name must be less than 255 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(100, ErrorMessage = "Password must be less than 100 characters.")] // Optional: Limit length
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
              ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [NotMapped] // Important: This property should NOT be mapped to the database
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required.")]
        [StringLength(255, ErrorMessage = "Street address must be less than 255 characters.")]
        public string? StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City must be less than 100 characters.")]
        public string? City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, ErrorMessage = "State must be less than 50 characters.")]
        public string? State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required.")]
        [RegularExpression(@"^\d{6}(-\d{4})?$", ErrorMessage = "Invalid postal code. Use format XXXXXX or XXXXX-XXXX.")] // Example for US zip codes
        [StringLength(10, ErrorMessage = "Postal code must be less than 10 characters.")]
        public string? PostalCode { get; set; } = string.Empty;

        public List<string>? Roles { get; set; }
    }
}
