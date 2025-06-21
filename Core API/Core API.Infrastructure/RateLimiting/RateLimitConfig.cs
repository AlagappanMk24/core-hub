using Microsoft.Extensions.Configuration;

namespace Core_API.Infrastructure.RateLimiting
{
    public static class RateLimitConfig
    {
        public record RateLimitPolicyMetadata(string Message, int RetryAfterSeconds);
        public static Dictionary<string, RateLimitPolicyMetadata> Policies { get; private set; }
        public static void Initialize(IConfiguration configuration)
        {
            var section = configuration.GetSection("RateLimitPolicies");
            Policies = section.Get<Dictionary<string, RateLimitPolicyMetadata>>() ?? new Dictionary<string, RateLimitPolicyMetadata>
            {
                { "LoginPolicy", new RateLimitPolicyMetadata("Too many login attempts. Please wait and try again.", 60) },
                { "OtpPolicy", new RateLimitPolicyMetadata("Too many OTP verification attempts. Please wait and try again.", 60) },
                { "ResendOtpPolicy", new RateLimitPolicyMetadata("Too many OTP resend requests. Please wait and try again.", 300) }
            };
        }
    }
}