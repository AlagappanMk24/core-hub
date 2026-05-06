using Core_API.Application.Contracts.Services.Auth;
using Core_API.Application.DTOs.Authentication.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Auth.Queries.GetExternalLoginUrl
{
    /// <summary>
    /// Handles generation of OAuth2 authorization URL for external providers using configuration-driven approach.
    /// </summary>
    public class GetExternalLoginUrlQueryHandler(
        IExternalAuthUrlBuilder externalAuthUrlBuilder,  
        ILogger<GetExternalLoginUrlQueryHandler> logger)
        : IRequestHandler<GetExternalLoginUrlQuery, GetExternalLoginUrlResponse>
    {
        private readonly IExternalAuthUrlBuilder _externalAuthUrlBuilder = externalAuthUrlBuilder;
        private readonly ILogger<GetExternalLoginUrlQueryHandler> _logger = logger;

        public async Task<GetExternalLoginUrlResponse> Handle(
            GetExternalLoginUrlQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating external login URL for provider: {Provider}", request.Provider);

            try
            {
                var redirectUrl = await _externalAuthUrlBuilder.BuildAuthorizationUrlAsync(request.Provider);

                _logger.LogInformation("External login URL generated successfully for provider: {Provider}", request.Provider);

                return new GetExternalLoginUrlResponse
                {
                    RedirectUrl = redirectUrl,
                    Provider = request.Provider.ToLowerInvariant()
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Unsupported provider requested: {Provider}", request.Provider);
                throw; // Let global exception handler or controller handle it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate external login URL for provider: {Provider}", request.Provider);
                throw;
            }
        }
    }
}