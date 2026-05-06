using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Features.Auth.Commands.ExternalLogin
{
    public class ExternalLoginCommand : IRequest<string>
    {
        [Required(ErrorMessage = "Provider is required")]
        public string Provider { get; set; } = string.Empty;

        [Required(ErrorMessage = "Authorization code is required")]
        public string AuthorizationCode { get; set; } = string.Empty;
    }
}