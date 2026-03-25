using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core_API.Domain.Entities
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
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [JsonIgnore]
        public List<Customer> Customers { get; set; } = [];

        [JsonIgnore]
        public List<Invoice> Invoices { get; set; } = [];

        [JsonIgnore]
        public List<RecurringInvoice> RecurringInvoices { get; set; } = [];
        public string CreatedByUserId { get; set; } = string.Empty;
    }
}