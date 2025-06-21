using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Core_API.Domain.Entities
{
    public class Company : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
        public ICollection<Location>? Locations { get; set; }
        public List<Vendor> Vendors { get; set; }
    }
}
