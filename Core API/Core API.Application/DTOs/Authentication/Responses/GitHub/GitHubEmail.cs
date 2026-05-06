using System.Text.Json.Serialization;

namespace Core_API.Application.DTOs.Authentication.Responses.GitHub
{
    public class GitHubEmail
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
    }
}
