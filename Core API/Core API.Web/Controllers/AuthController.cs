using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.DTOs.Authentication.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Core_API.Web.Areas.Auth
{
    /// <summary>
    /// Handles user authentication-related HTTP requests, including registration, login, OTP validation, and external logins.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController(IAuthService authService, IEmailSendingService emailService, ILogger<AuthController> logger) : ControllerBase
    {
        /// <param name="authService">Service for handling authentication operations.</param>
        /// <param name="logger">Logger for recording authentication events and errors.</param>
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IEmailSendingService _emailService = emailService;

        /// <summary>
        /// Registers a new user account with the provided details.
        /// </summary>
        /// <param name="registerDto">The registration details, including email, password, and optional roles.</param>
        /// <returns>An IActionResult indicating success or failure with appropriate status code and message.</returns>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">Invalid registration details or email already exists.</response>
        /// <response code="500">Internal server error during registration.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                // Attempt to register the user using the auth service
                var account = await _authService.RegisterAsync(registerDto);
                if (account != null)
                {
                    // Return success response if registration succeeded
                    if (account.IsSucceeded)
                        return Ok(account);

                    // Return error response with status code and message if registration failed
                    return StatusCode(account.StatusCode, new { success = false, message = account.Message });
                }
                // Return bad request if account creation failed unexpectedly
                return BadRequest(new { success = false, message = "Registration failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration.");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }

        /// <summary>
        /// Authenticates a user with email and password, initiating two-factor authentication (2FA) with OTP.
        /// </summary>
        /// <param name="loginDto">User login credentials, including email and password.</param>
        /// <returns>An IActionResult with OTP token and identifier if successful, or an error message.</returns>
        /// <response code="200">Login successful, OTP sent to email.</response>
        /// <response code="403">Invalid credentials or user not found.</response>
        /// <response code="429">Too many login attempts.</response>
        /// <response code="500">Internal server error during login.</response>
        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // Log the login attempt
            _logger.LogInformation("User login attempt for email: {Email}", loginDto.Email);
            try
            {
                // Attempt to authenticate the user
                var response = await _authService.LoginAsync(loginDto);
                if (!response.IsSucceeded)
                {
                    // Log and return failure response if authentication failed
                    _logger.LogWarning("Login failed for email: {Email}. {Message}", loginDto.Email, response.Message);
                    return StatusCode(response.StatusCode, new { message = response.Message });
                }
                // Return success response with OTP token and identifier
                return Ok(new { response.Message, response.Model, response.IsSucceeded });
            }
            catch (Exception ex)
            {
                // Log the error and return a generic server error response
                _logger.LogError(ex, "An error occurred during login for email: {Email}", loginDto.Email);
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        /// <summary>
        /// Validates a one-time password (OTP) to complete user authentication.
        /// </summary>
        /// <param name="dto">The OTP validation details, including OTP, token, and identifier.</param>
        /// <returns>An IActionResult with JWT token if successful, or an error message.</returns>
        /// <response code="200">OTP validated, JWT token issued.</response>
        /// <response code="403">Invalid or expired OTP or token.</response>
        /// <response code="429">Too many OTP validation attempts.</response>
        /// <response code="500">Internal server error during OTP validation.</response>
        [HttpPost("validate-otp")]
        [EnableRateLimiting("OtpPolicy")]
        public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpDto dto)
        {
            try
            {
                // Attempt to validate the OTP
                var response = await _authService.ValidateOtpAsync(dto);
                if (!response.IsSucceeded)
                {
                    // Return error response if OTP validation failed
                    return StatusCode(response.StatusCode, new { message = response.Message });
                }
                // Return success response with JWT token and user details
                return Ok(response.Model); // Return only the Model when successful
            }
            catch (Exception ex)
            {
                // Log the error and return a generic server error response
                _logger.LogError(ex, "An unexpected error occurred in ValidateOtp API for identifier: {OtpIdentifier}", dto.OtpIdentifier);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        /// <summary>
        /// Resends a one-time password (OTP) to the user's email.
        /// </summary>
        /// <param name="dto">The OTP resend request details, including the OTP identifier.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        /// <response code="200">New OTP sent successfully.</response>
        /// <response code="404">User not found for the provided identifier.</response>
        /// <response code="429">Too many OTP resend requests.</response>
        /// <response code="500">Internal server error during OTP resend.</response>
        [HttpPost("resend-otp")]
        [EnableRateLimiting("ResendOtpPolicy")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            try
            {
                // Attempt to resend the OTP
                var response = await _authService.ResendOtpAsync(dto);

                // Return response with appropriate status code and message
                return StatusCode(response.StatusCode, new { message = response.Message , isSucceeded = response.IsSucceeded });
            }
            catch (Exception ex)
            {
                // Log the error and return a generic server error response
                _logger.LogError(ex, "An unexpected error occurred in ResendOtp API.");
                return StatusCode(500, new { message = "An internal server error occurred." });
            }
        }

        /// <summary>
        /// Sends a password reset link to the user's email.
        /// </summary>
        /// <param name="dto">The forgot password request details, including the user's email.</param>
        /// <returns>An IActionResult indicating whether the reset link was sent.</returns>
        /// <response code="200">Reset link sent successfully.</response>
        /// <response code="404">Email not found.</response>
        /// <response code="500">Internal server error during forgot password process.</response>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            try
            {
                // Attempt to send password reset link
                var response = await _authService.ForgotPasswordAsync(dto);

                if (!response.IsSucceeded)
                {
                    // Return error response if email not found or link sending failed
                    return StatusCode(response.StatusCode, response.Message);
                }

                // Return success response
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the error and return a generic server error response
                _logger.LogError(ex, "Error occurred while forgot password.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Resets the user's password using a provided token.
        /// </summary>
        /// <param name="dto">The reset password request details, including email, token, and new password.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">Invalid token or password.</response>
        /// <response code="500">Internal server error during password reset.</response>
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            try
            {
                // Attempt to reset the password
                var response = await _authService.ResetPasswordAsync(dto);

                // Return response with appropriate status code and message
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the error and return a generic server error response
                _logger.LogError(ex, "Error occurred while resetting password for email: {Email}", dto.Email);
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Retrieves the OAuth2 authorization URL for an external login provider.
        /// </summary>
        /// <param name="provider">The external authentication provider (e.g., Google, Microsoft, GitHub).</param>
        /// <returns>An IActionResult with the redirect URL for authentication.</returns>
        /// <response code="200">Redirect URL generated successfully.</response>
        /// <response code="400">Unsupported provider or invalid configuration.</response>
        [HttpGet("external-login-url")]
        public IActionResult GetExternalLoginUrl(string provider)
        {
            try
            {
                // Generate OAuth2 redirect URL for the specified provider
                var authUrl = _authService.GetExternalLoginUrl(provider);

                // Return success response with redirect URL
                return Ok(new { redirectUrl = authUrl });
            }
            catch (Exception ex)
            {
                // Log the error and return a bad request response
                _logger.LogError(ex, "Error generating external login URL for provider: {Provider}", provider);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates a user using an external login provider's authorization code.
        /// </summary>
        /// <param name="model">The external login details, including authorization code and provider.</param>
        /// <returns>An IActionResult with a JWT token upon successful login.</returns>
        /// <response code="200">Login successful, JWT token returned.</response>
        /// <response code="400">Invalid authorization code or provider.</response>
        /// <response code="500">Internal server error during external login.</response>
        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto model)
        {
            try
            {
                // Exchange authorization code for JWT token
                var token = await _authService.ExchangeAuthCodeForTokenAsync(model);

                // Return success response with JWT token
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                // Log the error and return a bad request response
                _logger.LogError(ex, "Error in external login for provider: {Provider}", model.Provider);
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPost("update-company")]
        //[Authorize]
        //public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyDto dto)
        //{
        //    try
        //    {
        //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var user = await _userManager.FindByIdAsync(userId);
        //        if (user == null)
        //        {
        //            return NotFound(new { message = "User not found" });
        //        }

        //        user.CompanyId = dto.CompanyId;
        //        var result = await _userManager.UpdateAsync(user);
        //        if (!result.Succeeded)
        //        {
        //            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        //        }

        //        // Generate new JWT with updated CompanyId
        //        var token = GenerateJwtToken(user);
        //        return Ok(new { token });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating company for user");
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        [HttpPost("request-company")]
        public async Task<IActionResult> RequestCompany([FromBody] CompanyRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return BadRequest(new { message = "Email and company name are required." });
            }

            var emailBody = $"New company request:\n\n" +
                            $"Requested Company Name: {request.CompanyName}\n" +
                            $"Requester Email: {request.Email}\n" +
                            $"Date: {DateTime.UtcNow}\n\n" +
                            $"Please review and add the company if appropriate.";

            //await _emailService.SendEmailAsync("admin@yourdomain.com", "New Company Request", emailBody);

            return Ok(new { message = "Your request has been sent to the administrator. You will be notified once processed." });
        }
        public class CompanyRequestDto
        {
            public string Email { get; set; }
            public string CompanyName { get; set; }
        }
        //public class UpdateCompanyDto
        //{
        //    public int CompanyId { get; set; }
        //}
    }
}