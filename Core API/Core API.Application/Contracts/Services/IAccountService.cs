using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Application.Contracts.DTOs.Response;
using Core_API.Application.Features.Users.DTOs;
using Core_API.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Core_API.Application.Contracts.Service
{
    public interface IAccountService
    {
        Task<ResponseDto> ChangePasswordAsync(PasswordSettingsDto dto);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string provider, string providerKey);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ResponseDto> DeleteAccountAsync(LoginDto dto);
        Task<ResponseDto> UpdateProfile(UserDto dto, string currentEmail);
    }
}