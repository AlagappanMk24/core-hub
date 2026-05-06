using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Core_API.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger) : IRequestHandler<ResetPasswordCommand,ResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<ResetPasswordCommandHandler> _logger = logger;

        public async Task<ResponseDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing password reset for email: {Email}", request.Email);

                // Find user by email
                var user = await _unitOfWork.AuthUsers.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.LogWarning("Reset password failed: User not found for email {Email}", request.Email);
                    return new ResponseDto
                    {
                        StatusCode = 404,
                        IsSucceeded = false,
                        Message = "Email is incorrect!"
                    };
                }
                // Decode token
                var decodedToken = WebUtility.UrlDecode(request.Token);

                // Reset password
                var result = await _unitOfWork.AuthUsers.ResetPasswordAsync(user, decodedToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    return new ResponseDto
                    {
                        StatusCode = 400,
                        IsSucceeded = false,
                        Message = "Failed to reset password, try again."
                    };
                }

                // Clear any existing OTPs
                user.TwoFactorCode = null;
                user.TwoFactorExpiry = null;
                await _unitOfWork.AuthUsers.UpdateAsync(user);

                _logger.LogInformation("Password reset successfully for email: {Email}", request.Email);

                return new ResponseDto
                {
                    StatusCode = 200,
                    IsSucceeded = true,
                    Message = "Your password has been reset successfully. You can now log in with your new password.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
                return new ResponseDto
                {
                    StatusCode = 500,
                    IsSucceeded = false,
                    Message = "An error occurred while resetting your password"
                };
            }
        }
    }
}
