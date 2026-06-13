using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core_API.Infrastructure.Services.Authentication;

/// <summary>
/// Enterprise-grade OTP attempt tracking and security service.
/// </summary>
public class OtpAttemptTracker(
    IOptions<OtpSettings> otpSettings,
    ILogger<OtpAttemptTracker> logger) : IOtpAttemptTracker
{
    private readonly OtpSettings _otpSettings = otpSettings.Value;
    private readonly ILogger<OtpAttemptTracker> _logger = logger;

    /// <summary>
    /// Registers a failed OTP attempt and applies progressive delays.
    /// </summary>
    public OtpAttemptResult RegisterFailedAttempt(
        ApplicationUser user,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new OtpAttemptResult { IsSuccessful = false };

        try
        {
            // Check if user is currently locked out
            if (user.IsOtpLocked)
            {
                var remainingSeconds = (int)(user.OtpLockoutEnd!.Value - DateTime.UtcNow).TotalSeconds;
                result.IsLocked = true;
                result.LockoutSecondsRemaining = Math.Max(1, remainingSeconds);

                result.LockoutEnd = user.OtpLockoutEnd; // Return the actual end time
                result.ErrorMessage = $"Too many failed attempts. Please try again later.";

                _logger.LogWarning("OTP attempt blocked: User {UserId} is locked out for {SecondsRemaining} seconds",
                    user.Id, result.LockoutSecondsRemaining);
                return result;
            }

            // Update attempt counters - No null issues now since FailedOtpAttempts is int (not int?)
            user.FailedOtpAttempts++;
            user.ConsecutiveFailedAttempts++;
            user.LastOtpAttemptAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(ipAddress))
                user.LastOtpAttemptIp = ipAddress;

            // Calculate remaining attempts
            int remainingAttempts = Math.Max(0, _otpSettings.MaxAttempts - user.FailedOtpAttempts);
            result.RemainingAttempts = remainingAttempts;

            // Check if max attempts exceeded
            if (user.FailedOtpAttempts >= _otpSettings.MaxAttempts)
            {
                // Lock the user out
                user.OtpLockoutEnd = DateTime.UtcNow.AddMinutes(_otpSettings.LockoutDurationMinutes);
                user.FailedOtpAttempts = 0; // Reset for next session after lockout

                result.IsLocked = true;
                result.LockoutSecondsRemaining = _otpSettings.LockoutDurationMinutes * 60;
                result.LockoutEnd = user.OtpLockoutEnd;
                result.ErrorMessage = $"Maximum attempts exceeded. Please try again later.";

                _logger.LogWarning("OTP lockout applied for user {UserId} for {LockoutMinutes} minutes",
                    user.Id, _otpSettings.LockoutDurationMinutes);
            }
            else
            {
                // Apply progressive delay if enabled
                if (_otpSettings.EnableProgressiveDelay && user.FailedOtpAttempts > 1)
                {
                    result.ProgressiveDelaySeconds = Math.Min(
                        (int)Math.Pow(2, user.FailedOtpAttempts - 1),
                        60
                    );
                }

                result.ErrorMessage = remainingAttempts > 0
                    ? $"Invalid verification code. You have {remainingAttempts} attempt(s) remaining."
                    : "Maximum attempts exceeded. Please request a new OTP.";
            }

            // Check consecutive failures threshold
            if (user.ConsecutiveFailedAttempts >= _otpSettings.ConsecutiveFailuresBeforeLockout * 2)
            {
                _logger.LogCritical("Multiple consecutive OTP failures detected for user {UserId}. Possible brute force attack.",
                    user.Id);
            }

            _logger.LogInformation("Failed OTP attempt recorded for user {UserId}. Attempts: {Attempts}/{MaxAttempts}, Consecutive: {Consecutive}",
                user.Id, user.FailedOtpAttempts, _otpSettings.MaxAttempts, user.ConsecutiveFailedAttempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering failed OTP attempt for user {UserId}", user.Id);
            throw;
        }

        return result;
    }

    /// <summary>
    /// Resets OTP attempt counters on successful validation.
    /// </summary>
    public void ResetAttempts(ApplicationUser user)
    {
        user.FailedOtpAttempts = 0;
        user.ConsecutiveFailedAttempts = 0;
        user.OtpLockoutEnd = null;
        user.LastOtpAttemptAt = null;

        _logger.LogInformation("OTP attempts reset for user {UserId} after successful validation", user.Id);
    }

    /// <summary>
    /// Checks if a user can request a new OTP based on cooldown period.
    /// </summary>
    public bool CanResendOtp(ApplicationUser user, out int secondsRemaining)
    {
        secondsRemaining = 0;

        if (!user.OtpLastSentAt.HasValue)
            return true;

        var timeSinceLastSend = DateTime.UtcNow - user.OtpLastSentAt.Value;

        // Convert TotalSeconds to int safely
        var elapsedSeconds = (int)Math.Floor(timeSinceLastSend.TotalSeconds);

        if (elapsedSeconds >= _otpSettings.ResendCooldownSeconds)
            return true;

        secondsRemaining = _otpSettings.ResendCooldownSeconds - elapsedSeconds;
        return false;
    }

    /// <summary>
    /// Updates the last OTP sent timestamp.
    /// </summary>
    public void RecordOtpSent(ApplicationUser user)
    {
        user.OtpLastSentAt = DateTime.UtcNow;
        _logger.LogDebug("OTP sent recorded for user {UserId}", user.Id);
    }

    /// <summary>
    /// Gets the current OTP status for a user.
    /// </summary>
    public OtpStatus GetOtpStatus(ApplicationUser user)
    {
        // CanResendOtp expects out parameter, we can discard it with '_'
        var canResend = CanResendOtp(user, out _);

        return new OtpStatus
        {
            IsLocked = user.IsOtpLocked,
            LockoutEnd = user.OtpLockoutEnd,
            FailedAttempts = user.FailedOtpAttempts,
            RemainingAttempts = Math.Max(0, _otpSettings.MaxAttempts - user.FailedOtpAttempts),
            ConsecutiveFailures = user.ConsecutiveFailedAttempts,
            OtpExpiry = user.TwoFactorExpiry,
            CanResend = canResend
        };
    }
}