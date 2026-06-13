using System.Text.Json.Serialization;

namespace Core_API.Application.DTOs.Common
{
    /// <summary>
    /// Represents a standardized structure for all API responses.
    /// </summary>
    /// <typeparam name="T">The type of the data being returned.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was processed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A descriptive message regarding the result of the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The HTTP status code associated with the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The payload containing the requested data.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        /// <summary>
        /// Detailed information about errors, if any occurred.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorDetails? ErrorDetails { get; set; }

        /// <summary>
        /// The UTC timestamp when the response was generated.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The unique correlation ID for tracing the request through logs.
        /// </summary>
        public string? TraceId { get; set; }

        #region Factory Methods

        /// <summary>
        /// Creates a successful API response.
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <param name="message">Success message.</param>
        /// <returns>A configured <see cref="ApiResponse{T}"/>.</returns>
        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = 200,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Creates an error response with specific error details.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code (Default 400).</param>
        /// <param name="errorDetails">Structured error information.</param>
        /// <returns>A configured <see cref="ApiResponse{T}"/>.</returns>
        public static ApiResponse<T> Error(string message, int statusCode = 400, ErrorDetails? errorDetails = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                ErrorDetails = errorDetails
            };
        }

        /// <summary>
        /// Creates a failure response, optionally including partial data.
        /// </summary>
        /// <param name="message">The failure message.</param>
        /// <param name="statusCode">The HTTP status code (Default 400).</param>
        /// <param name="data">Optional partial data.</param>
        /// <returns>A configured <see cref="ApiResponse{T}"/>.</returns>
        public static ApiResponse<T> Failure(string message, int statusCode = 400, T? data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        #endregion
    }

    /// <summary>
    /// Contains granular details about application or validation errors.
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// An internal application-specific error code (e.g., "ERR_INV_INV").
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable description of what went wrong.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A dictionary of validation errors where the key is the field name.
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        /// <summary>
        /// Additional troubleshooting metadata.
        /// </summary>
        public object? Metadata { get; set; }
    }
}