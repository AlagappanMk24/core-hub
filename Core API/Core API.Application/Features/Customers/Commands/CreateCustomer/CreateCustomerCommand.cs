using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Commands.CreateCustomer
{
    /// <summary>
    /// Command to create a new customer.
    /// </summary>
    public record CreateCustomerCommand : BaseCommand<CustomerResponseDto>
    {
        /// <summary>
        /// Gets the customer's full name.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the customer's email address.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the customer's phone number.
        /// </summary>
        public string PhoneNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets the customer's tax ID (GST/VAT).
        /// </summary>
        public string? TaxId { get; init; }

        /// <summary>
        /// Gets the customer's website URL.
        /// </summary>
        public string? Website { get; init; }

        /// <summary>
        /// Gets the customer's credit limit.
        /// </summary>
        public decimal CreditLimit { get; init; }

        /// <summary>
        /// Gets the default payment terms.
        /// </summary>
        public string? DefaultPaymentTerms { get; init; }

        /// <summary>
        /// Gets the default currency.
        /// </summary>
        public string? DefaultCurrency { get; init; }

        /// <summary>
        /// Gets the customer group ID.
        /// </summary>
        public int? CustomerGroupId { get; init; }

        /// <summary>
        /// Gets the primary address line.
        /// </summary>
        public string AddressLine1 { get; init; } = string.Empty;

        /// <summary>
        /// Gets the secondary address line.
        /// </summary>
        public string? AddressLine2 { get; init; }

        /// <summary>
        /// Gets the city.
        /// </summary>
        public string City { get; init; } = string.Empty;

        /// <summary>
        /// Gets the state or province.
        /// </summary>
        public string? State { get; init; }

        /// <summary>
        /// Gets the country code (2 letters).
        /// </summary>
        public string CountryCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets the postal or ZIP code.
        /// </summary>
        public string ZipCode { get; init; } = string.Empty;
    }
}