using System.Security.Claims;

namespace Core_API.Application.Contracts.Services
{
    public interface IPermissionService
    {
        /// <summary>
        /// Checks if a user has a specific permission
        /// </summary>
        Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permission);

        /// <summary>
        /// Gets all permissions assigned to a user through their roles
        /// </summary>
        Task<IEnumerable<string>> GetPermissionsForUserAsync(ClaimsPrincipal user);

        /// <summary>
        /// Gets all permissions assigned to a specific role
        /// </summary>
        Task<IEnumerable<string>> GetPermissionsForRoleAsync(string roleName);

        /// <summary>
        /// Adds a permission to a role
        /// </summary>
        Task AddPermissionToRoleAsync(string roleName, string permission);

        /// <summary>
        /// Removes a permission from a role
        /// </summary>
        Task RemovePermissionFromRoleAsync(string roleName, string permission);

        /// <summary>
        /// Seeds default permissions for roles
        /// </summary>
        Task SeedDefaultPermissionsAsync();
    }
}
