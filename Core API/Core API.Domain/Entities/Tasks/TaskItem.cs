using Core_API.Domain.Common;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Domain.Entities.Tasks
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
}