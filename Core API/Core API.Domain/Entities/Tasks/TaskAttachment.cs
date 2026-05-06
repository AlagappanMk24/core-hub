using Core_API.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Tasks
{
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
}