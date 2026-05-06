using Core_API.Application.DTOs.Authentication.Responses;
using MediatR;

namespace Core_API.Application.Features.Auth.Queries.GetExternalLoginUrl
{
    /// <summary>
    /// Query to generate OAuth2 authorization URL for external login providers.
    /// </summary>
    public class GetExternalLoginUrlQuery : IRequest<GetExternalLoginUrlResponse>
    {
        public string Provider { get; set; } = string.Empty;
    }
}