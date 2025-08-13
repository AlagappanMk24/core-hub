using System.Text.Json.Serialization;

namespace Core_API.Application.DTOs.Authentication.Response.MicrosoftResponse
{
    public class MicrosoftUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("mail")]
        public string Mail { get; set; } // Primary email (can be null)

        [JsonPropertyName("otherMails")]
        public List<string> OtherMails { get; set; } // Alternative emails

        [JsonPropertyName("userPrincipalName")]
        public string UserPrincipalName { get; set; } // Use only if mail is null

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }
}
