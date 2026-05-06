using Core_API.Application.Contracts.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core_API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController(IUserService userService, ILogger<UserController> logger) : BaseApiController
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<UserController> _logger = logger;

        /// <summary>
        /// Get all users for task assignment (simplified list)
        /// </summary>
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserList()
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var users = await _userService.GetUserListAsync(context);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user list");
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Get users by company ID
        /// </summary>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsersByCompany(int companyId)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                // Check permission
                if (!context.IsSuperAdmin && context.CompanyId != companyId)
                    return Forbid();

                var users = await _userService.GetUsersByCompanyAsync(companyId, context);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        [HttpGet("role/{role}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var users = await _userService.GetUsersByRoleAsync(role, context);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role {Role}", role);
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }
    }
}
