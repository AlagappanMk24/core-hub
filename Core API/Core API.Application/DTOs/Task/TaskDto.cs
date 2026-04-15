using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Application.DTOs.Tasks
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public string PriorityName => Priority.ToString();
        public TaskStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }

        // User assignments
        public string? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }

        // Task hierarchy
        public int? ParentTaskId { get; set; }
        public string? ParentTaskTitle { get; set; }
        public int SubtaskCount { get; set; }

        // Task metadata
        public DateTime? ReminderDate { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }

        // Statistics
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Collections
        public List<TaskCommentDto>? Comments { get; set; }
        public List<TaskAttachmentDto>? Attachments { get; set; }
        public List<TaskDto>? Subtasks { get; set; }
    }
    public class CreateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? Tag { get; set; }

        public string? AssignedToUserId { get; set; }

        public DateTime? ReminderDate { get; set; }

        public bool IsRecurring { get; set; }

        [StringLength(50)]
        public string? RecurrencePattern { get; set; }

        public decimal? EstimatedHours { get; set; }

        public int? ParentTaskId { get; set; }
    }

    public class UpdateTaskDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskPriority? Priority { get; set; }

        public TaskStatus? Status { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? Tag { get; set; }

        public string? AssignedToUserId { get; set; }

        public DateTime? ReminderDate { get; set; }

        public bool? IsRecurring { get; set; }

        [StringLength(50)]
        public string? RecurrencePattern { get; set; }

        public decimal? EstimatedHours { get; set; }

        public decimal? ActualHours { get; set; }

        public int? ParentTaskId { get; set; }
    }

    public class TaskCommentDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Comment { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTaskCommentDto
    {
        [Required]
        [StringLength(2000)]
        public string Comment { get; set; }
    }

    public class TaskAttachmentDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string FileName { get; set; }
        public string? FileUrl { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TaskFilterDto
    {
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public bool? Overdue { get; set; }
        public bool? MyTasks { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "DueDate";
        public bool SortDescending { get; set; } = false;
    }

    public class TaskStatsDto
    {
        public int TotalTasks { get; set; }
        public int MyTasks { get; set; }
        public int TasksAssignedToMe { get; set; }
        public int TasksCreatedByMe { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CancelledTasks { get; set; }
        public int OnHoldTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        public int UrgentPriorityTasks { get; set; }
        public int TasksDueToday { get; set; }
        public int TasksDueThisWeek { get; set; }
        public int TasksDueThisMonth { get; set; }
        public Dictionary<string, int> TasksByCategory { get; set; } = new();
        public Dictionary<string, int> TasksByStatus { get; set; } = new();
        public Dictionary<string, int> TasksByPriority { get; set; } = new();
    }
}
