using Core_API.Application.Common.Base;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Features.Tasks.Commands.DeleteTask
{
    /// <summary>
    /// Command to delete (soft delete) a task
    /// </summary>
    public record DeleteTaskCommand : BaseCommand<bool>
    {
        public int TaskId { get; init; }
    }
}