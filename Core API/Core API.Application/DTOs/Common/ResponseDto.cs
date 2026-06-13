using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Core_API.Application.DTOs.Common
{
    public class ResponseDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public bool IsSucceeded { get; set; }
        public object? Data { get; set; }

        [JsonIgnore]
        public ICollection<object>? Models { get; set; }

        // Factory Methods
        public static ResponseDto Success(string message = "Operation completed successfully", object? data = null)
            => new()
            {
                StatusCode = StatusCodes.Status200OK,
                IsSucceeded = true,
                Message = message,
                Data = data
            };

        public static ResponseDto NotFound(string message = "Resource not found")
            => new()
            {
                StatusCode = StatusCodes.Status404NotFound,
                IsSucceeded = false,
                Message = message
            };

        public static ResponseDto Failure(string message, int statusCode = StatusCodes.Status400BadRequest, object? data = null)
            => new()
            {
                StatusCode = statusCode,
                IsSucceeded = false,
                Message = message,
                Data = data
            };
    }
}
