using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class ContactUs : BaseEntity
    {

        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be greater than 3 characters")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject must be less than 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }
    }
}
