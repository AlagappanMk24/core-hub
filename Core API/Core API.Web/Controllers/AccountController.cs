using Core_API.Application.Contracts.Service;
using Core_API.Application.DTOs.Authentication.Request;
using Core_API.Application.DTOs.User.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core_API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,User,Customer")]
    public class AccountController(IAccountService accountService, ILogger<AccountController> logger) : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;
        private readonly ILogger<AccountController> _logger = logger;

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="dto">Contains the current password and new password.</param>
        /// <returns>Returns a status code with a success or failure message.</returns>
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(PasswordSettingsDto dto)
        {
            try
            {
                var response = await _accountService.ChangePasswordAsync(dto);
                return StatusCode(response.StatusCode, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Updates the user's profile information.
        /// </summary>
        /// <param name="dto">User details to be updated.</param>
        /// <param name="currentEmail">The current email of the user (passed in the header).</param>
        /// <returns>Returns a status code with a success or failure message.</returns>
        [HttpPut("edit-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto dto, [FromHeader] string currentEmail)
        {
            try
            {
                var account = await _accountService.UpdateProfile(dto, currentEmail);
                return StatusCode(account.StatusCode, account.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Deletes the user's account.
        /// </summary>
        /// <param name="dto">Login credentials to confirm account deletion.</param>
        /// <returns>Returns a status code with a success or failure message.</returns>
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount(LoginDto dto)
        {
            try
            {
                var response = await _accountService.DeleteAccountAsync(dto);
                return StatusCode(response.StatusCode, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting account.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
