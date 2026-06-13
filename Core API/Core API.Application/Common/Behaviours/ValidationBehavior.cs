using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;

namespace Core_API.Application.Common.Behaviours;

/// <summary>
/// Pipeline behavior for automatic validation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest<TResponse>
    where TResponse : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join(", ", failures.Select(f => f.ErrorMessage));
            _logger.LogWarning("Validation failed for {RequestType}: {Errors}",
                typeof(TRequest).Name, errors);

            // Return failure response without executing the handler
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition() == typeof(OperationResult<>))
            {
                var resultType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(OperationResult<>)
                    .MakeGenericType(resultType)
                    .GetMethod(nameof(OperationResult<object>.FailureResult),
                        new[] { typeof(string) });

                return (TResponse)failureMethod?.Invoke(null, new object[] { errors })!;
            }
        }

        return await next();
    }
}