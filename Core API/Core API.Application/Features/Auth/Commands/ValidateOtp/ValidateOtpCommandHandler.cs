using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.DTOs.Authentication.Responses;
using Core_API.Application.DTOs.Common;
using Core_API.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Core_API.Application.Features.Auth.Commands.ValidateOtp
{
    /// <summary>
    /// Handles OTP validation and issues JWT + Refresh Token upon successful validation.
    /// </summary>
    public class ValidateOtpCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IOtpAttemptTracker otpTracker,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ValidateOtpCommandHandler> logger)
        : IRequestHandler<ValidateOtpCommand, ApiResponse<LoginResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IOtpAttemptTracker _otpTracker = otpTracker;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<ValidateOtpCommandHandler> _logger = logger;

        public async Task<ApiResponse<LoginResponseDto>> Handle(ValidateOtpCommand request, CancellationToken cancellationToken)
        {
            var clientIp = GetClientIpAddress();

            _logger.LogInformation("OTP validation attempt for identifier: {OtpIdentifier} from IP: {ClientIp}",
                request.OtpIdentifier, clientIp);

            try
            {
                // Step 1: Validate OTP Token
                var principal = _jwtService.ValidateOtpToken(request.OtpToken);
                if (principal == null)
                {
                    _logger.LogWarning("OTP validation failed: Invalid or expired OTP token for {OtpIdentifier}", request.OtpIdentifier);
                    return ApiResponse<LoginResponseDto>.Error(
                        "Your verification session has expired. Please request a new OTP.",
                        403,
                        new ErrorDetails
                        {
                            Code = "OTP_TOKEN_EXPIRED",
                            Description = "The OTP verification token has expired",
                            Metadata = new OtpErrorDetails
                            {
                                ErrorType = "TokenExpired",
                                RemainingAttempts = 0
                            }
                        });
                }

                // Step 2: Find user by OTP Identifier
                var user = await _unitOfWork.AuthUsers.FindByOtpIdentifierAsync(request.OtpIdentifier);
                if (user is null)
                {
                    _logger.LogWarning("OTP validation failed: Invalid identifier {OtpIdentifier}", request.OtpIdentifier);
                    return ApiResponse<LoginResponseDto>.Error(
                        "Invalid verification request. Please request a new OTP.",
                        400,
                        new ErrorDetails
                        {
                            Code = "INVALID_IDENTIFIER",
                            Description = "The OTP identifier is invalid"
                        });
                }

                // Step 3: Check if user is locked out
                if (user.IsOtpLocked)
                {
                    var secondsRemaining = (int)(user.OtpLockoutEnd!.Value - DateTime.UtcNow).TotalSeconds;
                    _logger.LogWarning("OTP validation blocked: User {UserId} is locked out for {SecondsRemaining} seconds",
                        user.Id, secondsRemaining);

                    return ApiResponse<LoginResponseDto>.Error(
                        "Too many failed attempts. Your account is temporarily locked.",
                        429,
                        new ErrorDetails
                        {
                            Code = "ACCOUNT_LOCKED",
                            Description = "Account temporarily locked due to too many failed attempts",
                            Metadata = new OtpErrorDetails
                            {
                                IsLocked = true,
                                LockoutSecondsRemaining = secondsRemaining,
                                LockoutEnd = user.OtpLockoutEnd,
                                ErrorType = "Locked",
                                RemainingAttempts = 0
                            }
                        });
                }

                // Step 4: Verify email from token matches user
                var emailFromToken = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (emailFromToken != user.Email)
                {
                    _logger.LogWarning("OTP validation failed: Token email mismatch for {OtpIdentifier}", request.OtpIdentifier);
                    await RegisterFailedAttempt(user, clientIp);
                    var status = await GetOtpStatus(user);

                    return ApiResponse<LoginResponseDto>.Error(
                        "The verification code doesn't match our records. Please check and try again.",
                        403,
                        new ErrorDetails
                        {
                            Code = "OTP_MISMATCH",
                            Description = "OTP code does not match",
                            Metadata = new OtpErrorDetails
                            {
                                RemainingAttempts = status.RemainingAttempts,
                                ErrorType = "Mismatch",
                                IsLocked = false
                            }
                        });
                }

                // Step 5: Verify OTP code
                if (user.TwoFactorCode != request.Otp)
                {
                    var attemptResult = await RegisterFailedAttempt(user, clientIp);

                    if (attemptResult.IsLocked)
                    {
                        return ApiResponse<LoginResponseDto>.Error(
                            attemptResult.ErrorMessage ?? "Too many failed attempts. Please try again later.",
                            429,
                            new ErrorDetails
                            {
                                Code = "MAX_ATTEMPTS_EXCEEDED",
                                Description = "Maximum OTP attempts exceeded",
                                Metadata = new OtpErrorDetails
                                {
                                    IsLocked = true,
                                    LockoutSecondsRemaining = attemptResult.LockoutSecondsRemaining,
                                    LockoutEnd = attemptResult.LockoutEnd,
                                    ErrorType = "Locked",
                                    RemainingAttempts = 0
                                }
                            });
                    }

                    _logger.LogWarning("OTP validation failed: Incorrect OTP for user {UserId}. Remaining attempts: {RemainingAttempts}",
                        user.Id, attemptResult.RemainingAttempts);

                    return ApiResponse<LoginResponseDto>.Error(
                        $"The verification code is incorrect. {attemptResult.RemainingAttempts} attempt(s) remaining.",
                        403,
                        new ErrorDetails
                        {
                            Code = "INVALID_OTP",
                            Description = "The provided OTP code is incorrect",
                            Metadata = new OtpErrorDetails
                            {
                                RemainingAttempts = attemptResult.RemainingAttempts,
                                ProgressiveDelaySeconds = attemptResult.ProgressiveDelaySeconds,
                                ErrorType = "InvalidOtp",
                                IsLocked = false
                            }
                        });
                }

                // Step 6: Check OTP expiry
                if (user.TwoFactorExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP validation failed: Expired OTP for user {UserId}", user.Id);
                    await RegisterFailedAttempt(user, clientIp);
                    var status = await GetOtpStatus(user);

                    return ApiResponse<LoginResponseDto>.Error(
                        "Your verification code has expired. Please request a new OTP code.",
                        403,
                        new ErrorDetails
                        {
                            Code = "OTP_EXPIRED",
                            Description = "The OTP code has expired",
                            Metadata = new OtpErrorDetails
                            {
                                RemainingAttempts = status.RemainingAttempts,
                                ErrorType = "Expired",
                                IsLocked = false
                            }
                        });
                }

                // Step 7: Successful validation - Reset all counters
                _otpTracker.ResetAttempts(user);
                user.TwoFactorCode = null;
                user.TwoFactorExpiry = null;
                user.OtpIdentifier = null;
                user.LastLogin = DateTime.UtcNow;

                await _unitOfWork.AuthUsers.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Step 8: Generate JWT and Refresh Token
                var jwtToken = await _jwtService.GenerateJwtTokenAsync(user);
                var refreshToken = _jwtService.CreateRefreshToken();

                user.RefreshTokens.Add(refreshToken);
                await _unitOfWork.AuthUsers.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("OTP validated successfully for identifier: {OtpIdentifier}", request.OtpIdentifier);

                var loginResponse = new LoginResponseDto
                {
                    IsAuthenticated = true,
                    Token = jwtToken,
                    UserName = user.UserName ?? user.Email,
                    Email = user.Email,
                    Message = "Verification successful. Redirecting to dashboard...",
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiration = refreshToken.ExpiresOn
                };

                return ApiResponse<LoginResponseDto>.Ok(loginResponse, "Verification successful. Redirecting to dashboard...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OTP validation for identifier: {OtpIdentifier}",
                    request.OtpIdentifier);

                return ApiResponse<LoginResponseDto>.Error(
                    "Unable to verify your code at this time. Please try again later.",
                    500,
                    new ErrorDetails
                    {
                        Code = "INTERNAL_ERROR",
                        Description = "An unexpected error occurred during OTP validation"
                    });
            }
        }

        #region Private Methods

        private async Task<OtpAttemptResult> RegisterFailedAttempt(ApplicationUser user, string clientIp)
        {
            var result = _otpTracker.RegisterFailedAttempt(user, clientIp);
            await _unitOfWork.AuthUsers.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        private string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            var forwardedIp = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedIp))
            {
                return forwardedIp.Split(',').First().Trim();
            }

            return context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private async Task<OtpStatus> GetOtpStatus(ApplicationUser user)
        {
            return _otpTracker.GetOtpStatus(user);
        }

        #endregion
    }
}