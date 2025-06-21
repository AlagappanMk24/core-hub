using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class Vendor : BaseEntity
    {
        [MaxLength(100)]
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string? VendorName { get; set; }

        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        [Display(Name = "Profile Picture")]
        public string? VendorPictureUrl { get; set; }
        public ICollection<Product>? Products { get; set; }      // Navigation property that contains multiple Products
        public int CompanyId { get; set; } // Foreign key
        public Company Company { get; set; } // Navigation property
    }
}
