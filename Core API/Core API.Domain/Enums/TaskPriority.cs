// Core_API.Domain/Enums/TaskEnums.cs
namespace Core_API.Domain.Enums
{
    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }

    public enum TaskStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3,
        OnHold = 4,
        Overdue = 5
    }
}