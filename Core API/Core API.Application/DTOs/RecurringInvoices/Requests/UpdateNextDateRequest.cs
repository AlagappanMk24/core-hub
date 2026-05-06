using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.RecurringInvoice.Request
{
    /// <summary>
    /// Request DTO for updating the next invoice date of a recurring template
    /// </summary>
    public class UpdateNextDateRequest
    {
        /// <summary>
        /// New next invoice date
        /// </summary>
        [Required(ErrorMessage = "Next date is required")]
        [DataType(DataType.Date)]
        public DateTime NextDate { get; set; }
    }
}