using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using MediatR;

namespace Core_API.Application.Common.Base;

/// <summary>
/// Base interface for all application requests (Commands & Queries)
/// </summary>
public interface IBaseRequest<TResponse> : IRequest<OperationResult<TResponse>>
{
    OperationContext Context { get; set; }
}