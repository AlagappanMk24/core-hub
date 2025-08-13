using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Authorization.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core_API.Web.Areas.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController(IRolesService rolesService, ILogger<RolesController> logger) : ControllerBase
    {
        private readonly IRolesService _rolesService = rolesService;
        private readonly ILogger<RolesController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _rolesService.GetRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            try
            {
                var permissions = await _rolesService.GetPermissionsAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching permissions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("role-menu-permissions")]
        public async Task<IActionResult> GetRoleMenuPermissions(string roleId)
        {
            try
            {
                var permissions = await _rolesService.GetRoleMenuPermissionsAsync(roleId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching role menu permissions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("role-menu-permissions")]
        public async Task<IActionResult> SaveRoleMenuPermissions([FromBody] List<RoleMenuPermissionDto> dtos)
        {
            try
            {
                await _rolesService.SaveRoleMenuPermissionsAsync(dtos);
                return Ok(new { message = "Permissions saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving role menu permissions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
