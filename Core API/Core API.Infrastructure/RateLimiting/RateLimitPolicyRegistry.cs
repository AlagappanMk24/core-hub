using Core_API.Infrastructure.RateLimiting.Policies;
using Microsoft.AspNetCore.RateLimiting;

namespace Core_API.Infrastructure.RateLimiting
{
    /// <summary>
    /// Registry for managing and configuring all rate limiting policies.
    /// </summary>
    /// <remarks>
    /// This class follows the Registry pattern to centralize policy registration
    /// and provide policy metadata lookup functionality.
    /// </remarks>
    public static class RateLimitPolicyRegistry
    {
        #region Private Fields

        private static readonly List<IRateLimitPolicy> _policies;
        private static readonly Dictionary<string, RateLimitPolicyMetadata> _policyMetadata;

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor that initializes all rate limiting policies.
        /// </summary>
        static RateLimitPolicyRegistry()
        {
            _policies =
            [
                new LoginRateLimitPolicy(),
                new OtpRateLimitPolicy(),
                new ResendOtpRateLimitPolicy(),
                new CompanyCreationRateLimitPolicy(),
                new GeneralApiRateLimitPolicy()
            ];

            _policyMetadata = _policies.ToDictionary(
                policy => policy.PolicyName,
                policy => new RateLimitPolicyMetadata(policy.RateLimitExceededMessage, policy.RetryAfterSeconds)
            );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets all registered rate limiting policies.
        /// </summary>
        public static IReadOnlyList<IRateLimitPolicy> Policies => _policies.AsReadOnly();

        /// <summary>
        /// Gets a dictionary mapping policy names to their metadata.
        /// </summary>
        public static IReadOnlyDictionary<string, RateLimitPolicyMetadata> PolicyMetadata => _policyMetadata;

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures all registered rate limiting policies on the provided options.
        /// </summary>
        /// <param name="options">The rate limiter options to configure.</param>
        /// <returns>The configured rate limiter options for method chaining.</returns>
        public static RateLimiterOptions ConfigureAllPolicies(RateLimiterOptions options)
        {
            foreach (var policy in _policies)
            {
                policy.ConfigurePolicy(options);
            }

            return options;
        }

        /// <summary>
        /// Retrieves metadata for a specific policy by name.
        /// </summary>
        /// <param name="policyName">The name of the policy.</param>
        /// <returns>The policy metadata, or null if not found.</returns>
        public static RateLimitPolicyMetadata? GetPolicyMetadata(string policyName)
        {
            return _policyMetadata.GetValueOrDefault(policyName);
        }

        /// <summary>
        /// Gets the default policy metadata for unknown policies.
        /// </summary>
        /// <returns>The default policy metadata.</returns>
        public static RateLimitPolicyMetadata GetDefaultPolicyMetadata()
        {
            return new RateLimitPolicyMetadata("Too many requests. Please try again later.", 60);
        }

        #endregion
    }
}