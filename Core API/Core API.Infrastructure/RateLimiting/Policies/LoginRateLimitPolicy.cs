namespace Core_API.Infrastructure.RateLimiting.Policies
{
    /// <summary>
    /// Rate limiting policy for login endpoints to prevent brute force attacks.
    /// </summary>
    /// <remarks>
    /// This policy limits login attempts to 5 requests per minute per client.
    /// Exceeding the limit returns a 429 status code with a 60-second retry recommendation.
    /// </remarks>
    public class LoginRateLimitPolicy : BaseRateLimitPolicy
    {
        #region Constants

        private const string LoginPolicyName = "LoginPolicy";
        private const int LoginPermitLimit = 5;
        private const int LoginWindowMinutes = 1;
        private const string LoginRateLimitMessage = "Too many login attempts. Please wait 60 seconds before trying again.";
        private const int LoginRetryAfterSeconds = 60;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique name of the rate limiting policy.
        /// </summary>
        public override string PolicyName => LoginPolicyName;

        /// <summary>
        /// Gets the message to display when rate limit is exceeded.
        /// </summary>
        public override string RateLimitExceededMessage => LoginRateLimitMessage;

        /// <summary>
        /// Gets the number of seconds to recommend waiting before retrying.
        /// </summary>
        public override int RetryAfterSeconds => LoginRetryAfterSeconds;

        /// <summary>
        /// Gets the maximum number of requests permitted in the time window.
        /// </summary>
        protected override int PermitLimit => LoginPermitLimit;

        /// <summary>
        /// Gets the time window for the rate limit (1 minute).
        /// </summary>
        protected override TimeSpan Window => TimeSpan.FromMinutes(LoginWindowMinutes);

        #endregion
    }
}