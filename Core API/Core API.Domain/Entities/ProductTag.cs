using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities
{
    public class ProductTag : BaseEntity
    {
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public string TagName { get; set; } // e.g., "New", "Eco-Friendly", "Limited Edition"
    }
}
