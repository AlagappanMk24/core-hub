using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.DTOs.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IEmailServiceProvider emailServiceProvider,
        ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, ResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IEmailServiceProvider _emailServiceProvider = emailServiceProvider;
        private readonly ILogger<LoginCommandHandler> _logger = logger;

        public async Task<ResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                // Find user by email
                var user = await _unitOfWork.AuthUsers.FindByEmailAsync(request.Email);
                if (user is null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                    return ResponseDto.Failure("Invalid email or password.", StatusCodes.Status403Forbidden);
                }

                // Verify password
                var isPasswordValid = await _unitOfWork.AuthUsers.CheckPasswordAsync(user, request.Password);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid credentials for email {Email}", request.Email);
                    return ResponseDto.Failure("Invalid email or password.", StatusCodes.Status403Forbidden);
                }

                // Generate OTP and unique identifier
                var otp = GenerateOtp();
                var otpIdentifier = Guid.NewGuid().ToString();

                // Update user with OTP details
                user.TwoFactorCode = otp;
                user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);
                user.OtpIdentifier = otpIdentifier;

                await _unitOfWork.AuthUsers.UpdateAsync(user);

                // WRITE OPERATION - Generate OTP token
                var otpToken = _jwtService.GenerateOtpToken(user);

                // Side effect - Send email
                await _emailServiceProvider.SendOtpEmailAsync(user.Email, otp);

                _logger.LogInformation("Login successful. OTP sent to email: {Email}", request.Email);

                return ResponseDto.Success(
                     message: "OTP has been sent to your email. Please verify to continue.",
                     data: new
                     {
                         OtpToken = otpToken,
                         OtpIdentifier = otpIdentifier
                     });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
                return ResponseDto.Failure(
                    "An internal server error occurred. Please try again later.",
                    StatusCodes.Status500InternalServerError);
            }
        }
        private static string GenerateOtp()
                   => Random.Shared.Next(100000, 999999).ToString();
    }
}