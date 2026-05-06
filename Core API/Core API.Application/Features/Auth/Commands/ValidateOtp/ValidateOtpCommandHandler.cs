using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.DTOs.Authentication.Responses;
using Core_API.Application.DTOs.Common;
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
        ILogger<ValidateOtpCommandHandler> logger)
        : IRequestHandler<ValidateOtpCommand, ResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IJwtService _jwtService = jwtService;
        private readonly ILogger<ValidateOtpCommandHandler> _logger = logger;

        public async Task<ResponseDto> Handle(ValidateOtpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("OTP validation attempt for identifier: {OtpIdentifier}", request.OtpIdentifier);

                // Step 1: Validate OTP Token
                var principal = _jwtService.ValidateOtpToken(request.OtpToken);
                if (principal == null)
                {
                    _logger.LogWarning("OTP validation failed: Invalid or expired OTP token for {OtpIdentifier}",
                        request.OtpIdentifier);
                    return ResponseDto.Failure("Invalid or expired OTP token.", StatusCodes.Status403Forbidden);
                }

                // Step 2: Find user by OTP Identifier
                var user = await _unitOfWork.AuthUsers.FindByOtpIdentifierAsync(request.OtpIdentifier);
                if (user is null)
                {
                    _logger.LogWarning("OTP validation failed: Invalid identifier {OtpIdentifier}", request.OtpIdentifier);
                    return ResponseDto.Failure("Invalid OTP identifier.", StatusCodes.Status403Forbidden);
                }

                // Step 3: Verify email from token matches user
                var emailFromToken = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (emailFromToken != user.Email)
                {
                    _logger.LogWarning("OTP validation failed: Token email mismatch for {OtpIdentifier}", request.OtpIdentifier);
                    return ResponseDto.Failure("Invalid OTP token.", StatusCodes.Status403Forbidden);
                }

                // Step 4: Verify OTP code
                if (user.TwoFactorCode != request.Otp)
                {
                    _logger.LogWarning("OTP validation failed: Incorrect OTP for {OtpIdentifier}", request.OtpIdentifier);
                    return ResponseDto.Failure("Invalid OTP.", StatusCodes.Status403Forbidden);
                }

                // Step 5: Check OTP expiry
                if (user.TwoFactorExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("OTP validation failed: Expired OTP for {OtpIdentifier}", request.OtpIdentifier);
                    return ResponseDto.Failure("OTP has expired.", StatusCodes.Status403Forbidden);
                }

                // Step 6: Clear OTP after successful validation
                user.TwoFactorCode = null;
                user.TwoFactorExpiry = null;
                await _unitOfWork.AuthUsers.UpdateAsync(user);

                // Step 7: Generate JWT and Refresh Token
                var jwtToken = await _jwtService.GenerateJwtToken(user);
                var refreshToken = _jwtService.CreateRefreshToken();

                // Store refresh token
                user.RefreshTokens.Add(refreshToken);
                await _unitOfWork.AuthUsers.UpdateAsync(user);

                _logger.LogInformation("OTP validated successfully for identifier: {OtpIdentifier}", request.OtpIdentifier);

                var loginResponse = new LoginResponseDto
                {
                    IsAuthenticated = true,
                    Token = jwtToken,
                    UserName = user.UserName ?? user.Email,
                    Email = user.Email,
                    Message = "Login successful",
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiration = refreshToken.ExpiresOn
                };

                return ResponseDto.Success(
                    message: "OTP validated successfully. Login completed.",
                    data: loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OTP validation for identifier: {OtpIdentifier}",
                    request.OtpIdentifier);

                return ResponseDto.Failure(
                    "An internal server error occurred during OTP validation.",
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}