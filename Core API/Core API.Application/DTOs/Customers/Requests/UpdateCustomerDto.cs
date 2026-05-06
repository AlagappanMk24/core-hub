using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Customer.Request
{
    /// <summary>
    /// Data transfer object for updating an existing customer.
    /// </summary>
    public class UpdateCustomerDto : CreateCustomerDto
    {
        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        [Required(ErrorMessage = "Customer ID is required")]
        public int Id { get; set; }
    }
}