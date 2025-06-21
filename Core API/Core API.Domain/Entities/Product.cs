using Core_API.Domain.Entities.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; } // for preview listings

        // Pricing
        public double Price { get; set; }
        public double DiscountPrice { get; set; }
        public bool IsDiscounted { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        // Inventory
        public int StockQuantity { get; set; }
        public bool IsInStock => StockQuantity > 0;
        public bool AllowBackorder { get; set; }

        // internal code
        public string SKU { get; set; }
        public string Barcode { get; set; }

        // Categorization and Brand
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public int? VendorId { get; set; } // Added to associate product with a Vendor/Company

        // Physical Attributes
        public double WeightInKg { get; set; }
        public double WidthInCm { get; set; }
        public double HeightInCm { get; set; }
        public double LengthInCm { get; set; }

        // Flags & Status
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsTrending { get; set; }

        // SEO & Meta
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }

        // Analytics & Tracking
        public int Views { get; set; }
        public int SoldCount { get; set; }

        // Ratings & Reviews
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }


        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }

        [ForeignKey("SubCategoryId")]
        [ValidateNever]
        public SubCategory? SubCategory { get; set; }

        [ForeignKey("BrandId")]
        [ValidateNever]
        public Brand? Brand { get; set; }

        [ForeignKey("VendorId")]
        [ValidateNever]
        public Vendor? Vendor { get; set; }

        // Media 
        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; }

        [ValidateNever]
        public List<ProductSpecification> Specifications { get; set; }

        [ValidateNever]
        public List<ProductVariant> Variants { get; set; }

        [ValidateNever]
        public List<ProductTag> Tags { get; set; }
    }
}
