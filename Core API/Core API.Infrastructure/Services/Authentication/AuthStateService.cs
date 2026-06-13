using Core_API.Application.Contracts.Services.Auth;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Services.Authentication
{
    public class AuthStateService(CoreInvoiceDbContext context) : IAuthStateService
    {
        private readonly CoreInvoiceDbContext _context = context;
        public async Task<AuthState> CreateAuthStateAsync(string userId)
        {
            var authState = new AuthState
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await _context.AuthStates.AddAsync(authState);
            await _context.SaveChangesAsync();

            return authState;
        }
        public async Task<AuthState> GetAuthStateAsync(string authStateId)
        {
            await CleanupExpiredStatesAsync();
            return await _context.AuthStates.FindAsync(authStateId);
        }
        public async System.Threading.Tasks.Task UpdateAuthStateAsync(AuthState authState)
        {
            _context.AuthStates.Update(authState);
            await _context.SaveChangesAsync();
        }
        public async System.Threading.Tasks.Task CleanupExpiredStatesAsync()
        {
            var expiredStates = await _context.AuthStates
                 .Where(x => x.ExpiresAt < DateTime.UtcNow && x.UserId == null)
                 .ToListAsync();

            _context.AuthStates.RemoveRange(expiredStates);
            await _context.SaveChangesAsync();
        }
    }
}