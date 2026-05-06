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
    public class JwtService(IConfiguration configuration, IUnitOfWork unitOfWork,CoreInvoiceDbContext context, IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger) : IJwtService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly CoreInvoiceDbContext _context = context;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly ILogger<JwtService> _logger = logger;

        /// <inheritdoc/>
        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var claims = await BuildUserClaimsAsync(user);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc/>
        public async Task<JwtSecurityToken> CreateToken(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var claims = await BuildUserClaimsAsync(user);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: signingCredentials);
        }

        /// <inheritdoc/>
        public string GenerateOtpToken(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("otp_purpose", "verification")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);

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
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.ValidIssuer,
                    ValidAudience = _jwtSettings.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OTP token validation failed");
                return null;
            }
        }

        /// <inheritdoc/>
        public string GenerateSecretKey(int length = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var byteArray = new byte[length];
            rng.GetBytes(byteArray);
            return Convert.ToBase64String(byteArray);
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
                ExpiresOn = DateTime.UtcNow.AddHours(6),
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task StoreTokenAsync(string userId, string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);
            ArgumentException.ThrowIfNullOrEmpty(token);

            try
            {
                var authToken = new AuthToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:ExpireHours"] ?? "12"))
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
        public async System.Threading.Tasks.Task CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.AuthTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow)
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

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task RevokeTokenAsync(string userId)
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

        #region Private Helpers

        /// <summary>
        /// Builds the complete list of claims for a user including roles.
        /// </summary>
        private async Task<List<Claim>> BuildUserClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("uid", user.Id),
                new Claim("companyId", user.CompanyId?.ToString() ?? "0"),
                new Claim("customerId", user.CustomerId?.ToString() ?? "0")
            };

            var roles = await _unitOfWork.AuthUsers.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        #endregion
    }
}