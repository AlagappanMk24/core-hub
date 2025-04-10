using System.Text.Json.Serialization;

namespace Core_API.Application.Contracts.DTOs.Response
{
    public class LoginResponseDto
    {
        public string? Token { get; set; } = string.Empty;
        public string UserName { get; set; }
        public string? Message { get; set; }
        public string Email { get; set; }
        public bool IsAuthenticated { get; set; }
        [JsonIgnore]
        public string? Role { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }
        [JsonIgnore]
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
