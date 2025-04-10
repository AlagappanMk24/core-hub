using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Domain.Models;
using Core_API.Domain.Models.Entities;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Core_API.Web.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="registerDto">The registration details of the user.</param>
        /// <returns>Returns success or failure response based on registration status.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var account = await _authService.RegisterAsync(registerDto);
                if (account != null)
                {
                    if (account.IsSucceeded)
                        return Ok(account);

                    return StatusCode(account.StatusCode, new { success = false, message = account.Message });
                }
                return BadRequest(new { success = false, message = "Registration failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration.");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }

        /// <summary>
        /// Authenticates a user and returns a token if successful.
        /// </summary>
        /// <param name="loginDto">User login credentials.</param>
        /// <returns>Returns success or failure response based on the user credentials.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            _logger.LogInformation("User login attempt for email: {Email}", loginDto.Email);
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                if (!response.IsSucceeded)
                {
                    _logger.LogWarning("Login failed for email: {Email}. {Message}", loginDto.Email, response.Message);
                    return StatusCode(response.StatusCode, new { message = response.Message });
                }
                return StatusCode(response.StatusCode, response.Model ?? new { response.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email: {Email}", loginDto.Email);
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        /// <summary>
        /// Validates a one-time password (OTP) for user authentication.
        /// </summary>
        /// <param name="dto">The OTP validation details.</param>
        /// <returns>Returns success or failure response based on OTP validation.</returns>
        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpDto dto)
        {
            try
            {
                var response = await _authService.ValidateOtpAsync(dto);
                if (!response.IsSucceeded)
                {
                    return StatusCode(response.StatusCode, new { message = response.Message });
                }
                return Ok(response.Model); // Return only the Model when successful
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in ValidateOtp API.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            try
            {
                var response = await _authService.ResendOtpAsync(dto.Email);
                return StatusCode(response.StatusCode, new { message = response.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in ResendOtp API.");
                return StatusCode(500, new { message = "An internal server error occurred." });
            }
        }

        /// <summary>
        /// Sends a password reset link to the user's email.
        /// </summary>
        /// <param name="dto">The forgot password request details.</param>
        /// <returns>Returns a message indicating whether the reset link was sent.</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            try
            {
                var response = await _authService.ForgotPasswordAsync(dto);
                if (!response.IsSucceeded)
                {
                    return StatusCode(response.StatusCode, response.Message);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while forgot password.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Resets the user's password.
        /// </summary>
        /// <param name="dto">The reset password request details.</param>
        /// <returns>Returns success or failure response.</returns>
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            try
            {
                var response = await _authService.ResetPasswordAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Gets the external login URL for third-party authentication.
        /// </summary>
        /// <param name="provider">The external authentication provider (e.g., Google, Facebook, Microsoft).</param>
        /// <returns>Returns a redirect URL for authentication.</returns>
        [HttpGet("external-login-url")]
        public IActionResult GetExternalLoginUrl(string provider)
        {
            try
            {
                var authUrl = _authService.GetExternalLoginUrl(provider);
                return Ok(new { redirectUrl = authUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetExternalLoginUrl for provider: {Provider}", provider);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates a user using an external login provider.
        /// </summary>
        /// <param name="model">The external login authentication details.</param>
        /// <returns>Returns an authentication token upon successful login.</returns>
        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto model)
        {
            try
            {
                var token = await _authService.ExchangeAuthCodeForTokenAsync(model);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExternalLogin for provider: {Provider}", model.Provider);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}