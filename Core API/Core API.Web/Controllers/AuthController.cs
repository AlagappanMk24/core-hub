using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.DTOs.Authentication.Request;
using Core_API.Application.DTOs.Authentication.Request.CompanyRequest;
using Core_API.Application.DTOs.Authentication.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Core_API.Web.Controllers;

/// <summary>
/// Handles user authentication-related HTTP requests, including registration, login, OTP validation, and external logins.
/// </summary>
[Route("api/auth")]
[ApiController]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController(IAuthService authService, ICompanyRequestService companyRequestService, ILogger<AuthController> logger) : ControllerBase
{
    /// <param name="authService">Service for handling authentication operations.</param>
    /// <param name="companyRequestService">Service for handling company requests.</param>
    /// <param name="logger">Logger for recording events and errors.</param>
    private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    private readonly ICompanyRequestService _companyRequestService = companyRequestService ?? throw new ArgumentNullException(nameof(companyRequestService));
    private readonly ILogger<AuthController> _logger = logger;

    /// <summary>
    /// Registers a new user account with the provided details.
    /// </summary>
    /// <param name="registerDto">The registration details, including email, password, and optional roles.</param>
    /// <returns>An IActionResult indicating success or failure with appropriate status code and message.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Invalid registration details or email already exists.</response>
    /// <response code="409">Email is already taken.</response>
    /// <response code="500">Internal server error during registration.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for registration: {Errors}",
                JsonSerializer.Serialize(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", registerDto.Email);
            var account = await _authService.RegisterAsync(registerDto);
            if (account != null)
            {
                // Return success response if registration succeeded
                if (account.IsSucceeded)
                {
                    _logger.LogInformation("User registered successfully with email: {Email}", registerDto.Email);
                    return Ok(account);
                }
                _logger.LogWarning("Registration failed for email {Email}: {Message}", registerDto.Email, account.Message);

                return account.StatusCode switch
                {
                    409 => Conflict(new ProblemDetails
                    {
                        Title = "Email Already Exists",
                        Detail = account.Message,
                        Status = StatusCodes.Status409Conflict
                    }),
                    _ => BadRequest(new ProblemDetails
                    {
                        Title = "Registration Failed",
                        Detail = account.Message,
                        Status = StatusCodes.Status400BadRequest
                    })
                };

            }
            _logger.LogError("Registration returned null for email: {Email}", registerDto.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = "An unexpected error occurred during registration.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during registration for email: {Email}", registerDto.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Database Error",
                Detail = "An error occurred while saving to the database.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email: {Email}", registerDto.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during registration.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Authenticates a user with email and password, initiating two-factor authentication (2FA) with OTP.
    /// </summary>
    /// <param name="loginDto">User login credentials, including email and password.</param>
    /// <returns>An IActionResult with OTP token and identifier if successful, or an error message.</returns>
    /// <response code="200">Login successful, OTP sent to email.</response>
    /// <response code="400">Invalid login credentials.</response>
    /// <response code="403">Invalid credentials or user not found.</response>
    /// <response code="429">Too many login attempts.</response>
    /// <response code="500">Internal server error during login.</response>
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for login");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        _logger.LogInformation("User login attempt for email: {Email}", loginDto.Email);
        try
        {
            var response = await _authService.LoginAsync(loginDto);
            if (!response.IsSucceeded)
            {
                _logger.LogWarning("Login failed for email: {Email}. {Message}", loginDto.Email, response.Message);
                return response.StatusCode switch
                {
                    403 => StatusCode(403, new ProblemDetails
                    {
                        Title = "Authentication Failed",
                        Detail = response.Message,
                        Status = StatusCodes.Status403Forbidden
                    }),
                    429 => StatusCode(429, new ProblemDetails
                    {
                        Title = "Too Many Attempts",
                        Detail = response.Message,
                        Status = StatusCodes.Status429TooManyRequests
                    }),
                    _ => BadRequest(new ProblemDetails
                    {
                        Title = "Login Failed",
                        Detail = response.Message,
                        Status = StatusCodes.Status400BadRequest
                    })
                };
            }
            _logger.LogInformation("Login successful for email: {Email}, OTP sent", loginDto.Email);
            return Ok(new
            {
                response.Message,
                response.Model,
                response.IsSucceeded
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", loginDto.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during login.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }


    /// <summary>
    /// Validates a one-time password (OTP) to complete user authentication.
    /// </summary>
    /// <param name="dto">The OTP validation details, including OTP, token, and identifier.</param>
    /// <returns>An IActionResult with JWT token if successful, or an error message.</returns>
    /// <response code="200">OTP validated, JWT token issued.</response>
    /// <response code="400">Invalid OTP data.</response>
    /// <response code="403">Invalid or expired OTP or token.</response>
    /// <response code="429">Too many OTP validation attempts.</response>
    /// <response code="500">Internal server error during OTP validation.</response>
    [HttpPost("validate-otp")]
    [EnableRateLimiting("OtpPolicy")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for OTP validation");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("OTP validation attempt for identifier: {OtpIdentifier}", dto.OtpIdentifier);

            var response = await _authService.ValidateOtpAsync(dto);

            if (!response.IsSucceeded)
            {
                _logger.LogWarning("OTP validation failed for identifier {OtpIdentifier}: {Message}",
                    dto.OtpIdentifier, response.Message);

                return response.StatusCode switch
                {
                    403 => StatusCode(403, new ProblemDetails
                    {
                        Title = "OTP Validation Failed",
                        Detail = response.Message,
                        Status = StatusCodes.Status403Forbidden
                    }),
                    429 => StatusCode(429, new ProblemDetails
                    {
                        Title = "Too Many Attempts",
                        Detail = response.Message,
                        Status = StatusCodes.Status429TooManyRequests
                    }),
                    _ => BadRequest(new ProblemDetails
                    {
                        Title = "OTP Validation Failed",
                        Detail = response.Message,
                        Status = StatusCodes.Status400BadRequest
                    })
                };
            }
            _logger.LogInformation("OTP validated successfully for identifier: {OtpIdentifier}", dto.OtpIdentifier);
            return Ok(response.Model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OTP validation for identifier: {OtpIdentifier}", dto.OtpIdentifier);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during OTP validation.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Resends a one-time password (OTP) to the user's email.
    /// </summary>
    /// <param name="dto">The OTP resend request details, including the OTP identifier.</param>
    /// <returns>An IActionResult indicating success or failure.</returns>
    /// <response code="200">New OTP sent successfully.</response>
    /// <response code="400">Invalid OTP data.</response>
    /// <response code="404">User not found for the provided identifier.</response>
    /// <response code="429">Too many OTP resend requests.</response>
    /// <response code="500">Internal server error during OTP resend.</response>
    [HttpPost("resend-otp")]
    [EnableRateLimiting("ResendOtpPolicy")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for OTP resend");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("OTP resend attempt for identifier: {OtpIdentifier}", dto.OtpIdentifier);

            var response = await _authService.ResendOtpAsync(dto);

            return response.StatusCode switch
            {
                200 => Ok(new { message = response.Message, isSucceeded = response.IsSucceeded }),
                404 => NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = response.Message,
                    Status = StatusCodes.Status404NotFound
                }),
                429 => StatusCode(429, new ProblemDetails
                {
                    Title = "Too Many Requests",
                    Detail = response.Message,
                    Status = StatusCodes.Status429TooManyRequests
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "OTP Resend Failed",
                    Detail = response.Message,
                    Status = StatusCodes.Status400BadRequest
                })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OTP resend");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while resending OTP.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Sends a password reset link to the user's email.
    /// </summary>
    /// <param name="dto">The forgot password request details, including the user's email.</param>
    /// <returns>An IActionResult indicating whether the reset link was sent.</returns>
    /// <response code="200">Reset link sent successfully.</response>
    /// <response code="400">Invalid email format.</response>
    /// <response code="404">Email not found.</response>
    /// <response code="500">Internal server error during forgot password process.</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for forgot password");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("Forgot password request for email: {Email}", dto.Email);

            var response = await _authService.ForgotPasswordAsync(dto);

            if (!response.IsSucceeded)
            {
                _logger.LogWarning("Forgot password failed for email {Email}: {Message}", dto.Email, response.Message);

                return response.StatusCode switch
                {
                    404 => NotFound(new ProblemDetails
                    {
                        Title = "Email Not Found",
                        Detail = response.Message,
                        Status = StatusCodes.Status404NotFound
                    }),
                    _ => BadRequest(new ProblemDetails
                    {
                        Title = "Forgot Password Failed",
                        Detail = response.Message,
                        Status = StatusCodes.Status400BadRequest
                    })
                };
            }

            _logger.LogInformation("Password reset link sent to email: {Email}", dto.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during forgot password for email: {Email}", dto.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Resets the user's password using a provided token.
    /// </summary>
    /// <param name="dto">The reset password request details, including email, token, and new password.</param>
    /// <returns>An IActionResult indicating success or failure.</returns>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid token or password.</response>
    /// <response code="404">Email not found.</response>
    /// <response code="500">Internal server error during password reset.</response>
    [HttpPut("reset-password")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for reset password");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("Password reset attempt for email: {Email}", dto.Email);

            var response = await _authService.ResetPasswordAsync(dto);

            if (!response.IsSucceeded)
            {
                _logger.LogWarning("Password reset failed for email {Email}: {Message}", dto.Email, response.Message);

                return response.StatusCode switch
                {
                    404 => NotFound(new ProblemDetails
                    {
                        Title = "Email Not Found",
                        Detail = response.Message,
                        Status = StatusCodes.Status404NotFound
                    }),
                    _ => BadRequest(new ProblemDetails
                    {
                        Title = "Password Reset Failed",
                        Detail = response.Message,
                        Status = StatusCodes.Status400BadRequest
                    })
                };
            }

            _logger.LogInformation("Password reset successful for email: {Email}", dto.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset for email: {Email}", dto.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while resetting your password.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Retrieves the OAuth2 authorization URL for an external login provider.
    /// </summary>
    /// <param name="provider">The external authentication provider (e.g., Google, Microsoft, GitHub).</param>
    /// <returns>An IActionResult with the redirect URL for authentication.</returns>
    /// <response code="200">Redirect URL generated successfully.</response>
    /// <response code="400">Unsupported provider or invalid configuration.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("external-login-url")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetExternalLoginUrl(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Provider",
                Detail = "Provider is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("Generating external login URL for provider: {Provider}", provider);

            var authUrl = _authService.GetExternalLoginUrl(provider);

            _logger.LogInformation("External login URL generated successfully for provider: {Provider}", provider);
            return Ok(new { redirectUrl = authUrl });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Unsupported provider: {Provider}", provider);
            return BadRequest(new ProblemDetails
            {
                Title = "Unsupported Provider",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating external login URL for provider: {Provider}", provider);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while generating the login URL.",
                Status = StatusCodes.Status500InternalServerError
            });
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
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for external login");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("Processing external login for provider: {Provider}", model.Provider);

            var token = await _authService.ExchangeAuthCodeForTokenAsync(model);

            _logger.LogInformation("External login successful for provider: {Provider}", model.Provider);
            return Ok(new { Token = token });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid external login request for provider: {Provider}", model.Provider);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during external login for provider: {Provider}", model.Provider);
            return StatusCode(500, new ProblemDetails
            {
                Title = "External Service Error",
                Detail = "Failed to communicate with the external provider.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during external login for provider: {Provider}", model.Provider);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during external login.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Updates the company association for the authenticated user.
    /// </summary>
    /// <param name="dto">The company update details containing the new company ID.</param>
    /// <returns>An IActionResult with a new JWT token containing the updated company ID.</returns>
    /// <response code="200">Company updated successfully, new token issued.</response>
    /// <response code="400">Invalid request or company update failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User or company not found.</response>
    /// <response code="500">Internal server error during company update.</response>
    [HttpPost("update-company")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for update company");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID claim not found in token");
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "User ID claim is missing from the token.",
                Status = StatusCodes.Status401Unauthorized
            });
        }
        try
        {
            _logger.LogInformation("Updating company for user {UserId} to company {CompanyId}", userId, dto.CompanyId);

            var result = await _companyRequestService.UpdateUserCompanyAsync(userId, dto.CompanyId);

            if (!result.Success)
            {
                _logger.LogWarning("Company update failed for user {UserId}: {Message}", userId, result.Message);

                return BadRequest(new ProblemDetails
                {
                    Title = "Company Update Failed",
                    Detail = result.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            _logger.LogInformation("Company updated successfully for user {UserId}", userId);
            return Ok(new { token = result.Token });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Company or user not found for user {UserId}", userId);
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company for user {UserId}", userId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while updating the company.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Submits a request to create a new company for the user.
    /// </summary>
    /// <param name="dto">The company request details including full name, email, and company name.</param>
    /// <returns>An IActionResult indicating whether the request was submitted successfully.</returns>
    /// <response code="200">Company request submitted successfully.</response>
    /// <response code="400">Invalid request data or validation failed.</response>
    /// <response code="409">Conflict - company already exists or pending request.</response>
    /// <response code="500">Internal server error during request submission.</response>
    [HttpPost("request-company")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestCompany([FromBody] CreateCompanyRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for company request");
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("Processing company request for email: {Email}, company: {CompanyName}",
               dto.Email, dto.CompanyName);

            var result = await _companyRequestService.CreateRequestAsync(dto);

            if (!result.Success)
            {
                _logger.LogWarning("Company request failed for {Email}: {Message}", dto.Email, result.Message);

                return result.Message.Contains("already exists")
                    ? Conflict(new ProblemDetails
                    {
                        Title = "Conflict",
                        Detail = result.Message,
                        Status = StatusCodes.Status409Conflict
                    })
                    : BadRequest(new ProblemDetails
                    {
                        Title = "Company Request Failed",
                        Detail = result.Message,
                        Status = StatusCodes.Status400BadRequest
                    });
            }

            _logger.LogInformation("Company request submitted successfully for {Email}, RequestId: {RequestId}",
                dto.Email, result.RequestId);

            return Ok(new
            {
                message = result.Message,
                requestId = result.RequestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing company request for {Email}", dto.Email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Retrieves the status of all company requests for a specific email.
    /// </summary>
    /// <param name="email">The email address to check request status for.</param>
    /// <returns>A list of company requests with their current status.</returns>
    /// <response code="200">Request statuses retrieved successfully.</response>
    /// <response code="400">Email parameter is missing or invalid.</response>
    /// <response code="500">Internal server error during status retrieval.</response>
    [HttpGet("request-status")]
    [ProducesResponseType(typeof(List<RequestStatusResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestStatus([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "Email is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        if (!IsValidEmail(email))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "A valid email address is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        try
        {
            _logger.LogInformation("Retrieving company request status for email: {Email}", email);

            var requests = await _companyRequestService.GetRequestStatusAsync(email);

            _logger.LogInformation("Retrieved {Count} company requests for email: {Email}", requests.Count, email);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company request status for email: {Email}", email);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while retrieving request status.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #region Private Methods

    /// <summary>
    /// Validates an email address format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email format is valid, otherwise false.</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}