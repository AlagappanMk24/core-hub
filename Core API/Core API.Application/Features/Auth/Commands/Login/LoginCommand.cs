using Core_API.Application.DTOs.Common;
using MediatR;

namespace Core_API.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Command for user login with email and password (initiates 2FA with OTP)
    /// </summary>
    public class LoginCommand : IRequest<ResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}