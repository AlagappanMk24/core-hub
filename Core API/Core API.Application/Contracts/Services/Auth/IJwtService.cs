using Core_API.Domain.Entities.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core_API.Application.Contracts.Services.Auth
{
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the specified user with full claims and roles.
        /// </summary>
        /// <param name="user">The application user for whom the token is generated.</param>
        /// <returns>A JWT token as a string.</returns>
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);

        /// <summary>
        /// Creates a JwtSecurityToken object with claims and roles (used internally or for advanced scenarios).
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <returns>A <see cref="JwtSecurityToken"/> instance.</returns>
        Task<JwtSecurityToken> CreateTokenAsync(ApplicationUser user);

        /// <summary>
        /// Generates a short-lived OTP token for two-factor authentication verification.
        /// </summary>
        /// <param name="user">The user requesting OTP verification.</param>
        /// <returns>A short-lived JWT token string used for OTP flow.</returns>
        string GenerateOtpToken(ApplicationUser user);

        /// <summary>
        /// Validates an OTP token and returns the ClaimsPrincipal if valid.
        /// </summary>
        /// <param name="token">The OTP token to validate.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> if the token is valid; otherwise, null.</returns>
        ClaimsPrincipal? ValidateOtpToken(string token);

        /// <summary>
        /// Gets the principal from an expired token (used for token refresh).
        /// </summary>
        /// <param name="token">The expired token.</param>
        /// <returns>The claims principal from the expired token.</returns>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        /// <summary>
        /// Creates a refresh token for maintaining user sessions.
        /// </summary>
        /// <returns>A <see cref="RefreshToken"/> containing token value and expiry information.</returns>
        RefreshToken CreateRefreshToken();

        /// <summary>
        /// Generates a cryptographically secure random secret key.
        /// </summary>
        /// <param name="length">The length of the key in bytes (default: 32).</param>
        /// <returns>A Base64-encoded secret key.</returns>
        string GenerateSecretKey(int length = 32);

        /// <summary>
        /// Stores a JWT token in the database for tracking and revocation purposes.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="token">The JWT token to store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StoreTokenAsync(string userId, string token);

        /// <summary>
        /// Validates whether a stored token is valid and not revoked.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>True if the token is valid and active; otherwise, false.</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Revokes all tokens for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        Task RevokeUserTokensAsync(string userId);

        /// <summary>
        /// Revokes all active tokens for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose tokens should be revoked.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RevokeTokenAsync(string userId);

        /// <summary>
        /// Cleans up all expired tokens from the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CleanupExpiredTokensAsync();
    }
}