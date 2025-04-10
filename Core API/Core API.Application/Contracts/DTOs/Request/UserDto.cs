using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Contracts.DTOs.Request
{
    public class UserDto
    {
        [StringLength(255, ErrorMessage = "Full name must be less than 255 characters.")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(255, ErrorMessage = "Email must be less than 255 characters.")]
        public string? Email { get; set; }
        [RegularExpression(@"^(\+\d{1,3})?[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Invalid phone number.")] // Example Regex
        [StringLength(20, ErrorMessage = "Phone number must be less than 20 characters.")]
        public string? PhoneNumber { get; set; }

        [StringLength(255, ErrorMessage = "Street address must be less than 255 characters.")]
        public string? StreetAddress { get; set; }

        [StringLength(100, ErrorMessage = "City must be less than 100 characters.")]
        public string? City { get; set; }

        [StringLength(50, ErrorMessage = "State must be less than 50 characters.")]
        public string? State { get; set; }

        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid postal code. Use format XXXXX or XXXXX-XXXX.")] // US Example
        [StringLength(10, ErrorMessage = "Postal code must be less than 10 characters.")]
        public string? PostalCode { get; set; }
    }
}
