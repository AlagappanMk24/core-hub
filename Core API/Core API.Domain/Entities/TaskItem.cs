using Core_API.Domain.Entities.Common;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Domain.Entities
{
    /// <summary>
    /// Represents a task or to-do item in the system
    /// </summary>
    public class TaskItem : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? Tag { get; set; }

        // User relationships
        public string? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public ApplicationUser? AssignedToUser { get; set; }

        public string? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public ApplicationUser? CreatedByUser { get; set; }

        // Task hierarchy
        public int? ParentTaskId { get; set; }

        [ForeignKey("ParentTaskId")]
        public TaskItem? ParentTask { get; set; }

        public ICollection<TaskItem> Subtasks { get; set; } = new List<TaskItem>();

        // Reminder
        public DateTime? ReminderDate { get; set; }

        // Recurring tasks
        public bool IsRecurring { get; set; }

        [StringLength(50)]
        public string? RecurrencePattern { get; set; } // "Daily", "Weekly", "Monthly", "Quarterly", "Yearly"

        // Time tracking
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualHours { get; set; }

        // Collections
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();

        public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();

        public ICollection<TaskAuditLog> AuditLogs { get; set; } = new List<TaskAuditLog>();
    }

    /// <summary>
    /// Represents a comment on a task
    /// </summary>
    public class TaskComment : BaseEntity
    {
        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public TaskItem Task { get; set; }

        [Required]
        [StringLength(2000)]
        public string Comment { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }

    /// <summary>
    /// Represents an attachment for a task
    /// </summary>
    public class TaskAttachment : BaseEntity
    {
        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public TaskItem Task { get; set; }

        [Required]
        [StringLength(500)]
        public string FileName { get; set; }

        [StringLength(1000)]
        public string? FilePath { get; set; }

        [StringLength(500)]
        public string? FileUrl { get; set; }

        public long FileSize { get; set; }

        [StringLength(100)]
        public string? ContentType { get; set; }
    }

    /// <summary>
    /// Represents audit log for task changes
    /// </summary>
    public class TaskAuditLog : BaseEntity
    {
        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public TaskItem Task { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(2000)]
        public string? Changes { get; set; }

        public string? PerformedByUserId { get; set; }

        [ForeignKey("PerformedByUserId")]
        public ApplicationUser? PerformedByUser { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }
    }
}