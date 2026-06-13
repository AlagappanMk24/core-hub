using System.ComponentModel.DataAnnotations;

namespace Core_API.Infrastructure.Configuration.Settings
{
    /// <summary>
    /// OTP (One-Time Password) security configuration settings.
    /// </summary>
    public class OtpSettings
    {
        [Range(1, 10, ErrorMessage = "Maximum attempts must be between 1 and 10")]
        public int MaxAttempts { get; set; } = 5;

        [Range(1, 60, ErrorMessage = "Lockout duration must be between 1 and 60 minutes")]
        public int LockoutDurationMinutes { get; set; } = 15;

        [Range(30, 300, ErrorMessage = "OTP validity must be between 30 and 300 seconds")]
        public int OtpValiditySeconds { get; set; } = 120;

        [Range(30, 600, ErrorMessage = "Resend cooldown must be between 30 and 600 seconds")]
        public int ResendCooldownSeconds { get; set; } = 60;

        [Range(1, 10, ErrorMessage = "Consecutive failures before lockout must be between 1 and 10")]
        public int ConsecutiveFailuresBeforeLockout { get; set; } = 3;
        public bool EnableProgressiveDelay { get; set; } = true;
        public bool LogAllAttempts { get; set; } = true;
        public bool EnableIpTracking { get; set; } = true;
    }
}