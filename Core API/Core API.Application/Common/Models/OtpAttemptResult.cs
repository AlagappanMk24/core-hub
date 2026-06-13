namespace Core_API.Application.Common.Models;

/// <summary>
/// Represents the result of an OTP validation attempt.
/// </summary>
public class OtpAttemptResult
{
    public bool IsSuccessful { get; set; }
    public bool IsLocked { get; set; }
    public int RemainingAttempts { get; set; }
    public int? LockoutSecondsRemaining { get; set; } 
    public DateTime? LockoutEnd { get; set; } // Added for precise lockout time
    public string? ErrorMessage { get; set; }
    public int? ProgressiveDelaySeconds { get; set; }
    public string ErrorType { get; set; } = string.Empty; // "InvalidOtp", "Expired", "Locked", "MaxAttempts"
}

/// <summary>
/// Represents the current OTP status for a user.
/// </summary>
public class OtpStatus
{
    public bool IsLocked { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int FailedAttempts { get; set; }
    public int RemainingAttempts { get; set; }
    public int ConsecutiveFailures { get; set; }
    public DateTime? OtpExpiry { get; set; }
    public bool CanResend { get; set; }
    public int? LockoutSecondsRemaining => IsLocked && LockoutEnd.HasValue
     ? (int?)Math.Max(0, (LockoutEnd.Value - DateTime.UtcNow).TotalSeconds)
     : null;
}