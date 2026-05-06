using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace Core_API.Application.Common.Base
{
    /// <summary>
    /// Base class for all commands
    /// </summary>
    public abstract record BaseCommand<TResponse> : IRequest<OperationResult<TResponse>>
    {
        // This property should NOT be in the JSON request
        // It will be set by the pipeline behavior
        [JsonIgnore]
        public OperationContext Context { get; init; }
    }
}