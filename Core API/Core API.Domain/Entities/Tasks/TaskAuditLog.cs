using Core_API.Domain.Common;
using Core_API.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Tasks
{
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