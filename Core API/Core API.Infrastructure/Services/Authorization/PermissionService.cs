using Core_API.Application.Common.Constants;
using Core_API.Application.Contracts.Services;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Services.Authorization
{
    public class PermissionService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    CoreAPIDbContext dbContext,
    ILogger<PermissionService> logger) : IPermissionService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly CoreAPIDbContext _dbContext = dbContext;
        private readonly ILogger<PermissionService> _logger = logger;
        public async Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            try
            {
                // Super admin has all permissions
                if (user.IsInRole(AppConstants.Role_Admin_Super))
                {
                    return true;
                }

                // Get user roles
                var appUser = await _userManager.GetUserAsync(user);
                if (appUser == null)
                {
                    return false;
                }

                var userRoles = await _userManager.GetRolesAsync(appUser);

                // Check if any of the user's roles have the required permission
                foreach (var role in userRoles)
                {
                    var permissions = await GetPermissionsForRoleAsync(role);

                    // Check for direct permission match
                    if (permissions.Contains(permission))
                    {
                        return true;
                    }

                    // Check for "Manage" permission (which implies all other permissions)
                    var entityName = permission.Split('.')[0];
                    var managePermission = $"{entityName}.{AuthorizationConstants.Actions.Manage}";

                    if (permissions.Contains(managePermission))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user permission: {Permission}", permission);
                return false;
            }
        }
        public async Task<IEnumerable<string>> GetPermissionsForUserAsync(ClaimsPrincipal user)
        {
            var result = new HashSet<string>();

            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
            {
                return result;
            }

            var userRoles = await _userManager.GetRolesAsync(appUser);

            foreach (var role in userRoles)
            {
                var rolePermissions = await GetPermissionsForRoleAsync(role);
                foreach (var permission in rolePermissions)
                {
                    result.Add(permission);
                }
            }

            return result;
        }
        public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(string roleName)
        {
            try
            {
                // Super admin has all permissions
                if (roleName == AppConstants.Role_Admin_Super)
                {
                    // Return all defined permissions
                    return GetAllDefinedPermissions();
                }

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    return new List<string>();
                }

                var permissions = await _dbContext.RoleMenuPermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Join(_dbContext.Set<Permission>(),
                          rp => rp.PermissionId,
                          p => p.Id,
                          (rp, p) => $"{p.EntityName}.{p.Action}")
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for role: {Role}", roleName);
                return new List<string>();
            }
        }
        public async Task AddPermissionToRoleAsync(string roleName, string permission)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                throw new ArgumentException($"Role {roleName} not found");
            }

            var parts = permission.Split('.');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Permission must be in format 'Entity.Action'");
            }

            var entityName = parts[0];
            var action = parts[1];

            var permissionEntity = await _dbContext.Permissions
               .FirstOrDefaultAsync(p => p.EntityName == entityName && p.Action == action);

            if (permissionEntity == null)
            {
                permissionEntity = new Permission
                {
                    EntityName = entityName,
                    Action = action
                };
                _dbContext.Permissions.Add(permissionEntity);
                await _dbContext.SaveChangesAsync();
            }

            var rolePermission = await _dbContext.RoleMenuPermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permissionEntity.Id);

            if (rolePermission == null)
            {
                rolePermission = new RoleMenuPermission
                {
                    RoleId = role.Id,
                    MenuName = entityName, // Use EntityName as MenuName
                    PermissionId = permissionEntity.Id,
                    IsEnabled = true
                };
                _dbContext.RoleMenuPermissions.Add(rolePermission);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task RemovePermissionFromRoleAsync(string roleName, string permission)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                throw new ArgumentException($"Role {roleName} not found");
            }

            var parts = permission.Split('.');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Permission must be in format 'Entity.Action'");
            }

            var entityName = parts[0];
            var action = parts[1];

            var permissionEntity = await _dbContext.Permissions
               .FirstOrDefaultAsync(p => p.EntityName == entityName && p.Action == action);

            if (permissionEntity != null)
            {
                var rolePermission = await _dbContext.RoleMenuPermissions
                         .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permissionEntity.Id);

                if (rolePermission != null)
                {
                    _dbContext.RoleMenuPermissions.Remove(rolePermission);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        public async Task SeedDefaultPermissionsAsync()
        {
            try
            {
                _logger.LogInformation("Seeding default permissions for roles...");

                // First, ensure all permission entities exist
                var allPermissions = GetAllDefinedPermissions();
                foreach (var permission in allPermissions)
                {
                    var parts = permission.Split('.');
                    if (parts.Length != 2) continue;

                    var entityName = parts[0];
                    var action = parts[1];

                    var permissionEntity = await _dbContext.Permissions
                       .FirstOrDefaultAsync(p => p.EntityName == entityName && p.Action == action);

                    if (permissionEntity == null)
                    {
                        permissionEntity = new Permission
                        {
                            EntityName = entityName,
                            Action = action
                        };
                        _dbContext.Permissions.Add(permissionEntity);
                    }
                }
                await _dbContext.SaveChangesAsync();

                // Now assign permissions to roles based on defaults
                foreach (var rolePermissionPair in AuthorizationConstants.DefaultRolePermissions)
                {
                    var roleName = rolePermissionPair.Key;
                    var permissions = rolePermissionPair.Value;

                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        _logger.LogWarning("Role {Role} not found, skipping permission assignment", roleName);
                        continue;
                    }

                    foreach (var permission in permissions)
                    {
                        await AddPermissionToRoleAsync(roleName, permission);
                    }
                }

                _logger.LogInformation("Successfully seeded default permissions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default permissions");
                throw;
            }
        }
        private IEnumerable<string> GetAllDefinedPermissions()
        {
            // Get all permission constants
            return typeof(AuthorizationConstants.Permissions)
                      .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                      .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                      .Where(fi => fi.FieldType == typeof(string))
                      .Select(fi => (string)fi.GetRawConstantValue())
                      .ToList();
        }
    }
}
