using Core_API.Domain.Entities.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public string VariantName { get; set; } // e.g., "Size - M", "Color - Red"

        public string SKU { get; set; }

        public double Price { get; set; }

        public double? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }

        public bool IsAvailable => StockQuantity > 0;
    }

}
