using Core_API.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; } // Primary key for the refresh token
        public string Token { get; set; } = string.Empty; // The actual refresh token string
        public DateTime ExpiresOn { get; set; } // When the refresh token expires
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow; // When the refresh token was created
        public bool IsRevoked { get; set; } // To mark a token as revoked (e.g., on logout)
        public string? RevokedByIp { get; set; } // Optional: IP address from which it was revoked
        public DateTime? RevokedOn { get; set; } // Optional: When it was revoked

        // Foreign key to ApplicationUser
        public string ApplicationUserId { get; set; } 

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
