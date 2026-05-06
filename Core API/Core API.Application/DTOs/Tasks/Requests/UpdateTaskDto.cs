using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Tasks.Requests
{
    public class UpdateTaskDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskPriority? Priority { get; set; }

        public Domain.Enums.TaskStatus? Status { get; set; }

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
}
