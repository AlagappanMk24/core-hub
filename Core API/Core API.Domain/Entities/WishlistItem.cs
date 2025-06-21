using Core_API.Domain.Entities.Common;
using Core_API.Domain.Entities.Identity;

namespace Core_API.Domain.Entities
{
    public class WishlistItem : BaseEntity
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public DateTime AddedDate { get; set; }

        // Navigation Properties
        public Product? Product { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
