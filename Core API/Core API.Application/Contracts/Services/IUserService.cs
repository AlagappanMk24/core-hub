using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.User;
using Core_API.Application.DTOs.User.Request;
using Core_API.Domain.Entities.Identity;
using System.Security.Claims;

namespace Core_API.Application.Contracts.Services
{
    public interface IUserService
    {
        Task<PaginatedResult<UserDto>> GetUsersPaginatedAsync(UserQueryParameters parameters);
        Task<UserDto> GetUserByIdAsync(string id);
        Task<OperationResult<string>> CreateUserAsync(UserDto userDto, string currentUserId);
        Task<OperationResult<string>> UpdateUserAsync(UserDto userDto, string currentUserId);
        Task<OperationResult<string>> DeleteUserAsync(string userId, string currentUserId);
        string GetUserId(ClaimsPrincipal user);
        Task<ApplicationUser> GetApplicationUser(string userId);
        //Task<UserUpsertVM> CreateUserUpsertVMAsync(UserDto userDto);
        Task<OperationResult<string>> ReleaseEmailAsync(string userId);
        Task CleanupSoftDeletedUsersAsync();
    }
}
