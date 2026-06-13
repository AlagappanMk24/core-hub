namespace Core_API.Application.DTOs.Authentication.Responses
{
    /// <summary>
    /// OTP validation error details
    /// </summary>
    public class OtpErrorDetails
    {
        public int RemainingAttempts { get; set; }
        public int? ProgressiveDelaySeconds { get; set; }
        public bool IsLocked { get; set; }
        public int? LockoutSecondsRemaining { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string ErrorType { get; set; } = string.Empty; // "InvalidOtp", "Expired", "Locked", "MaxAttempts"
    }
}