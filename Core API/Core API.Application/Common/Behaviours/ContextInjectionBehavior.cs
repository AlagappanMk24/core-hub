using Core_API.Application.Common.Base;
using Core_API.Application.Contracts.Services.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Common.Behaviours
{
    /// <summary>
    /// Pipeline behavior that automatically injects the OperationContext into commands and queries
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class ContextInjectionBehavior<TRequest, TResponse>(
        ICurrentUserService currentUserService,
        ILogger<ContextInjectionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly ILogger<ContextInjectionBehavior<TRequest, TResponse>> _logger = logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Check if the request is a BaseCommand or BaseQuery
            if (request is BaseCommand<object> baseCommand)
            {
                var context = _currentUserService.GetCurrentContext();

                // Use reflection to set the Context property
                var property = typeof(BaseCommand<object>).GetProperty("Context");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(baseCommand, context);
                    _logger.LogDebug("Injected context into command {CommandType} for user {UserId}",
                        typeof(TRequest).Name, context.UserId);
                }
            }
            else if (request is BaseQuery<object> baseQuery)
            {
                var context = _currentUserService.GetCurrentContext();

                var property = typeof(BaseQuery<object>).GetProperty("Context");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(baseQuery, context);
                    _logger.LogDebug("Injected context into query {QueryType} for user {UserId}",
                        typeof(TRequest).Name, context.UserId);
                }
            }

            return await next();
        }
    }
}