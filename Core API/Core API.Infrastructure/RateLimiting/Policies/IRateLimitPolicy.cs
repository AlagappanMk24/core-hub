using Microsoft.AspNetCore.RateLimiting;

namespace Core_API.Infrastructure.RateLimiting.Policies
{
    /// <summary>
    /// Defines the contract for rate limiting policy implementations.
    /// </summary>
    /// <remarks>
    /// Each policy implementation should define:
    /// <list type="bullet">
    /// <item><description>A unique policy name</description></item>
    /// <item><description>Rate limiting parameters (permit limit, window, queue options)</description></item>
    /// <item><description>Custom rejection response message and retry period</description></item>
    /// </list>
    /// </remarks>
    public interface IRateLimitPolicy
    {
        #region Properties

        /// <summary>
        /// Gets the unique name of the rate limiting policy.
        /// </summary>
        /// <remarks>
        /// This name is used to reference the policy when applying [EnableRateLimiting] attributes.
        /// Example: "LoginPolicy", "OtpPolicy", etc.
        /// </remarks>
        string PolicyName { get; }

        /// <summary>
        /// Gets the message to display when rate limit is exceeded.
        /// </summary>
        string RateLimitExceededMessage { get; }

        /// <summary>
        /// Gets the number of seconds to recommend waiting before retrying.
        /// </summary>
        int RetryAfterSeconds { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Configures the rate limiting options for this policy.
        /// </summary>
        /// <param name="options">The rate limiter options to configure.</param>
        /// <returns>The configured rate limiter options for method chaining.</returns>
        RateLimiterOptions ConfigurePolicy(RateLimiterOptions options);

        #endregion
    }
}
