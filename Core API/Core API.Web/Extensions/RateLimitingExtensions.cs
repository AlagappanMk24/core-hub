using Core_API.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Core_API.Web.Extensions;

/// <summary>
/// Provides extension methods for configuring rate limiting policies to protect API endpoints from abuse.
/// </summary>
/// <remarks>
/// Rate limiting helps prevent DoS attacks, brute force attempts, and ensures fair usage of API resources.
/// Each policy defines the maximum number of requests allowed within a specific time window.
/// </remarks>
public static class RateLimitingExtensions
{
    #region Public Methods

    /// <summary>
    /// Configures rate limiting policies for the application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration containing rate limit settings.</param>
    /// <returns>The same service collection so multiple calls can be chained.</returns>
    /// <remarks>
    /// The following rate limiting policies are configured:
    /// <list type="table">
    /// <listheader>
    /// <term>Policy Name</term>
    /// <description>Configuration</description>
    /// </listheader>
    /// <item>
    /// <term>LoginPolicy</term>
    /// <description>5 requests per minute - Protects login endpoints from brute force attacks</description>
    /// </item>
    /// <item>
    /// <term>OtpPolicy</term>
    /// <description>10 requests per minute - Limits OTP validation attempts</description>
    /// </item>
    /// <item>
    /// <term>ResendOtpPolicy</term>
    /// <description>3 requests per 5 minutes - Prevents OTP spam</description>
    /// </item>
    /// <item>
    /// <term>CompanyCreationPolicy</term>
    /// <description>5 requests per hour - Limits company registration attempts</description>
    /// </item>
    /// <item>
    /// <term>GeneralApiPolicy</term>
    /// <description>50 requests per minute - Default policy for general API endpoints</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            #region Rate Limit Rejection Handler

            options.OnRejected = HandleRateLimitRejection;

            #endregion

            #region Register All Policies

            RateLimitPolicyRegistry.ConfigureAllPolicies(options);

            #endregion

            options.RejectionStatusCode = 429;
        });

        return services;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles rate limit exceeded scenarios with custom responses and logging.
    /// </summary>
    /// <param name="context">The rate limiting context.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async ValueTask HandleRateLimitRejection(
       OnRejectedContext context,
       CancellationToken cancellationToken)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        context.HttpContext.Response.StatusCode = 429;

        // Determine the policy name from endpoint metadata
        var policyName = GetPolicyNameFromEndpoint(context.HttpContext);

        // Retrieve policy metadata
        var policyMetadata = RateLimitPolicyRegistry.GetPolicyMetadata(policyName)
            ?? RateLimitPolicyRegistry.GetDefaultPolicyMetadata();

        var retryAfterSeconds = policyMetadata.RetryAfterSeconds;
        context.HttpContext.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());

        // Log rate limit violation for monitoring
        logger.LogWarning(
            "Rate limit exceeded for policy {PolicyName} on endpoint {Endpoint}. Message: {Message}, Retry-After: {RetryAfterSeconds}s",
            policyName,
            context.HttpContext.GetEndpoint()?.DisplayName,
            policyMetadata.Message,
            retryAfterSeconds);

        // Return JSON response with error details
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            statusCode = 429,
            message = policyMetadata.Message,
            retryAfterSeconds
        }, cancellationToken);
    }

    /// <summary>
    /// Extracts the rate limiting policy name from the HTTP context endpoint metadata.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the endpoint.</param>
    /// <returns>The policy name, or "Unknown" if not found.</returns>
    private static string GetPolicyNameFromEndpoint(HttpContext httpContext)
    {
        const string unknownPolicyName = "Unknown";

        var endpoint = httpContext.GetEndpoint();
        var rateLimitAttribute = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();

        return rateLimitAttribute?.PolicyName ?? unknownPolicyName;
    }

    #endregion
}