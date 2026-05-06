namespace Core_API.Infrastructure.RateLimiting.Policies
{
    /// <summary>
    /// Default rate limiting policy for general API endpoints.
    /// </summary>
    /// <remarks>
    /// This policy limits general API requests to 50 requests per minute per client.
    /// Exceeding the limit returns a 429 status code with a 60-second retry recommendation.
    /// This serves as a baseline protection for all API endpoints.
    /// </remarks>
    public class GeneralApiRateLimitPolicy : BaseRateLimitPolicy
    {
        #region Constants

        private const string GeneralApiPolicyName = "GeneralApiPolicy";
        private const int GeneralApiPermitLimit = 50;
        private const int GeneralApiWindowMinutes = 1;
        private const string GeneralApiRateLimitMessage = "Too many requests. Please wait 60 seconds before trying again.";
        private const int GeneralApiRetryAfterSeconds = 60;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique name of the rate limiting policy.
        /// </summary>
        public override string PolicyName => GeneralApiPolicyName;

        /// <summary>
        /// Gets the message to display when rate limit is exceeded.
        /// </summary>
        public override string RateLimitExceededMessage => GeneralApiRateLimitMessage;

        /// <summary>
        /// Gets the number of seconds to recommend waiting before retrying.
        /// </summary>
        public override int RetryAfterSeconds => GeneralApiRetryAfterSeconds;

        /// <summary>
        /// Gets the maximum number of requests permitted in the time window.
        /// </summary>
        protected override int PermitLimit => GeneralApiPermitLimit;

        /// <summary>
        /// Gets the time window for the rate limit (1 minute).
        /// </summary>
        protected override TimeSpan Window => TimeSpan.FromMinutes(GeneralApiWindowMinutes);

        #endregion
    }
}
