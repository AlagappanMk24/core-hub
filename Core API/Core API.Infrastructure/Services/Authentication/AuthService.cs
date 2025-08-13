using AutoMapper;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.DTOs.Authentication.Request;
using Core_API.Application.DTOs.Authentication.Response;
using Core_API.Application.DTOs.Authentication.Response.GitHubResponse;
using Core_API.Application.DTOs.Authentication.Response.MicrosoftResponse;
using Core_API.Domain.Entities;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Identity.OAuth;
using Core_API.Infrastructure.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Core_API.Infrastructure.Service
{
    /// <summary>
    /// Provides authentication services including user registration, login, OTP handling, and external logins.
    /// </summary>
    public class AuthService(IMapper mapper, IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings, IConfiguration configuration, HttpClient httpClient, ILogger<AuthService> logger, IAccountService accountService, IEmailSendingService emailService) : IAuthService
    {
        /// <param name="mapper">AutoMapper instance for mapping DTOs to entities.</param>
        /// <param name="unitOfWork">Unit of work for database operations.</param>
        /// <param name="jwtSettings">JWT configuration settings.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="httpClient">HTTP client for external API calls.</param>
        /// <param name="logger">Logger for recording service events.</param>
        /// <param name="accountService">Service for account management.</param>
        /// <param name="emailService">Service for sending emails.</param>
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IAccountService _accountService = accountService;
        private readonly IEmailSendingService _emailService = emailService;

        /// <summary>
        /// Registers a new user with the provided details.
        /// </summary>
        /// <param name="registerDto">The registration details, including email, password, and roles.</param>
        /// <returns>A <see cref="ResponseDto"/> indicating success or failure.</returns>
        public async Task<ResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email is already registered
            if (await _unitOfWork.AuthUsers.FindByEmailAsync(registerDto.Email) != null)
            {
                return new ResponseDto
                {
                    Message = $"Email '{registerDto.Email}' is already taken.",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            // Map DTO to ApplicationUser entity
            var user = _mapper.Map<ApplicationUser>(registerDto);

            // Create user with password
            var result = await _unitOfWork.AuthUsers.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                // Aggregate identity errors into a single message
                var errors = string.Join(", \n", result.Errors.Select(e => e.Description));
                return new ResponseDto
                {
                    Message = errors,
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            // Assign roles: default to "User" if none specified
            if (registerDto.Roles is null)
            {
                await _unitOfWork.AuthUsers.AddToRoleAsync(user, "User");
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _unitOfWork.AuthUsers.AddToRoleAsync(user, role);
                }
            }

            return new ResponseDto
            {
                Message = "Account created successfully.",
                IsSucceeded = true,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Authenticates a user and initiates two-factor authentication by sending an OTP.
        /// </summary>
        /// <param name="loginDto">The login credentials, including email and password.</param>
        /// <returns>A <see cref="ResponseDto"/> with OTP token and identifier if successful.</returns>
        public async Task<ResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _unitOfWork.AuthUsers.FindByEmailAsync(loginDto.Email);

                if (user is null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", loginDto.Email);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "User not found with this email"
                    };
                }

                // Verify password
                var isPasswordValid = await _unitOfWork.AuthUsers.CheckPasswordAsync(user, loginDto.Password);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid credentials for email {Email}", loginDto.Email);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "Invalid Email or Password!",
                    };
                }

                // Generate 6-digit OTP and unique identifier
                var otp = GenerateOtp();
                var otpIdentifier = Guid.NewGuid().ToString();

                // Update user with OTP and expiry
                user.TwoFactorCode = otp;
                user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);
                user.OtpIdentifier = otpIdentifier;
                await _unitOfWork.AuthUsers.UpdateAsync(user);

                // Generate temporary OTP token
                var otpToken = GenerateOtpToken(user);

                // Send OTP via Email
                await _emailService.SendOtpEmailAsync(user.Email, otp);

                _logger.LogInformation("OTP sent to email: {Email}", user.Email);

                return new ResponseDto
                {
                    StatusCode = 200,
                    IsSucceeded = true,
                    Message = "OTP has been sent to your email. Please verify to continue.",
                    Model = new { OtpToken = otpToken, OtpIdentifier = otpIdentifier }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email {Email}", loginDto.Email);
                return new ResponseDto
                {
                    StatusCode = 500,
                    IsSucceeded = false,
                    Message = "An internal server error occurred. Please try again."
                };
            }
        }

        /// <summary>
        /// Validates a one-time password (OTP) and issues a JWT token upon success.
        /// </summary>
        /// <param name="dto">The OTP validation details, including OTP, token, and identifier.</param>
        /// <returns>A <see cref="ResponseDto"/> with JWT token and user details if successful.</returns>
        public async Task<ResponseDto> ValidateOtpAsync(ValidateOtpDto dto)
        {
            try
            {
                _logger.LogInformation("OTP validation attempt for identifier: {OtpIdentifier}", dto.OtpIdentifier);

                // Validate OTP token
                var principal = ValidateOtpToken(dto.OtpToken);
                if (principal == null)
                {
                    _logger.LogWarning("OTP validation failed: Invalid or expired OTP token for identifier {OtpIdentifier}", dto.OtpIdentifier);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "Invalid or expired OTP token."
                    };
                }

                // Find user by OTP identifier
                var user = await _unitOfWork.AuthUsers.FindByOtpIdentifierAsync(dto.OtpIdentifier);

                if (user is null)
                {
                    _logger.LogWarning("OTP validation failed: Invalid identifier {OtpIdentifier}", dto.OtpIdentifier);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "Invalid OTP identifier."
                    };
                }

                // Verify email in token matches user
                var emailFromToken = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (emailFromToken != user.Email)
                {
                    _logger.LogWarning("OTP validation failed: Token email mismatch for identifier {OtpIdentifier}", dto.OtpIdentifier);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "Invalid OTP token."
                    };
                }

                // Verify OTP
                if (user.TwoFactorCode != dto.Otp)
                {
                    _logger.LogWarning("OTP validation failed: Invalid OTP for identifier {OtpIdentifier}", dto.OtpIdentifier);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "Invalid OTP."
                    };
                }

                // Check OTP expiry
                if (user.TwoFactorExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP validation failed: Expired OTP for identifier {OtpIdentifier}", dto.OtpIdentifier);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "OTP Expired."
                    };
                }

                // Clear OTP after successful validation
                user.TwoFactorCode = null;
                user.TwoFactorExpiry = null;
                await _unitOfWork.AuthUsers.UpdateAsync(user);

                // Generate JWT and refresh tokens
                var jwtToken = await CreateToken(user);
                var refreshToken = CreateRefreshToken();

                // Store refresh token
                user.RefreshTokens.Add(refreshToken);
                await _unitOfWork.AuthUsers.UpdateAsync(user);

                _logger.LogInformation("OTP validated successfully for identifier {OtpIdentifier}", dto.OtpIdentifier);

                return new ResponseDto
                {
                    StatusCode = 200,
                    IsSucceeded = true,
                    Model = new LoginResponseDto
                    {
                        IsAuthenticated = true,
                        Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        UserName = user.UserName,
                        Email = user.Email,
                        Message = "Login successful",
                        RefreshToken = refreshToken.Token,
                        RefreshTokenExpiration = refreshToken.ExpiresOn
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during OTP validation for identifier {OtpIdentifier}", dto.OtpIdentifier);
                return new ResponseDto
                {
                    StatusCode = 500,
                    IsSucceeded = false,
                    Message = "An internal server error occurred. Please try again."
                };
            }
        }

        /// <summary>
        /// Resends a one-time password (OTP) to the user's email.
        /// </summary>
        /// <param name="dto">The OTP resend request details, including the OTP identifier.</param>
        /// <returns>A <see cref="ResponseDto"/> indicating success or failure.</returns>
        public async Task<ResponseDto> ResendOtpAsync(ResendOtpDto dto)
        {
            try
            {
                // Find user by OTP identifier
                var user = await _unitOfWork.AuthUsers.FindByOtpIdentifierAsync(dto.OtpIdentifier);

                if (user == null)
                {
                    _logger.LogWarning("Resend OTP failed: Invalid identifier {OtpIdentifier}", dto.OtpIdentifier);
                    return new ResponseDto { StatusCode = 404, IsSucceeded = false, Message = "User not found." };
                }

                // Generate a new OTP
                var newOtp = GenerateOtp();
                user.TwoFactorCode = newOtp;
                user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);

                // Update user record
                await _unitOfWork.AuthUsers.UpdateAsync(user);

                // Send OTP via email 
                await _emailService.SendOtpEmailAsync(user.Email, newOtp);

                _logger.LogInformation("New OTP sent for identifier: {OtpIdentifier}", dto.OtpIdentifier);

                return new ResponseDto { StatusCode = 200, IsSucceeded = true, Message = "A new OTP has been sent to your email." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during OTP resend for identifier {OtpIdentifier}", dto.OtpIdentifier);
                return new ResponseDto
                {
                    StatusCode = 500,
                    IsSucceeded = false,
                    Message = "An internal server error occurred. Please try again."
                };
            }
        }

        /// <summary>
        /// Sends a password reset link to the user's email.
        /// </summary>
        /// <param name="dto">The forgot password request details, including email.</param>
        /// <returns>A <see cref="ResponseDto"/> indicating success or failure.</returns>
        public async Task<ResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            // Find user by email
            var user = await _unitOfWork.AuthUsers.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new ResponseDto
                {
                    StatusCode = 404,
                    IsSucceeded = false,
                    Message = "Invalid Email"
                };
            }

            // Generate password reset token
            var token = await _unitOfWork.AuthUsers.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var resetLink = $"http://localhost:4200/auth/reset-password?email={dto.Email}&token={encodedToken}";

            // Send reset link via email
            await _emailService.SendResetPasswordEmailAsync(
                   dto.Email,
                   "Reset Password",
                   $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

            return new ResponseDto
            {
                StatusCode = 200,
                IsSucceeded = true,
                Message = "Reset password link sent successfully."
            };
        }

        /// <summary>
        /// Resets a user's password using a provided token.
        /// </summary>
        /// <param name="resetPwdDto">The reset password details, including email, token, and new password.</param>
        /// <returns>A <see cref="ResponseDto"/> indicating success or failure.</returns>
        public async Task<ResponseDto> ResetPasswordAsync(ResetPasswordDto resetPwdDto)
        {
            // Find user by email
            var user = await _unitOfWork.AuthUsers.FindByEmailAsync(resetPwdDto.Email);
            if (user == null)
            {
                return new ResponseDto
                {
                    StatusCode = 404,
                    IsSucceeded = false,
                    Message = "Email is incorrect!"
                };
            }

            // Attempt to reset password with token
            var result = await _unitOfWork.AuthUsers.ResetPasswordAsync(user, resetPwdDto.Token, resetPwdDto.NewPassword);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    StatusCode = 400,
                    IsSucceeded = false,
                    Message = "Failed to reset password, try again."
                };
            }
            return new ResponseDto
            {
                StatusCode = 200,
                IsSucceeded = true,
                Message = "Your password reset successfully."
            };
        }

        /// <summary>
        /// Generates an OAuth2 authorization URL for an external login provider.
        /// </summary>
        /// <param name="provider">The external provider (e.g., Google, Microsoft, GitHub).</param>
        /// <returns>The OAuth2 redirect URL.</returns>
        /// <exception cref="ArgumentException">Thrown if the provider is unsupported.</exception>
        public string GetExternalLoginUrl(string provider)
        {
            try
            {
                _logger.LogInformation("Generating external login URL for provider: {Provider}", provider);

                string clientId, redirectUri, tenantId, authUrl;

                // Configure provider-specific OAuth2 URL
                switch (provider.ToLower())
                {
                    case "google":
                        clientId = _configuration["GoogleKeys:ClientId"];
                        redirectUri = "http://localhost:4200/auth/callback";
                        authUrl = $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=email%20profile&state={provider}";
                        break;

                    case "microsoft":
                        clientId = _configuration["MicrosoftKeys:ClientId"];
                        redirectUri = "http://localhost:4200/auth/callback";
                        tenantId = _configuration["MicrosoftKeys:TenantId"];
                        authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=openid%20email%20profile%20User.Read&state={provider}";
                        break;

                    case "facebook":
                        clientId = _configuration["FacebookKeys:ClientId"];
                        redirectUri = "http://localhost:4200/auth/callback";
                        authUrl = $"https://www.facebook.com/v13.0/dialog/oauth?client_id={clientId}&redirect_uri={redirectUri}&scope=email%20public_profile";
                        break;

                    case "github":
                        clientId = _configuration["GitHubKeys:ClientId"];
                        redirectUri = "http://localhost:4200/auth/callback";
                        authUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope=user:email&state={provider}";
                        break;

                    default:
                        throw new ArgumentException("Unsupported provider");
                }

                return authUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating login URL for provider: {Provider}", provider);
                throw;
            }
        }

        /// <summary>
        /// Exchanges an OAuth2 authorization code for a JWT token.
        /// </summary>
        /// <param name="model">The external login details, including authorization code and provider.</param>
        /// <returns>A JWT token for the authenticated user.</returns>
        /// <exception cref="ArgumentException">Thrown if the provider is unsupported.</exception>
        public async Task<string> ExchangeAuthCodeForTokenAsync(ExternalLoginDto model)
        {
            _logger.LogInformation("Exchanging authorization code for access token (Provider: {Provider})", model.Provider);

            string clientId, clientSecret, tokenUrl, userInfoUrl;

            string? tenantId = null;

            // Configure provider-specific OAuth2 endpoints
            switch (model.Provider.ToLower())
            {
                case "google":
                    clientId = _configuration["GoogleKeys:ClientId"];
                    clientSecret = _configuration["GoogleKeys:ClientSecret"];
                    tokenUrl = "https://oauth2.googleapis.com/token";
                    userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
                    break;

                case "microsoft":
                    clientId = _configuration["MicrosoftKeys:ClientId"];
                    clientSecret = _configuration["MicrosoftKeys:ClientSecret"];
                    tenantId = _configuration["MicrosoftKeys:TenantId"];
                    tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    userInfoUrl = "https://graph.microsoft.com/v1.0/me";
                    break;

                case "facebook":
                    clientId = _configuration["FacebookKeys:ClientId"];
                    clientSecret = _configuration["FacebookKeys:ClientSecret"];
                    tokenUrl = "https://graph.facebook.com/v13.0/oauth/access_token";
                    userInfoUrl = "https://graph.facebook.com/me?fields=id,name,email,picture";
                    break;

                case "github":
                    clientId = _configuration["GitHubKeys:ClientId"];
                    clientSecret = _configuration["GitHubKeys:ClientSecret"];
                    tokenUrl = "https://github.com/login/oauth/access_token";
                    userInfoUrl = "https://api.github.com/user";
                    break;

                default:
                    throw new ArgumentException("Unsupported provider");
            }

            // Prepare token request
            var tokenRequest = new Dictionary<string, string>
            {
                { "code", model.AuthorizationCode },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", "http://localhost:4200/auth/callback" },
                { "grant_type", "authorization_code" },
            };

            if (model.Provider.ToLower() == "github")
            {
                tokenRequest.Add("scope", "read:user user:email");
            }

            if (model.Provider.ToLower() == "microsoft")
            {
                tokenRequest.Add("scope", "https://graph.microsoft.com/User.Read email openid profile");
            }

            // Exchange code for access token
            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(tokenRequest));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token exchange failed: {Response}", responseContent);
            }

            OpenAuthTokenResponse tokenResponse;

            // Handle GitHub's non-JSON response
            if (model.Provider.ToLower() == "github")
            {
                // GitHub returns URL-encoded response, parse manually
                var queryParams = System.Web.HttpUtility.ParseQueryString(responseContent);
                tokenResponse = new OpenAuthTokenResponse
                {
                    AccessToken = queryParams["access_token"],
                    TokenType = queryParams["token_type"],
                    Scope = queryParams["scope"]
                };
            }
            else
            {
                tokenResponse = JsonSerializer.Deserialize<OpenAuthTokenResponse>(responseContent);
            }

            // Set authorization header for user info request
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            OAuthUserInfo userInfo;

            // Handle GitHub-specific user info retrieval
            if (model.Provider.ToLower() == "github")
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AngularCore");

                // Fetch GitHub User Info
                var userResponse = await _httpClient.GetAsync(userInfoUrl);
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var githubUser = JsonSerializer.Deserialize<GitHubUser>(userJson);

                // Fetch GitHub Email separately
                var emailResponse = await _httpClient.GetAsync("https://api.github.com/user/emails");
                var emailJson = await emailResponse.Content.ReadAsStringAsync();
                var emails = JsonSerializer.Deserialize<List<GitHubEmail>>(emailJson);

                var primaryEmail = emails?.FirstOrDefault(e => e.Primary)?.Email ?? emails?.FirstOrDefault()?.Email;

                userInfo = new OAuthUserInfo
                {
                    Provider = "github",
                    ProviderKey = githubUser?.Id.ToString() ?? string.Empty,
                    Email = primaryEmail ?? string.Empty,
                    Name = githubUser?.Name ?? githubUser?.Login ?? string.Empty,
                    ProfilePicture = githubUser?.AvatarUrl ?? string.Empty
                };
            }
            else
            {
                // Fetch user info for other providers
                var userResponse = await _httpClient.GetAsync(userInfoUrl);
                userResponse.EnsureSuccessStatusCode();
                var userJson = await userResponse.Content.ReadAsStringAsync();

                if (model.Provider.ToLower() == "microsoft")
                {
                    var microsoftUser = JsonSerializer.Deserialize<MicrosoftUser>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    string validEmail = microsoftUser?.UserPrincipalName.Contains("#EXT#") == true
                        ? microsoftUser.UserPrincipalName.Split("#EXT#")[0].Replace("_", "@")
                        : microsoftUser?.UserPrincipalName ?? "";

                    userInfo = new OAuthUserInfo
                    {
                        Provider = "microsoft",
                        ProviderKey = microsoftUser?.Id ?? "",
                        Email = validEmail,
                        Name = microsoftUser?.DisplayName ?? "",
                        ProfilePicture = "" // Profile picture requires separate Graph API call
                    };
                }
                else
                {
                    userInfo = JsonSerializer.Deserialize<OAuthUserInfo>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                              ?? throw new Exception("Failed to parse user info response");
                }
            }

            // Check if user exists or create new user
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
                    _logger.LogWarning("Failed to create user: {Errors}", createResult.Errors);
                    throw new Exception("User creation failed");
                }
                await _unitOfWork.AuthUsers.AddToRoleAsync(user, "Customer");
            }
            _logger.LogInformation("Successfully exchanged authorization code for access token (Provider: {Provider})", model.Provider);
            return await GenerateJwtToken(user);
        }

        /// <summary>
        /// Adds an external login provider to a user's account.
        /// </summary>
        /// <param name="user">The user to add the login to.</param>
        /// <param name="loginInfo">The external login information.</param>
        /// <returns>An <see cref="IdentityResult"/> indicating success or failure.</returns>
        public async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo loginInfo)
        {
            return await _unitOfWork.AuthUsers.AddLoginAsync(user, loginInfo);
        }

        /// <summary>
        /// Generates a JWT token for a user.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <returns>A JWT token as a string.</returns>
        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("companyId", user.CompanyId?.ToString() ?? "0"),
                new Claim("customerId", user.CustomerId?.ToString() ?? "0")
            };

            var roles = await _unitOfWork.AuthUsers.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:ValidIssuer"],
                audience: _configuration["JwtSettings:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Creates a JWT token with user claims and roles.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <returns>A <see cref="JwtSecurityToken"/> containing user claims.</returns>
        private async Task<JwtSecurityToken> CreateToken(ApplicationUser user)
        {
            // Build claims list
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(JwtRegisteredClaimNames.Sub,user.Id),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim("uid",user.Id),
                new Claim("companyId", user.CompanyId?.ToString() ?? "0"),
                new Claim("customerId", user.CustomerId?.ToString() ?? "0")
            };

            // Add user roles to claims
            var roles = await _unitOfWork.AuthUsers.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            SigningCredentials signing = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create JWT token
            var JwtToken = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                signingCredentials: signing,
                expires: DateTime.Now.AddHours(6)
            );
            return JwtToken;
        }

        /// <summary>
        /// Generates a temporary OTP token for 2FA verification.
        /// </summary>
        /// <param name="user">The user requesting OTP verification.</param>
        /// <returns>An OTP token as a string.</returns>
        private string GenerateOtpToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("otp_purpose", "verification")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates an OTP token.
        /// </summary>
        /// <param name="token">The OTP token to validate.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> if valid, otherwise null.</returns>
        private ClaimsPrincipal ValidateOtpToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.ValidIssuer,
                    ValidAudience = _jwtSettings.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a refresh token for session continuity.
        /// </summary>
        /// <returns>A <see cref="RefreshToken"/> with a random token and expiry.</returns>
        private RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddHours(6),
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Generates a 6-digit random OTP.
        /// </summary>
        /// <returns>A 6-digit OTP as a string.</returns>
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit OTP
        }
    }
}