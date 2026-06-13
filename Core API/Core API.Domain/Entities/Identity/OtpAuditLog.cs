using Core_API.Domain.Common;

namespace Core_API.Domain.Entities.Identity
{
    /// <summary>
    /// Audit log for OTP-related activities.
    /// </summary>
    public class OtpAuditLog : BaseEntity
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? OtpIdentifier { get; set; }
        public string Action { get; set; } = string.Empty; // "SENT", "VERIFIED", "FAILED", "RESENT"
        public string? OtpCode { get; set; } // Hashed or masked
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public int RemainingAttempts { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}