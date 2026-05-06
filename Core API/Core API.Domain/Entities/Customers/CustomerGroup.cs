using Core_API.Domain.Common;
using Core_API.Domain.Entities.Companies;

namespace Core_API.Domain.Entities.Customers
{
    /// <summary>
    /// Represents a customer group for segmentation.
    /// </summary>
    public class CustomerGroup : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public List<Customer> Customers { get; set; } = new();
    }
}