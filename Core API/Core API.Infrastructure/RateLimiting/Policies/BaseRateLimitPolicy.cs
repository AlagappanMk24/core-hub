using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Core_API.Infrastructure.RateLimiting.Policies
{
    /// <summary>
    /// Abstract base class providing common functionality for rate limiting policies.
    /// </summary>
    public abstract class BaseRateLimitPolicy : IRateLimitPolicy
    {
        #region Properties

        /// <summary>
        /// Gets the unique name of the rate limiting policy.
        /// </summary>
        public abstract string PolicyName { get; }

        /// <summary>
        /// Gets the message to display when rate limit is exceeded.
        /// </summary>
        public virtual string RateLimitExceededMessage =>
            "Too many requests. Please try again later.";

        /// <summary>
        /// Gets the number of seconds to recommend waiting before retrying.
        /// </summary>
        public virtual int RetryAfterSeconds => 60;

        /// <summary>
        /// Gets the maximum number of requests permitted in the time window.
        /// </summary>
        protected abstract int PermitLimit { get; }

        /// <summary>
        /// Gets the time window for the rate limit.
        /// </summary>
        protected abstract TimeSpan Window { get; }

        /// <summary>
        /// Gets the queue processing order.
        /// </summary>
        protected virtual QueueProcessingOrder QueueProcessingOrder => QueueProcessingOrder.OldestFirst;

        /// <summary>
        /// Gets the maximum number of requests that can be queued.
        /// </summary>
        protected virtual int QueueLimit => 0;

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the rate limiting options for this policy.
        /// </summary>
        /// <param name="options">The rate limiter options to configure.</param>
        /// <returns>The configured rate limiter options for method chaining.</returns>
        public RateLimiterOptions ConfigurePolicy(RateLimiterOptions options)
        {
            options.AddFixedWindowLimiter(PolicyName, opt =>
            {
                opt.PermitLimit = PermitLimit;
                opt.Window = Window;
                opt.QueueProcessingOrder = QueueProcessingOrder;
                opt.QueueLimit = QueueLimit;
            });

            return options;
        }

        #endregion
    }
}
