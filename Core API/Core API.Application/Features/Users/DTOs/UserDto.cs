using Core_API.Domain.Exceptions;
using Core_API.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Features.Users.DTOs
{
    public class UserDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name can't exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\.'-]{2,}$", ErrorMessage = "Name should only contain letters, spaces, dots, apostrophes, or hyphens")]
        public string FullName { get; set; }

        private string _email;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email
        {
            get => _email;
            set
            {
                try
                {
                    var validatedEmail = new Email(value);
                    _email = validatedEmail.ToString();
                }
                catch (InvalidEmailException ex)
                {
                    throw new ValidationException(ex.Message, null, value);
                }
            }
        }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
            ErrorMessage = "Password must include uppercase, lowercase, digit, and special character")]
        public string? Password { get; set; } // Optional for updates

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        private string _phoneNumber;
        [Required(ErrorMessage = "Phone Number is required")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _phoneNumber = null;
                    return;
                }

                if (value.StartsWith("+"))
                {
                    CountryCode = DetectCountryFromPhoneNumber(value);
                }

                // Call TryCreate on the PhoneNumber record type, not on the string
                if (!Domain.ValueObjects.PhoneNumber.TryCreate(value, CountryCode ?? "US", out var validatedPhone, out var errorMessage))
                {
                    // This will trigger ModelState.IsValid = false
                    throw new ValidationException(errorMessage);
                }
                // Store the fully formatted phone number
                _phoneNumber = validatedPhone.ToString();
            }
        }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid company")]
        public int? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public List<string> Roles { get; set; } = new();

        // Address Fields
        [Required(ErrorMessage = "Street Address is required")]
        [StringLength(100, ErrorMessage = "Street Address can't exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s,\.'-]{5,}$", ErrorMessage = "Please enter a valid address")]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City can't exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\.'-]{2,}$", ErrorMessage = "Please enter a valid city name")]
        public string City { get; set; }

        [Required(ErrorMessage = "State/Province is required")]
        [StringLength(50, ErrorMessage = "State/Province can't exceed 50 characters")]
        [Display(Name = "State/Province")]
        public string State { get; set; }

        [Required(ErrorMessage = "Postal Code is required")]
        [StringLength(20, ErrorMessage = "Postal Code can't exceed 20 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s-]{3,}$", ErrorMessage = "Please enter a valid postal code")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        public string? ProfileImageUrl { get; set; }

        // For phone validation
        [NotMapped] // This won't be mapped to database
        public string? CountryCode { get; set; } = "US";

        // Backward compatibility property - combines all address fields for legacy support
        //[NotMapped]
        //public string FullAddress =>
        //    $"{Address1}{(!string.IsNullOrEmpty(Address2) ? ", " + Address2 : "")}, {City}, {State} {PostalCode}";

        /// <summary>
        /// Helper method to detect country code from an international format phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number with international prefix</param>
        /// <returns>Two-letter country code or default</returns>
        private string DetectCountryFromPhoneNumber(string phoneNumber)
        {
            // Check the prefix after "+" to determine the country
            if (phoneNumber.StartsWith("+1"))
                return "US"; // Could be US or CA (North America)
            else if (phoneNumber.StartsWith("+91"))
                return "IN"; // India
            else if (phoneNumber.StartsWith("+61"))
                return "AU"; // Australia
            else if (phoneNumber.StartsWith("+44"))
                return "UK"; // United Kingdom
            else if (phoneNumber.StartsWith("+7"))
                return "RU"; // Russia
            else if (phoneNumber.StartsWith("+86"))
                return "CN"; // China
                             // Add more country codes as needed

            return CountryCode ?? "US"; // Default to existing value or US
        }
    }
}
