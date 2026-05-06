using Core_API.Web.Middleware;

namespace Core_API.Web.Extensions
{
    /// <summary>
    /// Extension methods for registering the global exception middleware.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds the global exception handling middleware to the application pipeline.
        /// </summary>
        /// <param name="builder">The application builder to add middleware to.</param>
        /// <returns>The same application builder for method chaining.</returns>
        /// <remarks>
        /// This middleware should be added early in the pipeline, typically after
        /// rate limiting and before authentication.
        /// </remarks>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}