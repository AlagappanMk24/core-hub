using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core_API.Domain.Entities;
public class Customer : BaseEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public Address Address { get; set; }

    [ForeignKey("Company")]
    public int? CompanyId { get; set; }
    public Company Company { get; set; }

    [JsonIgnore]
    public List<Invoice> Invoices { get; set; } = new List<Invoice>();
}
public class Address
{
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string ZipCode { get; set; }
}