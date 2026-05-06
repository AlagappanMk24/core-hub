namespace Core_API.Infrastructure.RateLimiting.Policies
{
    /// <summary>
    /// Rate limiting policy for company creation endpoints to prevent spam.
    /// </summary>
    /// <remarks>
    /// This policy limits company registration requests to 5 requests per hour per client.
    /// Exceeding the limit returns a 429 status code with a 3600-second (1-hour) retry recommendation.
    /// </remarks>
    public class CompanyCreationRateLimitPolicy : BaseRateLimitPolicy
    {
        #region Constants

        private const string CompanyCreationPolicyName = "CompanyCreationPolicy";
        private const int CompanyCreationPermitLimit = 5;
        private const int CompanyCreationWindowHours = 1;
        private const string CompanyCreationRateLimitMessage = "Too many company registration attempts. Please wait 1 hour before trying again.";
        private const int CompanyCreationRetryAfterSeconds = 3600;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique name of the rate limiting policy.
        /// </summary>
        public override string PolicyName => CompanyCreationPolicyName;

        /// <summary>
        /// Gets the message to display when rate limit is exceeded.
        /// </summary>
        public override string RateLimitExceededMessage => CompanyCreationRateLimitMessage;

        /// <summary>
        /// Gets the number of seconds to recommend waiting before retrying.
        /// </summary>
        public override int RetryAfterSeconds => CompanyCreationRetryAfterSeconds;

        /// <summary>
        /// Gets the maximum number of requests permitted in the time window.
        /// </summary>
        protected override int PermitLimit => CompanyCreationPermitLimit;

        /// <summary>
        /// Gets the time window for the rate limit (1 hour).
        /// </summary>
        protected override TimeSpan Window => TimeSpan.FromHours(CompanyCreationWindowHours);

        #endregion
    }
}