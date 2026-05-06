using Core_API.Domain.Common;
using Core_API.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Tasks
{

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
}
