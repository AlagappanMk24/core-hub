using Core_API.Domain.Entities.Common;
using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class CompanyRequest : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CompanyName { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public CompanyRequestStatus Status { get; set; } = CompanyRequestStatus.Pending;

        public DateTime? ProcessedAt { get; set; }
        public string ProcessedBy { get; set; } = string.Empty; // Default empty string

        public string RejectionReason { get; set; } = string.Empty; // Default empty string

        public string RequestToken { get; set; } = string.Empty; // For Secure Email Links
    }
}
