using System.Net;
using System.Text.Json;

namespace Core_API.Web.Middleware
{
    /// <summary>
    /// Middleware that catches unhandled exceptions globally and returns standardized error responses.
    /// </summary>
    /// <remarks>
    /// This middleware should be registered early in the pipeline to catch exceptions
    /// from all subsequent middleware components and controllers.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
    /// </remarks>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for recording exception details.</param>
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        #region Private Fields

        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<GlobalExceptionMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes the HTTP request and catches any unhandled exceptions.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// When an exception occurs, this method:
        /// <list type="number">
        /// <item><description>Logs the exception with stack trace</description></item>
        /// <item><description>Returns a standardized JSON error response</description></item>
        /// <item><description>Sets appropriate HTTP status code (500 Internal Server Error)</description></item>
        /// </list>
        /// </remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the exception by creating a standardized error response.
        /// </summary>
        /// <param name="context">The HTTP context to write the response to.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                success = false,
                statusCode = 500,
                message = "An error occurred while processing your request.",
                detail = exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
        #endregion
    }
}