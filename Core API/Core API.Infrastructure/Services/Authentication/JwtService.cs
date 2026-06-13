using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Configuration.Settings;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core_API.Infrastructure.Services.Authentication
{
    /// <summary>
    /// Service responsible for JWT token generation, validation, refresh token management,
    /// and token persistence/revocation.
    /// </summary>
    public class JwtService(IOptions<JwtSettings> jwtSettings, CoreInvoiceDbContext context, IUnitOfWork unitOfWork, ILogger<JwtService> logger) : IJwtService
    {
        #region Private Fields

        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly CoreInvoiceDbContext _context = context;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<JwtService> _logger = logger;

        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var token = await CreateTokenAsync(user);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc/>
        public async Task<JwtSecurityToken> CreateTokenAsync(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var claims = await BuildUserClaimsAsync(user);
            var signingCredentials = GetSigningCredentials();

            return new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours),
                signingCredentials: signingCredentials);
        }

        /// <inheritdoc/>
        public string GenerateOtpToken(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("otp_purpose", "verification"),
                new("otp_identifier", Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.OtpExpireMinutes),
                signingCredentials: GetSigningCredentials());

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? ValidateOtpToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters(validateLifetime: true);

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "OTP token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "OTP token has invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OTP token validation failed");
                return null;
            }
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters(validateLifetime: false);

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get principal from expired token");
                return null;
            }
        }

        /// <inheritdoc/>
        public RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddHours(_jwtSettings.RefreshTokenExpireHours),
                CreatedOn = DateTime.UtcNow,
                CreatedByIp = GetCurrentIpAddress()
            };
        }

        /// <inheritdoc/>
        public string GenerateSecretKey(int length = 32)
        {
            var byteArray = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(byteArray);
            return Convert.ToBase64String(byteArray);
        }

        /// <inheritdoc/>
        public async Task StoreTokenAsync(string userId, string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);
            ArgumentException.ThrowIfNullOrEmpty(token);

            try
            {
                var authToken = new AuthToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = GetCurrentIpAddress()
                };

                await _context.AuthTokens.AddAsync(authToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation("JWT token stored successfully for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store JWT token for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);

            await CleanupExpiredTokensAsync();

            return await _context.AuthTokens
                .AnyAsync(t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
        }

        /// <inheritdoc/>
        public async Task RevokeUserTokensAsync(string userId)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);

            try
            {
                var tokens = await _context.AuthTokens
                    .Where(t => t.UserId == userId && !t.IsRevoked)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = GetCurrentIpAddress();
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Revoked {Count} tokens for user {UserId}", tokens.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking tokens for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task RevokeTokenAsync(string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);

            try
            {
                var authToken = await _context.AuthTokens
                    .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);

                if (authToken != null)
                {
                    authToken.IsRevoked = true;
                    authToken.RevokedAt = DateTime.UtcNow;
                    authToken.RevokedByIp = GetCurrentIpAddress();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Revoked token for user {UserId}", authToken.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.AuthTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow)
                    .Take(1000) // Limit batch size
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _context.AuthTokens.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} expired tokens", expiredTokens.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning up expired tokens");
                throw;
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Builds the complete list of claims for a user including roles.
        /// </summary>
        private async Task<List<Claim>> BuildUserClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("uid", user.Id),
                new("companyId", user.CompanyId?.ToString() ?? "0"),
                new("customerId", user.CustomerId?.ToString() ?? "0"),
                new("fullName", user.FullName ?? string.Empty)
            };

            // Add roles as claims
            var roles = await _unitOfWork.AuthUsers.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role)); // For backward compatibility
            }

            // Add custom permissions if needed
            // var permissions = await _unitOfWork.AuthUsers.GetPermissionsAsync(user);
            // foreach (var permission in permissions)
            // {
            //     claims.Add(new Claim("permission", permission));
            // }

            return claims;
        }

        /// <summary>
        /// Gets the signing credentials using the configured secret key.
        /// </summary>
        private SigningCredentials GetSigningCredentials()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// Gets the token validation parameters.
        /// </summary>
        /// <param name="validateLifetime">Whether to validate token lifetime.</param>
        private TokenValidationParameters GetTokenValidationParameters(bool validateLifetime = true)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.ValidIssuer,
                ValidAudience = _jwtSettings.ValidAudience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };
        }

        /// <summary>
        /// Gets the current IP address from the HTTP context.
        /// </summary>
        private static string GetCurrentIpAddress()
        {
            // This should be enhanced to get actual IP from HttpContextAccessor
            // For now, return a placeholder
            return "unknown";
        }

        #endregion
    }
}