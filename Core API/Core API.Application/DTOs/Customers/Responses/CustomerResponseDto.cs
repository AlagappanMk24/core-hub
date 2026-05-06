// Core_API.Application/DTOs/Customer/CustomerResponseDto.cs
namespace Core_API.Application.DTOs.Customer.Response
{
    /// <summary>
    /// Data transfer object for customer response.
    /// </summary>
    public class CustomerResponseDto
    {
        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the customer's full name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's tax ID (GST/VAT).
        /// </summary>
        public string? TaxId { get; set; }

        /// <summary>
        /// Gets or sets the customer's website URL.
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit limit.
        /// </summary>
        public decimal CreditLimit { get; set; }

        /// <summary>
        /// Gets or sets the default payment terms.
        /// </summary>
        public string? DefaultPaymentTerms { get; set; }

        /// <summary>
        /// Gets or sets the default currency.
        /// </summary>
        public string? DefaultCurrency { get; set; }

        /// <summary>
        /// Gets or sets the customer group ID.
        /// </summary>
        public int? CustomerGroupId { get; set; }

        /// <summary>
        /// Gets or sets the customer group name.
        /// </summary>
        public string? CustomerGroupName { get; set; }

        /// <summary>
        /// Gets or sets the customer's status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date when customer became active.
        /// </summary>
        public DateTime? ActiveSince { get; set; }

        /// <summary>
        /// Gets or sets the last purchase date.
        /// </summary>
        public DateTime? LastPurchaseDate { get; set; }

        /// <summary>
        /// Gets or sets the total purchases amount.
        /// </summary>
        public decimal TotalPurchases { get; set; }

        /// <summary>
        /// Gets or sets the average payment days.
        /// </summary>
        public int? AveragePaymentDays { get; set; }

        /// <summary>
        /// Gets or sets the company ID.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the primary address line.
        /// </summary>
        public string AddressLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the secondary address line.
        /// </summary>
        public string? AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state or province.
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country name.
        /// </summary>
        public string CountryName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal or ZIP code.
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company name (useful for Super Admin view)
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}