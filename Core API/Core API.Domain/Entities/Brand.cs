using Core_API.Domain.Entities.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class Brand : BaseEntity
    {

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Slug { get; set; } // SEO-friendly URLs (e.g., nike, samsung)

        public string Description { get; set; }

        public string WebsiteUrl { get; set; }

        public string LogoUrl { get; set; } // image link to logo

        public string Country { get; set; }

        public int EstablishedYear { get; set; }

        public bool IsActive { get; set; } = true;

        [ValidateNever]
        public ICollection<Product> Products { get; set; }
    }
}
