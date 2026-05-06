using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Application.DTOs.Tasks.Responses
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
}