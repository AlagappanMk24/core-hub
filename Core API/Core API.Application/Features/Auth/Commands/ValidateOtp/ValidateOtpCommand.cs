using Core_API.Application.DTOs.Authentication.Responses;
using Core_API.Application.DTOs.Common;
using MediatR;

namespace Core_API.Application.Features.Auth.Commands.ValidateOtp
{
    /// <summary>
    /// Command to validate OTP and complete user authentication
    /// </summary>
    public class ValidateOtpCommand : IRequest<ApiResponse<LoginResponseDto>>
    {
        public string Otp { get; set; } = string.Empty;
        public string OtpToken { get; set; } = string.Empty;
        public string OtpIdentifier { get; set; } = string.Empty;
    }
}