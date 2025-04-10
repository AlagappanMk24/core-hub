namespace Core_API.Application.Contracts.DTOs.Request
{
    public class ExternalLoginDto
    {
        public string Provider { get; set; } = string.Empty; // e.g., "google", "microsoft", "facebook"
        public string AuthorizationCode { get; set; } = string.Empty; // The authorization code returned by the provider
    }
}
