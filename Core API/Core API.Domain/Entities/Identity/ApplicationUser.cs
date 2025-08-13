using Core_API.Domain.Entities.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Identity
{
    public class ApplicationUser : AppIdentityUser
    {
        public string? FullName { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? CountryCode { get; set; }

        //Adding Foregin Key relation
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        [ValidateNever]
        public Company? Company { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        [ValidateNever]
        public Customer? Customer { get; set; }

        [NotMapped]
        public string Role { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsFirstLogin { get; set; } = true; // Default to true for new users
        public DateTime? LastLogin { get; set; }
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorExpiry { get; set; }
        public string? OtpIdentifier { get; set; } // Temporary identifier for OTP

        // Navigation property for the one-to-many relationship
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
