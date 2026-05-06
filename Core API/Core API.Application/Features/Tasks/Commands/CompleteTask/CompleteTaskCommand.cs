using Core_API.Application.Common.Base;

namespace Core_API.Application.Features.Tasks.Commands.CompleteTask
{
    /// <summary>
    /// Command to mark a task as completed
    /// </summary>
    public record CompleteTaskCommand : BaseCommand<bool>
    {
        public int TaskId { get; init; }
    }
}