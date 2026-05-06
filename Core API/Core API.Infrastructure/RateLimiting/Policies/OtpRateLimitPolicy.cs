namespace Core_API.Infrastructure.RateLimiting.Policies
{
    /// <summary>
    /// Rate limiting policy for OTP (One-Time Password) validation endpoints.
    /// </summary>
    /// <remarks>
    /// This policy limits OTP verification attempts to 10 requests per minute per client.
    /// Exceeding the limit returns a 429 status code with a 60-second retry recommendation.
    /// </remarks>
    public class OtpRateLimitPolicy : BaseRateLimitPolicy
    {
        #region Constants

        private const string OtpPolicyName = "OtpPolicy";
        private const int OtpPermitLimit = 10;
        private const int OtpWindowMinutes = 1;
        private const string OtpRateLimitMessage = "Too many OTP verification attempts. Please wait 60 seconds before trying again.";
        private const int OtpRetryAfterSeconds = 60;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique name of the rate limiting policy.
        /// </summary>
        public override string PolicyName => OtpPolicyName;

        /// <summary>
        /// Gets the message to display when rate limit is exceeded.
        /// </summary>
        public override string RateLimitExceededMessage => OtpRateLimitMessage;

        /// <summary>
        /// Gets the number of seconds to recommend waiting before retrying.
        /// </summary>
        public override int RetryAfterSeconds => OtpRetryAfterSeconds;

        /// <summary>
        /// Gets the maximum number of requests permitted in the time window.
        /// </summary>
        protected override int PermitLimit => OtpPermitLimit;

        /// <summary>
        /// Gets the time window for the rate limit (1 minute).
        /// </summary>
        protected override TimeSpan Window => TimeSpan.FromMinutes(OtpWindowMinutes);

        #endregion
    }
}
