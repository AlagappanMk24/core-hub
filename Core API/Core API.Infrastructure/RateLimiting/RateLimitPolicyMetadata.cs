namespace Core_API.Infrastructure.RateLimiting;

/// <summary>
/// Represents metadata information for a rate limiting policy.
/// </summary>
/// <param name="Message">The message to display when rate limit is exceeded.</param>
/// <param name="RetryAfterSeconds">The number of seconds to wait before retrying.</param>
public record RateLimitPolicyMetadata(string Message, int RetryAfterSeconds);