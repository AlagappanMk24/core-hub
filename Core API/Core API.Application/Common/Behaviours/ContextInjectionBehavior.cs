using MediatR;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Base;
using Core_API.Application.Contracts.Services.Common;

namespace Core_API.Application.Common.Behaviours;

/// <summary>
/// Pipeline behavior that automatically injects the OperationContext into requests
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ContextInjectionBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    ILogger<ContextInjectionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest<TResponse>
    where TResponse : notnull
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ILogger<ContextInjectionBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get current user context
        var context = _currentUserService.GetCurrentContext();

        // Inject context if not already set
        if (request.Context == null)
        {
            request.Context = context;
            _logger.LogDebug("Injected context into {RequestType} for user {UserId}",
                typeof(TRequest).Name, context.UserId);
        }

        return await next(cancellationToken);
    }
}