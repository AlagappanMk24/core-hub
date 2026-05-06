using Core_API.Domain.Common;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Entities.RecurringInvoices;
using Core_API.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core_API.Domain.Entities.Companies
{
    public class Company : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? TaxId { get; set; }
        public Address? Address { get; set; }

        [StringLength(100)]
        public Email? Email { get; set; }

        [StringLength(20)]
        public PhoneNumber? PhoneNumber { get; set; }

        // Add base currency for the company
        [StringLength(3)]
        public string BaseCurrency { get; set; } = "USD"; // Default to USD

        [JsonIgnore]
        public List<Customer> Customers { get; set; } = [];

        /// <summary>
        /// Gets or sets the customer groups for this company.
        /// </summary>
        [JsonIgnore]
        public List<CustomerGroup> CustomerGroups { get; set; } = new();

        [JsonIgnore]
        public List<Invoice> Invoices { get; set; } = [];

        [JsonIgnore]
        public List<RecurringInvoice> RecurringInvoices { get; set; } = [];
        public string CreatedByUserId { get; set; } = string.Empty;
    }
}