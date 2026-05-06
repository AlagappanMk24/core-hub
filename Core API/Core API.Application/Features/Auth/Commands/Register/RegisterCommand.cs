using Core_API.Application.DTOs.Common;
using MediatR;

namespace Core_API.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<ResponseDto>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
        public int? CompanyId { get; set; }
    }
}