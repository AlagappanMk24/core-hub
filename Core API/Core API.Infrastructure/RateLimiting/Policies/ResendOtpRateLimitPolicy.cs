namespace Core_API.Infrastructure.RateLimiting.Policies
{
    /// <summary>
    /// Rate limiting policy for OTP (One-Time Password) resend endpoints.
    /// </summary>
    /// <remarks>
    /// This policy limits OTP resend requests to 3 requests per 5 minutes per client.
    /// Exceeding the limit returns a 429 status code with a 300-second (5-minute) retry recommendation.
    /// </remarks>
    public class ResendOtpRateLimitPolicy : BaseRateLimitPolicy
    {
        #region Constants

        private const string ResendOtpPolicyName = "ResendOtpPolicy";
        private const int ResendOtpPermitLimit = 3;
        private const int ResendOtpWindowMinutes = 5;
        private const string ResendOtpRateLimitMessage = "Too many OTP resend requests. Please wait 5 minutes before trying again.";
        private const int ResendOtpRetryAfterSeconds = 300;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique name of the rate limiting policy.
        /// </summary>
        public override string PolicyName => ResendOtpPolicyName;

        /// <summary>
        /// Gets the message to display when rate limit is exceeded.
        /// </summary>
        public override string RateLimitExceededMessage => ResendOtpRateLimitMessage;

        /// <summary>
        /// Gets the number of seconds to recommend waiting before retrying.
        /// </summary>
        public override int RetryAfterSeconds => ResendOtpRetryAfterSeconds;

        /// <summary>
        /// Gets the maximum number of requests permitted in the time window.
        /// </summary>
        protected override int PermitLimit => ResendOtpPermitLimit;

        /// <summary>
        /// Gets the time window for the rate limit (5 minutes).
        /// </summary>
        protected override TimeSpan Window => TimeSpan.FromMinutes(ResendOtpWindowMinutes);

        #endregion
    }
}
