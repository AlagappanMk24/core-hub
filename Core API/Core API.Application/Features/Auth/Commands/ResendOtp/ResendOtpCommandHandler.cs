using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.DTOs.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Auth.Commands.ResendOtp
{
    /// <summary>
    /// Handles the resend OTP command.
    /// </summary>
    public class ResendOtpCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailServiceProvider emailServiceProvider,
        ILogger<ResendOtpCommandHandler> logger)
        : IRequestHandler<ResendOtpCommand, ResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailServiceProvider _emailServiceProvider = emailServiceProvider;
        private readonly ILogger<ResendOtpCommandHandler> _logger = logger;

        public async Task<ResponseDto> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.AuthUsers.FindByOtpIdentifierAsync(request.OtpIdentifier);

                if (user == null)
                {
                    _logger.LogWarning("Resend OTP failed: Invalid identifier {OtpIdentifier}", request.OtpIdentifier);
                    return ResponseDto.NotFound("User not found with the provided OTP identifier.");
                }

                // Generate new OTP
                var newOtp = GenerateOtp();
                user.TwoFactorCode = newOtp;
                user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(5);

                await _unitOfWork.AuthUsers.UpdateAsync(user);

                // Send email
                await _emailServiceProvider.SendOtpEmailAsync(user.Email, newOtp);

                _logger.LogInformation("New OTP sent successfully for identifier: {OtpIdentifier}", request.OtpIdentifier);

                return ResponseDto.Success("A new OTP has been sent to your email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OTP resend for identifier {OtpIdentifier}", request.OtpIdentifier);

                return ResponseDto.Failure(
                    "An internal server error occurred while resending OTP.",
                    StatusCodes.Status500InternalServerError);
            }
        }
        private static string GenerateOtp()
            => Random.Shared.Next(100000, 999999).ToString();   // Better than new Random()
    }
}