using Core_API.Domain.Common;

namespace Core_API.Domain.Entities.Customers
{
    /// <summary>
    /// Represents a contact person for a customer.
    /// </summary>
    public class CustomerContact : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Designation { get; set; }
        public bool IsPrimary { get; set; }
    }
}