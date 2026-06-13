using System.ComponentModel.DataAnnotations;

namespace Core_API.Infrastructure.Configuration.Settings
{
    public class JwtSettings
    {
        [Required(ErrorMessage = "SecretKey is required")]
        [MinLength(32, ErrorMessage = "SecretKey must be at least 32 characters")]
        public string SecretKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "ValidIssuer is required")]
        public string ValidIssuer { get; set; } = string.Empty;

        [Required(ErrorMessage = "ValidAudience is required")]
        public string ValidAudience { get; set; } = string.Empty;

        [Range(1, 72, ErrorMessage = "ExpireHours must be between 1 and 72")]
        public int ExpireHours { get; set; } = 12;

        [Range(1, 30, ErrorMessage = "OtpExpireMinutes must be between 1 and 30")]
        public int OtpExpireMinutes { get; set; } = 10;

        [Range(1, 168, ErrorMessage = "RefreshTokenExpireHours must be between 1 and 168")]
        public int RefreshTokenExpireHours { get; set; } = 24;

        [Range(0, 5, ErrorMessage = "ClockSkewMinutes must be between 0 and 5")]
        public int ClockSkewMinutes { get; set; } = 2;
    }
}