using AutoMapper;
using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Application.Contracts.DTOs.Response;
using Core_API.Application.Contracts.DTOs.Response.GitHubResponse;
using Core_API.Application.Contracts.DTOs.Response.MicrosoftResponse;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Domain.Models;
using Core_API.Domain.Models.Entities;
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
    public class AuthService(IMapper mapper, IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings, IConfiguration configuration, HttpClient httpClient, ILogger<AuthService> logger, IAccountService accountService, IEmailService emailService) : IAuthService
    {
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IAccountService _accountService = accountService;
        private readonly IEmailService _emailService = emailService;
        public async Task<ResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _unitOfWork.Users.FindByEmailAsync(registerDto.Email) != null)
            {
                return new ResponseDto
                {
                    Message = $"Email '{registerDto.Email}' is already taken.",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            var user = _mapper.Map<ApplicationUser>(registerDto);

            var result = await _unitOfWork.Users.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = "";
                foreach (var error in result.Errors)
                {
                    errors += error.Description;
                    if (error != result.Errors.LastOrDefault()) // Add comma and newline only if not the last error
                    {
                        errors += ", \n";
                    }
                }
                return new ResponseDto
                {
                    Message = errors,
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }
            if (registerDto.Roles is null)
            {
                await _unitOfWork.Users.AddToRoleAsync(user, "User");
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _unitOfWork.Users.AddToRoleAsync(user, role);
                }
            }

            return new ResponseDto
            {
                Message = "Account created successfully.",
                IsSucceeded = true,
                StatusCode = 200
            };
        }
        public async Task<ResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _unitOfWork.Users.FindByEmailAsync(loginDto.Email);

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

                var isPasswordValid = await _unitOfWork.Users.CheckPasswordAsync(user, loginDto.Password);

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

                // Generate OTP
                var otp = GenerateOtp();
                user.TwoFactorCode = otp;
                user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5); // OTP expires in 5 minutes
                await _unitOfWork.Users.UpdateAsync(user);

                // Send OTP via Email
                await _emailService.SendOtpEmailAsync(user.Email, otp);

                _logger.LogInformation("OTP sent to email: {Email}", user.Email);

                return new ResponseDto
                {
                    StatusCode = 200,
                    IsSucceeded = true,
                    Model = new { Message = "OTP has been sent to your email. Please verify to continue." }
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
        public async Task<ResponseDto> ValidateOtpAsync(ValidateOtpDto dto)
        {
            try
            {
                _logger.LogInformation("OTP validation attempt for email: {Email}", dto.Email);

                var user = await _unitOfWork.Users.FindByEmailAsync(dto.Email);

                if (user is null)
                {
                    _logger.LogWarning("OTP validation failed: User not found for email {Email}", dto.Email);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "User not found."
                    };
                }

                if (user.TwoFactorCode != dto.Otp)
                {
                    _logger.LogWarning("OTP validation failed: Invalid OTP for email {Email}", dto.Email);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "Invalid OTP."
                    };
                }

                if (user.TwoFactorExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP validation failed: Expired OTP for email {Email}", dto.Email);
                    return new ResponseDto
                    {
                        StatusCode = 403,
                        IsSucceeded = false,
                        Message = "OTP Expired."
                    };
                }

                // 🔹 Clear OTP after verification
                user.TwoFactorCode = null;
                user.TwoFactorExpiry = null;
                await _unitOfWork.Users.UpdateAsync(user);

                // 🔹 Generate JWT Token
                var jwtToken = await CreateToken(user);
                var refreshToken = CreateRefreshToken();

                user.RefreshTokens.Add(refreshToken);
                await _unitOfWork.Users.UpdateAsync(user);

                _logger.LogInformation("OTP validated successfully for email {Email}", dto.Email);

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
                _logger.LogError(ex, "An error occurred during OTP validation for email {Email}", dto.Email);
                return new ResponseDto
                {
                    StatusCode = 500,
                    IsSucceeded = false,
                    Message = "An internal server error occurred. Please try again."
                };
            }
        }
        public async Task<ResponseDto> ResendOtpAsync(string email)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(email);

            if (user == null)
            {
                return new ResponseDto { StatusCode = 404, IsSucceeded = false, Message = "User not found." };
            }

            // Generate a new OTP
            var newOtp = GenerateOtp();
            user.TwoFactorCode = newOtp;
            user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);

            await _unitOfWork.Users.UpdateAsync(user);

            // Send OTP via email 
            await _emailService.SendOtpEmailAsync(user.Email, newOtp);

            return new ResponseDto { StatusCode = 200, IsSucceeded = true, Message = "A new OTP has been sent to your email." };
        }

        public async Task<ResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new ResponseDto
                {
                    StatusCode = 404,
                    IsSucceeded = false,
                    Message = "Invalid Email"
                };
            }

            var token = await _unitOfWork.Users.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var resetLink = $"http://localhost:4200/auth/reset-password?email={dto.Email}&token={encodedToken}";

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
        public async Task<ResponseDto> ResetPasswordAsync(ResetPasswordDto resetPwdDto)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(resetPwdDto.Email);
            if (user == null)
            {
                return new ResponseDto
                {
                    StatusCode = 404,
                    IsSucceeded = false,
                    Message = "Email is incorrect!"
                };
            }
            //var token = await _unitOfWork.Users.GeneratePasswordResetTokenAsync(user);
            var result = await _unitOfWork.Users.ResetPasswordAsync(user, resetPwdDto.Token, resetPwdDto.NewPassword);
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
        public string GetExternalLoginUrl(string provider)
        {
            try
            {
                _logger.LogInformation("Generating external login URL for provider: {Provider}", provider);

                string clientId, redirectUri, tenantId, authUrl;
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
        public async Task<string> ExchangeAuthCodeForTokenAsync(ExternalLoginDto model)
        {
            _logger.LogInformation("Exchanging authorization code for access token (Provider: {Provider})", model.Provider);

            string clientId, clientSecret, tokenUrl, userInfoUrl;

            string? tenantId = null;

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

            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(tokenRequest));

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token exchange failed: {Response}", responseContent);
            }

            OpenAuthTokenResponse tokenResponse;

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

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            OAuthUserInfo userInfo;

            if (model.Provider.ToLower() == "github")
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AngularCore");

                // 1️⃣ Fetch GitHub User Info
                var userResponse = await _httpClient.GetAsync(userInfoUrl);
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var githubUser = JsonSerializer.Deserialize<GitHubUser>(userJson);

                // 2️⃣ Fetch GitHub Email separately
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
            }
            _logger.LogInformation("Successfully exchanged authorization code for access token (Provider: {Provider})", model.Provider);
            return GenerateJwtToken(user);
        }
        public async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo loginInfo)
        {
            return await _unitOfWork.Users.AddLoginAsync(user, loginInfo);
        }
        public string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<JwtSecurityToken> CreateToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim("uid",user.Id)
            };

            var roles = await _unitOfWork.Users.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            SigningCredentials signing = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var JwtToken = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                signingCredentials: signing,
                expires: DateTime.Now.AddHours(6)
            );

            return JwtToken;
        }
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
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit OTP
        }
    }
}
