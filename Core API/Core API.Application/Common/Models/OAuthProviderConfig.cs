namespace Core_API.Application.Common.Models
{
    public class OAuthProviderConfig
    {
        public string Provider { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string? TenantId { get; set; }
        public string TokenUrl { get; set; } = string.Empty;
        public string UserInfoUrl { get; set; } = string.Empty;
    }
}
