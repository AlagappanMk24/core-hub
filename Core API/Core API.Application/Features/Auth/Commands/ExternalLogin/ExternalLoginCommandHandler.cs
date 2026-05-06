using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Account;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.DTOs.Authentication.Responses;
using Core_API.Application.DTOs.Authentication.Responses.GitHub;
using Core_API.Application.DTOs.Authentication.Responses.Microsoft;
using Core_API.Domain.Entities.Identity;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Core_API.Application.Features.Auth.Commands.ExternalLogin
{
    /// <summary>
    /// Handles external login (OAuth2) requests by exchanging authorization code for JWT token.
    /// Supports Google, Microsoft, Facebook, and GitHub providers.
    /// </summary>
    public class ExternalLoginCommandHandler(
        IUnitOfWork unitOfWork,
        HttpClient httpClient,
        IConfiguration configuration,
        IJwtService jwtService,
        IAccountService accountService,
        ILogger<ExternalLoginCommandHandler> logger) : IRequestHandler<ExternalLoginCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IAccountService _accountService = accountService;
        private readonly ILogger<ExternalLoginCommandHandler> _logger = logger;

        #region IRequestHandler Implementation

        /// <summary>
        /// Exchanges an OAuth2 authorization code for a JWT token.
        /// </summary>
        /// <param name="model">The external login details, including authorization code and provider.</param>
        /// <returns>A JWT token for the authenticated user.</returns>
        /// <exception cref="ArgumentException">Thrown if the provider is unsupported.</exception>
        public async Task<string> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Exchanging authorization code for access token (Provider: {Provider})", request.Provider);

            // Step 1: Get provider-specific configuration
            var config = GetOAuthProviderConfiguration(request.Provider);

            // Step 2: Prepare and send token exchange request
            var tokenResponse = await ExchangeCodeForTokenAsync(request.AuthorizationCode, config);

            // Step 3: Fetch user information from provider
            var userInfo = await GetUserInfoFromProviderAsync(tokenResponse.AccessToken, request.Provider, config.UserInfoUrl);

            // Step 4: Get or create user in our system
            var user = await GetOrCreateUserAsync(userInfo);

            _logger.LogInformation("Successfully exchanged authorization code for access token (Provider: {Provider})", request.Provider);

            return await _jwtService.GenerateJwtToken(user);
        }
        #endregion

        #region OAuth Configuration

        /// <summary>
        /// Retrieves OAuth2 configuration (endpoints, credentials) for the specified provider.
        /// </summary>
        /// <param name="provider">The OAuth provider name (google, microsoft, facebook, github).</param>
        /// <returns>Configuration object for the provider.</returns>
        /// <exception cref="ArgumentException">Thrown if provider is not supported.</exception>
        private OAuthProviderConfig GetOAuthProviderConfiguration(string provider)
        {
            string providerLower = provider.ToLower();

            return providerLower switch
            {
                "google" => new OAuthProviderConfig
                {
                    ClientId = _configuration["GoogleKeys:ClientId"],
                    ClientSecret = _configuration["GoogleKeys:ClientSecret"],
                    TokenUrl = "https://oauth2.googleapis.com/token",
                    UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo"
                },

                "microsoft" => new OAuthProviderConfig
                {
                    ClientId = _configuration["MicrosoftKeys:ClientId"],
                    ClientSecret = _configuration["MicrosoftKeys:ClientSecret"],
                    TenantId = _configuration["MicrosoftKeys:TenantId"],
                    TokenUrl = $"https://login.microsoftonline.com/{_configuration["MicrosoftKeys:TenantId"]}/oauth2/v2.0/token",
                    UserInfoUrl = "https://graph.microsoft.com/v1.0/me"
                },

                "facebook" => new OAuthProviderConfig
                {
                    ClientId = _configuration["FacebookKeys:ClientId"],
                    ClientSecret = _configuration["FacebookKeys:ClientSecret"],
                    TokenUrl = "https://graph.facebook.com/v13.0/oauth/access_token",
                    UserInfoUrl = "https://graph.facebook.com/me?fields=id,name,email,picture"
                },

                "github" => new OAuthProviderConfig
                {
                    ClientId = _configuration["GitHubKeys:ClientId"],
                    ClientSecret = _configuration["GitHubKeys:ClientSecret"],
                    TokenUrl = "https://github.com/login/oauth/access_token",
                    UserInfoUrl = "https://api.github.com/user"
                },

                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };
        }

        #endregion

        #region Token Exchange

        /// <summary>
        /// Exchanges the authorization code for an access token using provider-specific configuration.
        /// </summary>
        private async Task<OpenAuthTokenResponse> ExchangeCodeForTokenAsync(
            string authorizationCode, OAuthProviderConfig config)
        {
            var tokenRequest = PrepareTokenRequest(authorizationCode, config);

            var response = await _httpClient.PostAsync(config.TokenUrl, new FormUrlEncodedContent(tokenRequest));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token exchange failed for {Provider}: {Response}", config.Provider, responseContent);
            }

            return ParseTokenResponse(responseContent, config.Provider);
        }

        /// <summary>
        /// Prepares the form data required for token exchange request.
        /// </summary>
        private Dictionary<string, string> PrepareTokenRequest(string authorizationCode, OAuthProviderConfig config)
        {
            var request = new Dictionary<string, string>
            {
                { "code", authorizationCode },
                { "client_id", config.ClientId },
                { "client_secret", config.ClientSecret },
                { "redirect_uri", "http://localhost:4200/auth/callback" },
                { "grant_type", "authorization_code" }
            };

            if (config.Provider == "github")
                request.Add("scope", "read:user user:email");

            if (config.Provider == "microsoft")
                request.Add("scope", "https://graph.microsoft.com/User.Read email openid profile");

            return request;
        }

        /// <summary>
        /// Parses the token response, handling GitHub's special URL-encoded format.
        /// </summary>
        private OpenAuthTokenResponse ParseTokenResponse(string responseContent, string provider)
        {
            if (provider.ToLower() == "github")
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(responseContent);
                return new OpenAuthTokenResponse
                {
                    AccessToken = queryParams["access_token"],
                    TokenType = queryParams["token_type"],
                    Scope = queryParams["scope"]
                };
            }

            return JsonSerializer.Deserialize<OpenAuthTokenResponse>(responseContent)
                    ?? throw new Exception("Failed to deserialize token response from provider.");
        }

        #endregion

        #region User Information Retrieval

        /// <summary>
        /// Retrieves user information from the OAuth provider using the access token.
        /// Routes to provider-specific logic when necessary.
        /// </summary>
        private async Task<OAuthUserInfo> GetUserInfoFromProviderAsync(
            string accessToken, string provider, string userInfoUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            if (provider.ToLower() == "github")
            {
                return await GetGitHubUserInfoAsync(accessToken);
            }

            return await GetStandardUserInfoAsync(userInfoUrl, provider);
        }

        /// <summary>
        /// Retrieves and maps GitHub user profile and email information.
        /// </summary>
        private async Task<OAuthUserInfo> GetGitHubUserInfoAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AngularCore");

            // Get user profile
            var userResponse = await _httpClient.GetAsync("https://api.github.com/user");
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var githubUser = JsonSerializer.Deserialize<GitHubUser>(userJson);

            // Get emails
            var emailResponse = await _httpClient.GetAsync("https://api.github.com/user/emails");
            var emailJson = await emailResponse.Content.ReadAsStringAsync();
            var emails = JsonSerializer.Deserialize<List<GitHubEmail>>(emailJson);

            var primaryEmail = emails?.FirstOrDefault(e => e.Primary)?.Email
                            ?? emails?.FirstOrDefault()?.Email;

            return new OAuthUserInfo
            {
                Provider = "github",
                ProviderKey = githubUser?.Id.ToString() ?? string.Empty,
                Email = primaryEmail ?? string.Empty,
                Name = githubUser?.Name ?? githubUser?.Login ?? string.Empty,
                ProfilePicture = githubUser?.AvatarUrl ?? string.Empty
            };
        }

        /// <summary>
        /// Retrieves user information for standard OAuth providers (Google, Facebook, Microsoft).
        /// </summary>
        private async Task<OAuthUserInfo> GetStandardUserInfoAsync(string userInfoUrl, string provider)
        {
            var userResponse = await _httpClient.GetAsync(userInfoUrl);
            userResponse.EnsureSuccessStatusCode();

            var userJson = await userResponse.Content.ReadAsStringAsync();

            if (provider.ToLower() == "microsoft")
            {
                var microsoftUser = JsonSerializer.Deserialize<MicrosoftUser>(
                    userJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                string validEmail = microsoftUser?.UserPrincipalName?.Contains("#EXT#") == true
                    ? microsoftUser.UserPrincipalName.Split("#EXT#")[0].Replace("_", "@")
                    : microsoftUser?.UserPrincipalName ?? "";

                return new OAuthUserInfo
                {
                    Provider = "microsoft",
                    ProviderKey = microsoftUser?.Id ?? "",
                    Email = validEmail,
                    Name = microsoftUser?.DisplayName ?? "",
                    ProfilePicture = "" // Requires separate call for photo
                };
            }

            return JsonSerializer.Deserialize<OAuthUserInfo>(
                       userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? throw new Exception("Failed to parse user info response from provider.");
        }

        #endregion

        #region User Management

        /// <summary>
        /// Retrieves existing user by email or creates a new one if not found.
        /// Assigns default "Customer" role to new users.
        /// </summary>
        private async Task<ApplicationUser> GetOrCreateUserAsync(OAuthUserInfo userInfo)
        {
            var user = await _accountService.GetUserByEmailAsync(userInfo.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    FullName = userInfo.Name
                };

                var createResult = await _accountService.CreateUserAsync(user, userInfo.Provider, userInfo.ProviderKey);

                if (!createResult.Succeeded)
                {
                    _logger.LogWarning("Failed to create user for email {Email}: {Errors}",
                        userInfo.Email, createResult.Errors);
                    throw new Exception("User creation failed during external login.");
                }

                await _unitOfWork.AuthUsers.AddToRoleAsync(user, "Customer");
            }

            return user;
        }

        #endregion
    }
}