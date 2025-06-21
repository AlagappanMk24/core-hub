using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Application.Contracts.Services;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Core_API.Infrastructure.Services.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Admin
{
    public class RolesService( CoreAPIDbContext context, RoleManager<IdentityRole> roleManager, IPermissionService permissionService, ILogger<RolesService> logger) : IRolesService
    {
        private readonly CoreAPIDbContext _context = context;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IPermissionService _permissionService = permissionService;
        private readonly ILogger<RolesService> _logger = logger;
        public async Task<List<RoleDto>> GetRolesAsync()
        {
            return await _roleManager.Roles
                .Select(r => new RoleDto { Id = r.Id, Name = r.Name })
                .ToListAsync();
        }
        public async Task<List<PermissionDto>> GetPermissionsAsync()
        {
            return await _context.Permissions
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    EntityName = p.EntityName,
                    Action = p.Action
                })
                .ToListAsync();
        }
        public async Task<List<RoleMenuPermissionDto>> GetRoleMenuPermissionsAsync(string roleId)
        {
            return await _context.RoleMenuPermissions
                .Where(p => p.RoleId == roleId)
                .Select(p => new RoleMenuPermissionDto
                {
                    Id = p.Id,
                    RoleId = p.RoleId,
                    MenuName = p.MenuName,
                    PermissionId = p.PermissionId,
                    IsEnabled = p.IsEnabled
                })
                .ToListAsync();
        }
        public async Task SaveRoleMenuPermissionsAsync(List<RoleMenuPermissionDto> dtos)
        {
            foreach (var dto in dtos)
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Id == dto.PermissionId);
                if (permission == null)
                {
                    throw new ArgumentException($"Permission ID {dto.PermissionId} not found");
                }

                var existing = await _context.RoleMenuPermissions
                    .FirstOrDefaultAsync(p =>
                        p.RoleId == dto.RoleId &&
                        p.MenuName == dto.MenuName &&
                        p.PermissionId == dto.PermissionId);

                var permissionString = $"{permission.EntityName}.{permission.Action}";
                if (dto.IsEnabled)
                {
                    if (existing == null)
                    {
                        await _permissionService.AddPermissionToRoleAsync(dto.RoleId, permissionString);
                        _context.RoleMenuPermissions.Add(new RoleMenuPermission
                        {
                            RoleId = dto.RoleId,
                            MenuName = dto.MenuName,
                            PermissionId = dto.PermissionId,
                            IsEnabled = true
                        });
                    }
                    else
                    {
                        existing.IsEnabled = true;
                    }
                }
                else if (existing != null)
                {
                    await _permissionService.RemovePermissionFromRoleAsync(dto.RoleId, permissionString);
                    _context.RoleMenuPermissions.Remove(existing);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}