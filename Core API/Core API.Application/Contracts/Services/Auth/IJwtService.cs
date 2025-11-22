using Core_API.Domain.Entities.Identity;

namespace Core_API.Application.Contracts.Services.Auth
{
    public interface IJwtService
    {
        string GenerateSecretKey(int length = 32);
        string GenerateJwtToken(ApplicationUser user);
        Task StoreTokenAsync(string userId, string token);
        Task<bool> ValidateTokenAsync(string token);
        Task CleanupExpiredTokensAsync();
        Task RevokeTokenAsync(string userId);
    }
}