using System.Text.Json.Serialization;

namespace Core_API.Application.Contracts.DTOs.Response
{
    public class ResponseDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public bool IsSucceeded { get; set; }
        public object? Model { get; set; }

        [JsonIgnore]
        public ICollection<object>? Models { get; set; }
    }
}