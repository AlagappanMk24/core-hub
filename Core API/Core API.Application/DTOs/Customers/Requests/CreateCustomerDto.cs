using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Customer.Request
{
    /// <summary>
    /// Data transfer object for creating a new customer.
    /// </summary>
    public class CreateCustomerDto
    {
        /// <summary>
        /// Gets or sets the customer's full name.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer's tax ID (GST/VAT).
        /// </summary>
        [StringLength(50, ErrorMessage = "Tax ID cannot exceed 50 characters")]
        public string? TaxId { get; set; }

        /// <summary>
        /// Gets or sets the customer's website URL.
        /// </summary>
        [StringLength(200, ErrorMessage = "Website cannot exceed 200 characters")]
        [Url(ErrorMessage = "Invalid website URL")]
        public string? Website { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit limit.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Credit limit must be a positive number")]
        public decimal CreditLimit { get; set; } = 0;

        /// <summary>
        /// Gets or sets the default payment terms.
        /// </summary>
        [StringLength(50, ErrorMessage = "Payment terms cannot exceed 50 characters")]
        public string? DefaultPaymentTerms { get; set; }

        /// <summary>
        /// Gets or sets the default currency.
        /// </summary>
        [StringLength(3, ErrorMessage = "Currency code must be 3 characters")]
        public string? DefaultCurrency { get; set; }

        /// <summary>
        /// Gets or sets the customer group ID.
        /// </summary>
        public int? CustomerGroupId { get; set; }

        /// <summary>
        /// Gets or sets the primary address line.
        /// </summary>
        [Required(ErrorMessage = "Address line 1 is required")]
        [StringLength(200, ErrorMessage = "Address line 1 cannot exceed 200 characters")]
        public string AddressLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the secondary address line.
        /// </summary>
        [StringLength(200, ErrorMessage = "Address line 2 cannot exceed 200 characters")]
        public string? AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state or province.
        /// </summary>
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the country code (2 letters).
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal or ZIP code.
        /// </summary>
        [Required(ErrorMessage = "Zip code is required")]
        [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
        public string ZipCode { get; set; } = string.Empty;
    }
}
