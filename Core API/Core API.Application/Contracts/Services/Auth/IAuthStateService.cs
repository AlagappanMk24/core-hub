using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Services.Auth
{
    public interface IAuthStateService
    {
        Task<AuthState> CreateAuthStateAsync(string userId);
        Task<AuthState> GetAuthStateAsync(string authStateId);
        Task UpdateAuthStateAsync(AuthState authState);
        Task CleanupExpiredStatesAsync();
    }
}