using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
namespace Core_API.Domain.Entities
{
    public class Location : BaseEntity
    {
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; } // Like "Mumbai Office" or "New York Store"
        public Address Address { get; set; } // Branch address

        [ForeignKey("Currency")]
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; } // Navigation property

        [ForeignKey("Timezone")]
        public int TimezoneId { get; set; }
        public Timezone Timezone { get; set; } // Navigation property
        public ICollection<Invoice> Invoices { get; set; }

    }
    public class Currency : BaseEntity
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
    }
    public class Timezone : BaseEntity
    {
        public string Name { get; set; }
        public string UtcOffset { get; set; }
        public string UtcOffsetString { get; set; }
        public string Abbreviation { get; set; }
    }
}
