using Core_API.Application.DTOs.Authorization.Request;

namespace Core_API.Application.Contracts.Services
{
    public interface IRolesService
    {
        Task<List<RoleDto>> GetRolesAsync();
        Task<List<PermissionDto>> GetPermissionsAsync();
        Task<List<RoleMenuPermissionDto>> GetRoleMenuPermissionsAsync(string roleId);
        Task SaveRoleMenuPermissionsAsync(List<RoleMenuPermissionDto> dtos);
    }
}
