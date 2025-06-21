using Core_API.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IAuthRepository
    {
        Task<ApplicationUser> FindByNameAsync(string name);
        Task<ApplicationUser> FindByEmailAsync(string email);
        Task<ApplicationUser> FindByOtpIdentifierAsync(string otpIdentifier);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddExternalLoginAsync(ApplicationUser user, UserLoginInfo loginInfo);
        Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo loginInfo);
        Task AddToRoleAsync(ApplicationUser user, string role);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        Task<IdentityResult> DeleteAsync(ApplicationUser user);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    }
}