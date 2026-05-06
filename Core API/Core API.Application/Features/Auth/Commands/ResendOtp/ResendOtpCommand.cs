using Core_API.Application.DTOs.Common;
using MediatR;

namespace Core_API.Application.Features.Auth.Commands.ResendOtp
{
    /// <summary>
    /// Command to resend OTP to user's email.
    /// </summary>
    public class ResendOtpCommand : IRequest<ResponseDto>
    {
        public string OtpIdentifier { get; set; } = string.Empty;
    }
}