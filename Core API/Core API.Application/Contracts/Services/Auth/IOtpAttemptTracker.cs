using Core_API.Application.Common.Models;
using Core_API.Domain.Entities.Identity;

namespace Core_API.Application.Contracts.Services.Auth;

/// <summary>
/// Interface for OTP attempt tracking and security service.
/// </summary>
public interface IOtpAttemptTracker
{
    /// <summary>
    /// Registers a failed OTP attempt and applies progressive delays.
    /// </summary>
    OtpAttemptResult RegisterFailedAttempt(
        ApplicationUser user,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets OTP attempt counters on successful validation.
    /// </summary>
    void ResetAttempts(ApplicationUser user);

    /// <summary>
    /// Checks if a user can request a new OTP based on cooldown period.
    /// </summary>
    bool CanResendOtp(ApplicationUser user, out int secondsRemaining);

    /// <summary>
    /// Updates the last OTP sent timestamp.
    /// </summary>
    void RecordOtpSent(ApplicationUser user);

    /// <summary>
    /// Gets the current OTP status for a user.
    /// </summary>
    OtpStatus GetOtpStatus(ApplicationUser user);
}