using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Invoice.Request;
public class InvoiceUpdateDto : InvoiceCreateDto
{
    [Required(ErrorMessage = "Invoice ID is required")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = "Draft";
}