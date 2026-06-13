using System.Text.Json.Serialization;
using Core_API.Application.Common.Models;

namespace Core_API.Application.Common.Base;

/// <summary>
/// Abstract base class for all queries
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public abstract record BaseQuery<TResponse> : IBaseRequest<TResponse>
{
    [JsonIgnore]
    public OperationContext Context { get; set; } = null!;
}