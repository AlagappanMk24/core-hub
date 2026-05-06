using Core_API.Domain.Common;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Entities.RecurringInvoices;
using Core_API.Domain.Enums;
using Core_API.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core_API.Domain.Entities.Customers
{
    /// <summary>
    /// Represents a customer in the system.
    /// </summary>
    public class Customer : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer's full name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        public Email Email { get; set; }

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        public PhoneNumber PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer's address.
        /// </summary>
        public Address Address { get; set; }


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
        /// Gets or sets the payment terms (e.g., "Net 30").
        /// </summary>
        public string? DefaultPaymentTerms { get; set; }

        /// <summary>
        /// Gets or sets the default currency for this customer.
        /// </summary>
        public string? DefaultCurrency { get; set; }

        /// <summary>
        /// Gets or sets the customer group ID.
        /// </summary>
        public int? CustomerGroupId { get; set; }

        /// <summary>
        /// Gets or sets the customer group.
        /// </summary>
        public CustomerGroup? CustomerGroup { get; set; }

        /// <summary>
        /// Gets or sets the customer's status (Active, Inactive, Suspended).
        /// </summary>
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

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
        /// Gets or sets the company ID this customer belongs to.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        public Company Company { get; set; }

        /// <summary>
        /// Gets or sets the customer's invoices.
        /// </summary>
        [JsonIgnore]
        public List<Invoice> Invoices { get; set; } = new();

        /// <summary>
        /// Gets or sets the customer's recurring invoices.
        /// </summary>
        [JsonIgnore]
        public List<RecurringInvoice> RecurringInvoices { get; set; } = new();

        /// <summary>
        /// Gets or sets the customer's notes.
        /// </summary>
        [JsonIgnore]
        public List<CustomerNote> Notes { get; set; } = new();

        /// <summary>
        /// Gets or sets the customer's documents.
        /// </summary>
        [JsonIgnore]
        public List<CustomerDocument> Documents { get; set; } = new();

        /// <summary>
        /// Gets or sets the customer's contact persons.
        /// </summary>
        [JsonIgnore]
        public List<CustomerContact> Contacts { get; set; } = new();
    }
}