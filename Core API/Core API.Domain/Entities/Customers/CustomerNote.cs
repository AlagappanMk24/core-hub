using Core_API.Domain.Common;
using Core_API.Domain.Enums;

namespace Core_API.Domain.Entities.Customers
{
    /// <summary>
    /// Represents a note attached to a customer.
    /// </summary>
    public class CustomerNote : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string Note { get; set; }
        public NoteType Type { get; set; } = NoteType.Internal;
        public bool IsPinned { get; set; }
    }
}