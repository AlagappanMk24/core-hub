using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
namespace Core_API.Domain.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        [JsonIgnore]
        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; } = new List<RefreshToken>();

        // Add Two-Factor Authentication Fields
        public string? TwoFactorCode { get; set; } // Store OTP
        public DateTime? TwoFactorExpiry { get; set; } // Store OTP Expiration Time
    }
}