using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core_API.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        // Navigation property for orders
        [JsonIgnore]
        public List<OrderHeader>? Orders { get; set; }

        // Navigation property for invoices
        [JsonIgnore]
        public List<Invoice>? Invoices { get; set; }
    }
}
