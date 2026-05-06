using Core_API.Application.Contracts.Services.Auth;
using Microsoft.Extensions.Configuration;

namespace Core_API.Infrastructure.Services.Authentication
{
    public class ExternalAuthUrlBuilder : IExternalAuthUrlBuilder
    {
        private readonly IConfiguration _configuration;

        public ExternalAuthUrlBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> BuildAuthorizationUrlAsync(string provider)
        {
            string providerLower = provider.ToLowerInvariant();

            return providerLower switch
            {
                "google" => Task.FromResult(BuildGoogleUrl()),
                "microsoft" => Task.FromResult(BuildMicrosoftUrl()),
                "facebook" => Task.FromResult(BuildFacebookUrl()),
                "github" => Task.FromResult(BuildGitHubUrl()),

                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };
        }

        private string BuildGoogleUrl()
        {
            var clientId = _configuration["GoogleKeys:ClientId"]
                ?? throw new InvalidOperationException("Google ClientId not configured");

            var redirectUri = _configuration["OAuth:RedirectUri"] ?? "http://localhost:4200/auth/callback";

            return $"https://accounts.google.com/o/oauth2/auth?" +
                   $"client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&response_type=code" +
                   $"&scope=email%20profile" +
                   $"&state=google";
        }

        private string BuildMicrosoftUrl()
        {
            var clientId = _configuration["MicrosoftKeys:ClientId"]
                ?? throw new InvalidOperationException("Microsoft ClientId not configured");

            var tenantId = _configuration["MicrosoftKeys:TenantId"]
                ?? throw new InvalidOperationException("Microsoft TenantId not configured");

            var redirectUri = _configuration["OAuth:RedirectUri"] ?? "http://localhost:4200/auth/callback";

            return $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                   $"client_id={Uri.EscapeDataString(clientId)}" +
                   $"&response_type=code" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&scope=openid%20email%20profile%20User.Read" +
                   $"&state=microsoft";
        }

        private string BuildFacebookUrl()
        {
            var clientId = _configuration["FacebookKeys:ClientId"]
                ?? throw new InvalidOperationException("Facebook ClientId not configured");

            var redirectUri = _configuration["OAuth:RedirectUri"] ?? "http://localhost:4200/auth/callback";

            return $"https://www.facebook.com/v13.0/dialog/oauth?" +
                   $"client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&scope=email%20public_profile";
        }

        private string BuildGitHubUrl()
        {
            var clientId = _configuration["GitHubKeys:ClientId"]
                ?? throw new InvalidOperationException("GitHub ClientId not configured");

            var redirectUri = _configuration["OAuth:RedirectUri"] ?? "http://localhost:4200/auth/callback";

            return $"https://github.com/login/oauth/authorize?" +
                   $"client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&scope=user:email" +
                   $"&state=github";
        }
    }
}