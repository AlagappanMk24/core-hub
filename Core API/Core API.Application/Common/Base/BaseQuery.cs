using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using MediatR;
using System.Text.Json.Serialization;

namespace Core_API.Application.Common.Base
{
    /// <summary>
    /// Base class for all queries
    /// </summary>
    public abstract record BaseQuery<TResponse> : IRequest<OperationResult<TResponse>>
    {
        [JsonIgnore]
        public OperationContext Context { get; init; }
    }
}