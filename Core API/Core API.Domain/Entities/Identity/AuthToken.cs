using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Identity
{
    public class AuthToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [MaxLength(500)] // Adjust length based on your token type (JWT vs Opaque)
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        [MaxLength(45)] // Maximum length for an IPv6 address
        public string? RevokedByIp { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        public string? CreatedByIp { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}