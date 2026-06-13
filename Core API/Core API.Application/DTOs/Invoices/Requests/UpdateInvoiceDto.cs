using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Invoice.Request;

/// <summary>
/// DTO for updating an existing invoice
/// </summary>
public class UpdateInvoiceDto : CreateInvoiceDto
{
    // ── Identity ─────────────────────────────────────────────────────────
    /// <summary>
    /// Invoice ID
    /// </summary>
    [Required(ErrorMessage = "Invoice ID is required")]
    public int Id { get; set; }
}