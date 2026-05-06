using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities.Invoices;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core_API.Infrastructure.Services.Invoicing.InvoiceAttachment
{
    public class InvoiceAttachmentService(IUnitOfWork unitOfWork, ILogger<InvoiceAttachmentService> logger) : IInvoiceAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ILogger<InvoiceAttachmentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<List<Domain.Entities.Invoices.InvoiceAttachment>> HandleAttachmentsAsync(List<InvoiceAttachmentDto> attachmentDtos, int companyId, int invoiceId, string userId)
        {
            var attachments = new List<Domain.Entities.Invoices.InvoiceAttachment>();

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "Invoices",
                companyId.ToString(), invoiceId.ToString());

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            foreach (var attachmentDto in attachmentDtos.Where(a => a.FileContent != null))
            {
                var file = attachmentDto.FileContent;
                var uniqueFileName = $"{Guid.NewGuid()}_{attachmentDto.FileName}";
                var filePath = Path.Combine(basePath, uniqueFileName);
                var relativePath = $"/Attachments/Invoices/{companyId}/{invoiceId}/{uniqueFileName}";

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new Domain.Entities.Invoices.InvoiceAttachment
                {
                    InvoiceId = invoiceId,
                    FileName = attachmentDto.FileName,
                    FilePath = relativePath,
                    FileUrl = relativePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    Description = attachmentDto.FileName,
                    IsPublic = true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };

                attachments.Add(attachment);
            }

            return attachments;
        }
        public async Task<OperationResult<bool>> DeleteAttachmentAsync(int invoiceId, int attachmentId, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for deleting an attachment.");
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                // Fetch the invoice to ensure it exists and belongs to the company
                var invoice = await _unitOfWork.Invoices.GetAsync(i => i.Id == invoiceId && i.CompanyId == companyId && !i.IsDeleted);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to company {CompanyId}.", invoiceId, companyId);
                    return OperationResult<bool>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Fetch the attachment
                var attachment = await _unitOfWork.InvoiceAttachments.GetAsync(a => a.Id == attachmentId && a.InvoiceId == invoiceId && !a.IsDeleted);
                if (attachment == null)
                {
                    _logger.LogWarning("Attachment {AttachmentId} not found for invoice {InvoiceId}.", attachmentId, invoiceId);
                    return OperationResult<bool>.FailureResult("Attachment not found or does not belong to the specified invoice.");
                }

                // Delete file from storage
                var filePath = Path.Combine("wwwroot", attachment.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Perform soft delete
                attachment.IsDeleted = true;
                attachment.UpdatedBy = operationContext.UserId;
                attachment.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.InvoiceAttachments.Update(attachment);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new InvoiceAuditLog
                {
                    InvoiceId = invoiceId,
                    Action = "AttachmentDeleted",
                    Description = $"Attachment {attachment.FileName} deleted",
                    Changes = JsonSerializer.Serialize(new { attachmentId, attachment.FileName }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Attachment {AttachmentId} deleted successfully for invoice {InvoiceId} for company {CompanyId}.",
                    attachmentId, invoiceId, companyId);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} for invoice {InvoiceId} for company {CompanyId}",
                    attachmentId, invoiceId, operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to delete attachment.");
            }
        }
        public List<Domain.Entities.Invoices.InvoiceAttachment> CopyAttachments(List<Domain.Entities.Invoices.InvoiceAttachment> originalAttachments, int companyId, string userId)
        {
            var newAttachments = new List<Domain.Entities.Invoices.InvoiceAttachment>();

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "Invoices",
                companyId.ToString(), "Temp");

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            foreach (var attachment in originalAttachments)
            {
                if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    var originalFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        attachment.FilePath.TrimStart('/'));

                    if (System.IO.File.Exists(originalFilePath))
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{attachment.FileName}";
                        var newFilePath = Path.Combine(basePath, uniqueFileName);
                        var relativePath = $"/Attachments/Invoices/{companyId}/Temp/{uniqueFileName}";

                        // Copy file
                        System.IO.File.Copy(originalFilePath, newFilePath);

                        var newAttachment = new Domain.Entities.Invoices.InvoiceAttachment
                        {
                            FileName = attachment.FileName,
                            FilePath = relativePath,
                            FileUrl = relativePath,
                            ContentType = attachment.ContentType,
                            FileSize = attachment.FileSize,
                            Description = $"[Duplicated] {attachment.Description ?? attachment.FileName}",
                            IsPublic = true,
                            CreatedBy = userId,
                            CreatedDate = DateTime.UtcNow
                        };

                        newAttachments.Add(newAttachment);
                    }
                }
            }

            return newAttachments;
        }
    }
}
