using Core_API.Application.DTOs.Common;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<ResponseDto>
    {
        [Required(ErrorMessage = "The email field is required.")]
        [EmailAddress(ErrorMessage = "The email field is not a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}