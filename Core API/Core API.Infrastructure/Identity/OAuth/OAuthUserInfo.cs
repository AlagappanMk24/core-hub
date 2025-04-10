namespace Core_API.Infrastructure.Identity.OAuth
{
    public class OAuthUserInfo
    {
        public string Provider { get; set; } = string.Empty; // e.g., "google", "microsoft", "facebook"
        public string ProviderKey { get; set; } = string.Empty; // Unique identifier from the provider (e.g., Google ID, Microsoft ID)
        public string Email { get; set; } = string.Empty; // User's email from the provider
        public string Name { get; set; } = string.Empty; // Full name from the provider
        public string ProfilePicture { get; set; } = string.Empty; // URL of the user's profile picture (optional)
    }
}
