using Core_API.Domain.Common;
using Core_API.Domain.Enums;

namespace Core_API.Domain.Entities.Customers
{
    /// <summary>
    /// Represents a document attached to a customer.
    /// </summary>
    public class CustomerDocument : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string DocumentName { get; set; }
        public string FileUrl { get; set; }
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public DocumentType Type { get; set; }
    }
}
