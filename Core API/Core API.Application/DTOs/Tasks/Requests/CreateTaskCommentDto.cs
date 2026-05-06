using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Tasks.Requests
{
    public class CreateTaskCommentDto
    {
        [Required]
        [StringLength(2000)]
        public string Comment { get; set; }
    }
}