using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Customer.Request
{
    public class CustomerUpdateDto : CustomerCreateDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public int Id { get; set; }
    }
}
