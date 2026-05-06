using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.DTOs.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Core_API.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailServiceProvider emailServiceProvider,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, ResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailServiceProvider _emailServiceProvider = emailServiceProvider;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

        public async Task<ResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing forgot password request for email: {Email}", request.Email);

                // Find user by email
                var user = await _unitOfWork.AuthUsers.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new ResponseDto
                    {
                        StatusCode = 404,
                        IsSucceeded = false,
                        Message = "Invalid Email"
                    };
                }

                // Generate password reset token
                var token = await _unitOfWork.AuthUsers.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);

                // Build reset link (configurable from app settings)
                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
                var resetLink = $"{frontendUrl}/auth/reset-password?email={request.Email}&token={encodedToken}";

                // Send reset link via email
                await _emailServiceProvider.SendResetPasswordEmailAsync(
                       request.Email,
                       "Reset Password",
                        $@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                    <h2 style='color: #8A2BE2;'>Reset Your Password</h2>
                                    <p>Hello {user.FullName ?? user.UserName},</p>
                                    <p>We received a request to reset your password. Click the button below to create a new password:</p>
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <a href='{resetLink}' 
                                           style='background-color: #8A2BE2; color: white; padding: 12px 24px; 
                                                  text-decoration: none; border-radius: 5px; display: inline-block;'>
                                            Reset Password
                                        </a>
                                    </div>
                                    <p>If you didn't request this, please ignore this email. This link will expire in 24 hours.</p>
                                    <p>Or copy this link: <a href='{resetLink}'>{resetLink}</a></p>
                                    <hr style='margin: 20px 0;' />
                                    <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
                                </div>
                            </body>
                            </html>");

                _logger.LogInformation("Password reset link sent to email: {Email}", request.Email);

                return new ResponseDto
                {
                    StatusCode = 200,
                    IsSucceeded = true,
                    Message = "Reset password link sent successfully."
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", request.Email);
                return new ResponseDto
                {
                    StatusCode = 500,
                    IsSucceeded = false ,
                    Message = "An error occurred while processing your request"
                };
            }
        }
    }
}