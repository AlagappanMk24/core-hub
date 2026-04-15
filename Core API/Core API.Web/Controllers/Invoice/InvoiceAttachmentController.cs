using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Web.Controllers.Invoice
{
    /// <summary>
    /// Controller for managing invoice attachments
    /// </summary>
    [Route("api/invoices")] 
    [ApiController]
    [Authorize]
    public class InvoiceAttachmentController(
        IInvoiceAttachmentService invoiceAttachmentService,
        ILogger<InvoiceAttachmentController> logger) : BaseApiController
    {
        private readonly IInvoiceAttachmentService _invoiceAttachmentService = invoiceAttachmentService ?? throw new ArgumentNullException(nameof(invoiceAttachmentService));
        private readonly ILogger<InvoiceAttachmentController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Gets a specific file attachment for an invoice
        /// </summary>
        /// <param name="invoiceId">Parent Invoice ID</param>
        /// <param name="fileName">The specific filename</param>
        [HttpGet("{invoiceId}/attachments/{fileName}")]
        public IActionResult GetAttachment(int invoiceId, string fileName)
        {
            try
            {
                // Extract CompanyId from Context (Security Best Practice)
                var companyId = CurrentContext.CompanyId;
                if (companyId == null) return Unauthorized();

                string attachmentStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "Attachments", "Invoices");
                var decodedFileName = Uri.UnescapeDataString(fileName);

                if (decodedFileName.Contains("..")) return BadRequest("Invalid filename.");

                var filePath = Path.Combine(attachmentStoragePath, companyId.ToString(), invoiceId.ToString(), decodedFileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found.");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(decodedFileName);

                return File(fileBytes, contentType, decodedFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving file: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an attachment from an invoice.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{invoiceId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int invoiceId, int attachmentId)
        {
            var operationContext = CurrentContext;
            try
            {
                _logger.LogInformation("Deleting attachment {AttachmentId} for invoice {InvoiceId} for company {CompanyId} by user {UserId}",
                    attachmentId, invoiceId, operationContext.CompanyId, operationContext.UserId);

                var result = await _invoiceAttachmentService.DeleteAttachmentAsync(invoiceId, attachmentId, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Attachment deletion failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Attachment Deletion Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during attachment deletion.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting attachment {AttachmentId} for invoice {InvoiceId} for company {CompanyId}",
                    attachmentId, invoiceId, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while deleting the attachment from the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} for invoice {InvoiceId} for company {CompanyId}",
                    attachmentId, invoiceId, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while deleting the attachment."
                });
            }
        }
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
}