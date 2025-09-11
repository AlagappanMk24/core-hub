using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.File.Excel;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Application.DTOs.EmailDto;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Core_API.Web.Controllers
{
    /// <summary>
    /// Controller for managing invoice-related operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="InvoiceController"/> class.
    /// </remarks>
    /// <param name="invoiceService">The invoice service for business logic.</param>
    /// <param name="logger">The logger for logging controller actions.</param>
    [Route("api/[controller]")] 
    [ApiController]
    [Authorize(Roles = "Admin, User, Customer")]
    public class InvoiceController(IInvoiceService invoiceService, IExcelService excelService, IPdfService pdfService, ITaxService taxService, ILogger<InvoiceController> logger) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        private readonly IExcelService _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        private readonly IPdfService _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        private readonly ITaxService _taxService = taxService ?? throw new ArgumentNullException(nameof(taxService));
        private readonly ILogger<InvoiceController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        //private OperationContext GetOperationContext()
        //{
        //    var companyIdClaim = User.FindFirst("companyId")?.Value;
        //    var customerIdClaim = User.FindFirst("customerId")?.Value;
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    return new OperationContext(int.Parse(companyIdClaim), userId, int.Parse(customerIdClaim));
        //}

        private OperationContext GetOperationContext()
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value; // Case-sensitive, matches JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID claim is missing.");
                throw new UnauthorizedAccessException("User ID claim is missing.");
            }

            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var companyId) || companyId <= 0)
            {
                _logger.LogError("Valid CompanyId claim is missing or invalid.");
                throw new UnauthorizedAccessException("Valid CompanyId claim is required.");
            }

            // Create context for company-specific operations (no CustomerId)
            return new OperationContext(userId, companyId: companyId);
        }

        /// <summary>
        /// Creates a new invoice for the authenticated user's company.
        /// </summary>
        /// <param name="dto">The invoice data to create.</param>
        /// <returns>A <see cref="InvoiceResponseDto"/> representing the created invoice.</returns>
        /// <response code="201">Invoice created successfully.</response>
        /// <response code="400">Invalid request data or invoice creation failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto invoiceDto)
        {
            var operationContext = GetOperationContext();
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for invoice creation.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid invoice data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Creating invoice for company {CompanyId} by user {UserId}", operationContext.CompanyId, operationContext.UserId);

                if (invoiceDto.IsAutomated)
                {
                    invoiceDto.InvoiceNumber = $"INV{DateTime.UtcNow.Ticks}";
                }

                var result = await _invoiceService.CreateAsync(invoiceDto, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice creation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Creation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice creation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating invoice for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while creating the invoice in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while creating the invoice."
                });
            }
        }

        /// <summary>
        /// Updates an existing invoice.
        /// </summary>
        /// <param name="id">The ID of the invoice to update.</param>
        /// <param name="dto">The updated invoice data.</param>
        /// <returns>The updated <see cref="InvoiceResponseDto"/>.</returns>
        /// <response code="200">Invoice updated successfully.</response>
        /// <response code="400">Invalid request data or invoice not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceUpdateDto dto)
        {
            var operationContext = GetOperationContext();
            try
            {
                if (!ModelState.IsValid || id != dto.Id)
                {
                    _logger.LogWarning("Invalid model state or ID mismatch for invoice update. ID: {Id}, DTO ID: {DtoId}", id, dto.Id);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = id != dto.Id ? "ID in URL does not match DTO ID." : "Invalid invoice data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }
                _logger.LogInformation("Updating invoice {InvoiceId} for company {CompanyId} by user {UserId}", id, operationContext.CompanyId, operationContext.UserId);

                var result = await _invoiceService.UpdateAsync(dto, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice update failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Update Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice update.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while updating the invoice in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while updating the invoice."
                });
            }
        }

        /// <summary>
        /// Deletes an invoice by ID (soft delete).
        /// </summary>
        /// <param name="id">The ID of the invoice to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">Invoice deleted successfully.</response>
        /// <response code="400">Invoice not found or deletion failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Deleting invoice {InvoiceId} for company {CompanyId} by user {UserId}", id, operationContext.CompanyId, operationContext.UserId);

                var result = await _invoiceService.DeleteAsync(id, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice deletion failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Deletion Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice deletion.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while deleting the invoice from the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while deleting the invoice."
                });
            }
        }

        /// <summary>
        /// Retrieves an invoice by ID.
        /// </summary>
        /// <param name="id">The ID of the invoice to retrieve.</param>
        /// <returns>The <see cref="InvoiceResponseDto"/> for the specified invoice.</returns>
        /// <response code="200">Invoice retrieved successfully.</response>
        /// <response code="404">Invoice not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Retrieving invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);

                var result = await _invoiceService.GetByIdAsync(id, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Invoice Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving the invoice."
                });
            }
        }

        [HttpGet("{id}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoicePdf(int id)
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Generating PDF for invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);

                var result = await _pdfService.GenerateInvoicePdfAsync(id, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice PDF generation failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Invoice Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }
                // Get the PDF stream from the InvoiceResponseDto
                var pdfStream = result.Data.PdfStream;

                if (pdfStream == null || !pdfStream.CanRead)
                {
                    _logger.LogError("PDF stream is null or not readable for invoice {InvoiceId}", id);
                    return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                    {
                        Title = "PDF Generation Error",
                        Detail = "The generated PDF stream is invalid."
                    });
                }

                // Convert the MemoryStream to a byte array
                byte[] pdfBytes = pdfStream.ToArray();

                // Ensure the stream position is at the beginning before returning (optional but good practice if you were to reuse the stream)
                pdfStream.Position = 0;

                // Return the byte array as a file
                return File(pdfBytes, "application/pdf", $"invoice_{result.Data.InvoiceNumber}.pdf");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice PDF generation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while generating the invoice PDF."
                });
            }
        }

        /// <summary>
        /// Retrieves a paged list of invoices for the authenticated user's company.
        /// </summary>
        /// <param name="filter">The filter parameters for retrieving invoices.</param>
        /// <returns>A <see cref="PaginatedResult{InvoiceResponseDto}"/> containing the invoices.</returns>
        /// <response code="200">Invoices retrieved successfully.</response>
        /// <response code="400">Invalid filter parameters or retrieval failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaged([FromQuery] InvoiceFilterRequestDto filter)
        {
            var operationContext = GetOperationContext();
            try
            {
                if (!filter.IsValid())
                {
                    _logger.LogWarning("Invalid filter parameters: pageNumber={PageNumber}, pageSize={PageSize}", filter.PageNumber, filter.PageSize);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Filter Parameters",
                        Detail = "Page number and page size must be greater than 0, and minAmount must not exceed maxAmount.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation("Retrieving paged invoices for company {CompanyId}, page {PageNumber}, size {PageSize}, search: {Search}, status: {Status}",
                    operationContext.CompanyId, filter.PageNumber, filter.PageSize, filter.Search ?? "none", filter.InvoiceStatus ?? "none");

                var result = await _invoiceService.GetPagedAsync(operationContext, filter);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Paged invoices retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during paged invoice retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged invoices for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving invoices."
                });
            }
        }
        [HttpGet("export/excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportExcel([FromQuery] InvoiceFilterRequestDto invoiceFilterRequestDto)
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Exporting invoices to Excel for company {CompanyId}", operationContext.CompanyId);

                var result = await _excelService.ExportInvoicesExcelAsync(operationContext, invoiceFilterRequestDto);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Excel export failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Excel Export Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"invoices_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during Excel export.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices to Excel for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while exporting invoices to Excel."
                });
            }
        }

        [HttpGet("export/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportPdf([FromQuery] InvoiceFilterRequestDto invoiceFilterRequestDto)
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Exporting invoices to PDF for company {CompanyId}", operationContext.CompanyId);

                var result = await _pdfService.ExportInvoicesPdfAsync(operationContext, invoiceFilterRequestDto);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("PDF export failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "PDF Export Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return File(result.Data, "application/pdf", $"invoices_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during PDF export.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices to PDF for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while exporting invoices to PDF."
                });
            }
        }

        //[HttpPost("import")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> ImportInvoices(IFormFile file)
        //{
        //    var operationContext = GetOperationContext();
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //        {
        //            _logger.LogWarning("No file uploaded for import.");
        //            return BadRequest(new ProblemDetails
        //            {
        //                Title = "Invalid File",
        //                Detail = "No file was uploaded or the file is empty.",
        //                Status = StatusCodes.Status400BadRequest
        //            });
        //        }

        //        if (!file.FileName.EndsWith(".xlsx"))
        //        {
        //            _logger.LogWarning("Invalid file type uploaded: {FileName}", file.FileName);
        //            return BadRequest(new ProblemDetails
        //            {
        //                Title = "Invalid File Type",
        //                Detail = "Only .xlsx files are supported.",
        //                Status = StatusCodes.Status400BadRequest
        //            });
        //        }

        //        _logger.LogInformation("Importing invoices from file {FileName} for company {CompanyId}", file.FileName, operationContext.CompanyId);

        //        var result = await _invoiceService.ImportInvoicesAsync(file, operationContext);
        //        if (!result.IsSuccess)
        //        {
        //            _logger.LogWarning("Invoice import failed: {ErrorMessage}", result.ErrorMessage);
        //            return BadRequest(new ProblemDetails
        //            {
        //                Title = "Invoice Import Failed",
        //                Detail = result.ErrorMessage,
        //                Status = StatusCodes.Status400BadRequest
        //            });
        //        }

        //        return Ok(new
        //        {
        //            result.Data.SuccessCount,
        //            result.Data.ErrorCount,
        //            result.Data.Errors
        //        });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        _logger.LogWarning(ex, "Unauthorized access during invoice import.");
        //        return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error importing invoices for company {CompanyId}", operationContext.CompanyId);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        //        {
        //            Title = "Internal Server Error",
        //            Detail = "An unexpected error occurred while importing invoices."
        //        });
        //    }
        //}

        /// <summary>
        /// Retrieves a list of available tax types for the authenticated user's company.
        /// </summary>
        /// <returns>A list of <see cref="TaxTypeResponseDto"/>.</returns>
        /// <response code="200">Tax types retrieved successfully.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet("tax-types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetTaxTypes()
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Retrieving tax types for company {CompanyId}", operationContext.CompanyId);

                var result = await _taxService.GetTaxTypesAsync(operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Tax types retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Tax Types Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during tax types retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax types for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving tax types."
                });
            }
        }

        /// <summary>
        /// Creates a new tax type for the authenticated user's company.
        /// </summary>
        /// <param name="dto">The tax type data to create.</param>
        /// <returns>The created <see cref="TaxTypeResponseDto"/>.</returns>
        /// <response code="201">Tax type created successfully.</response>
        /// <response code="400">Invalid request data or creation failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("tax-types")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTaxType([FromBody] TaxTypeCreateDto dto)
        {
            var operationContext = GetOperationContext();
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for tax type creation.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid tax type data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }
                _logger.LogInformation("Creating tax type for company {CompanyId} by user {UserId}", operationContext.CompanyId, operationContext.UserId);

                var result = await _taxService.CreateTaxTypeAsync(dto, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Tax type creation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Tax Type Creation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return CreatedAtAction(nameof(GetTaxTypes), null, result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during tax type creation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating tax type for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while creating the tax type in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax type for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while creating the tax type."
                });
            }
        }

        /// <summary>
        /// Sends an invoice to its recipient.
        /// </summary>
        /// <param name="id">The ID of the invoice to send.</param>
        /// <returns>A success message if the invoice was sent.</returns>
        /// <response code="200">Invoice sent successfully.</response>
        /// <response code="400">Invoice not found or sending failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("{id}/send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendInvoice(int id, [FromBody] EmailDataDto emailData)
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Sending invoice {InvoiceId} for company {CompanyId} by user {UserId}", id, operationContext.CompanyId, operationContext.UserId);

                var result = await _invoiceService.SendInvoiceAsync(id, operationContext, emailData);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice sending failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Sending Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new { Message = "Invoice sent successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice sending.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while sending the invoice."
                });
            }
        }

        /// <summary>
        /// Retrieves various statistics related to invoices for the authenticated user's company.
        /// </summary>
        /// <returns>Invoice statistics.</returns>
        /// <response code="200">Invoice stats retrieved successfully.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStats()
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Retrieving invoice stats for company {CompanyId}", operationContext.CompanyId);

                var result = await _invoiceService.GetStatsAsync(operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Stats retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Stats Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during stats retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving stats."
                });
            }
        }

        /// <summary>
        /// Retrieves the invoice settings for the authenticated user's company.
        /// </summary>
        /// <returns>The <see cref="InvoiceSettings"/> object.</returns>
        /// <response code="200">Invoice settings retrieved successfully.</response>
        /// <response code="404">Invoice settings not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<InvoiceSettings>> GetInvoiceSettings()
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Retrieving invoice settings for company {CompanyId}", operationContext.CompanyId);

                var result = await _invoiceService.GetInvoiceSettingsAsync(operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice settings retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Invoice Settings Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice settings retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving invoice settings."
                });
            }
        }

        /// <summary>
        /// Saves or updates the invoice settings for the authenticated user's company.
        /// </summary>
        /// <param name="invoiceSettingsDto">The invoice settings data to save.</param>
        /// <returns>A success response if the settings were saved.</returns>
        /// <response code="200">Invoice settings saved successfully.</response>
        /// <response code="400">Invalid request data or settings save failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SaveInvoiceSettings([FromBody] InvoiceSettingsDto invoiceSettingsDto)
        {
            var operationContext = GetOperationContext();
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for invoice settings save.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid invoice settings data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Saving invoice settings for company {CompanyId} by user {UserId}", operationContext.CompanyId, operationContext.UserId);

                var result = await _invoiceService.SaveInvoiceSettingsAsync(invoiceSettingsDto, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice settings save failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Settings Save Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice settings save.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while saving invoice settings."
                });
            }
        }

        /// <summary>
        /// Retrieves the next available invoice number for the authenticated user's company.
        /// </summary>
        /// <returns>The next invoice number as a string.</returns>
        /// <response code="200">Next invoice number retrieved successfully.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet("next-invoice-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<string>> GetNextInvoiceNumber()
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Retrieving next invoice number for company {CompanyId}", operationContext.CompanyId);

                var result = await _invoiceService.GetNextInvoiceNumberAsync(operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Next invoice number retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Next Invoice Number Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during next invoice number retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next invoice number for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving the next invoice number."
                });
            }
        }

        [HttpGet("template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetImportTemplate()
        {
            try
            {
                _logger.LogInformation("Generating import template for company {CompanyId}", GetOperationContext().CompanyId);
                var templateBytes = _excelService.GenerateImportTemplate();
                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "invoice-import-template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating import template for company {CompanyId}", GetOperationContext().CompanyId);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Failed to generate template."
                });
            }
        }
    }
}