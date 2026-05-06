using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Invoicing.InvoiceDuplication
{
    /// <summary>
    /// Implementation of invoice duplication service
    /// </summary>
    public class InvoiceDuplicationService(ILogger<InvoiceDuplicationService> logger) : IInvoiceDuplicationService
    {
        private readonly ILogger<InvoiceDuplicationService> _logger = logger;
        public async Task<Domain.Entities.Invoices.Invoice> DuplicateInvoiceAsync(Domain.Entities.Invoices.Invoice originalInvoice, OperationContext operationContext)
        {
            var duplicatedInvoice = new Domain.Entities.Invoices.Invoice
            {
                // Copy basic properties
                IssueDate = DateTime.UtcNow,
                DueDate = CalculateDueDateFromOriginal(originalInvoice),
                PONumber = originalInvoice.PONumber,
                ProjectDetail = originalInvoice.ProjectDetail,
                InvoiceType = originalInvoice.InvoiceType,
                Currency = originalInvoice.Currency,
                CurrencyRate = originalInvoice.CurrencyRate,
                CustomerId = originalInvoice.CustomerId,
                CustomerNotes = originalInvoice.CustomerNotes,
                InternalNotes = originalInvoice.InternalNotes,
                TermsAndConditions = originalInvoice.TermsAndConditions,
                FooterNote = originalInvoice.FooterNote,
                PaymentMethod = originalInvoice.PaymentMethod,
                PaymentTerms = originalInvoice.PaymentTerms,
                ShippingAmount = originalInvoice.ShippingAmount,
                AdjustmentAmount = originalInvoice.AdjustmentAmount,
                AdjustmentDescription = originalInvoice.AdjustmentDescription,

                // Set status as Draft
                InvoiceStatus = InvoiceStatus.Draft,
                PaymentStatus = PaymentStatus.Pending,

                // Reset payment fields
                AmountPaid = 0,
                AmountRefunded = 0,
                AmountDue = 0,
                SentDate = null,
                PaidDate = null,
                PaymentGateway = null,
                PaymentTransactionId = null,

                // Set system fields
                CompanyId = originalInvoice.CompanyId,
                IsAutomated = originalInvoice.IsAutomated,
                SourceSystem = "Duplicate",
                CreatedBy = operationContext.UserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            CopyInvoiceItems(originalInvoice, duplicatedInvoice, operationContext);
            CopyTaxDetails(originalInvoice, duplicatedInvoice, operationContext);
            CopyDiscounts(originalInvoice, duplicatedInvoice, operationContext);

            if (originalInvoice.InvoiceAttachments.Count != 0)
            {
                duplicatedInvoice.InvoiceAttachments = await CopyAttachmentsAsync(
                    originalInvoice.InvoiceAttachments,
                    originalInvoice.CompanyId,
                    operationContext.UserId);
            }
            return duplicatedInvoice;
        }
        public void CopyInvoiceItems(Domain.Entities.Invoices.Invoice original, Domain.Entities.Invoices.Invoice duplicate, OperationContext operationContext)
        {
            foreach (var item in original.InvoiceItems)
            {
                duplicate.InvoiceItems.Add(new InvoiceItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxType = item.TaxType,
                    TaxPercentage = item.TaxPercentage,
                    TaxAmount = item.TaxAmount,
                    Amount = item.Amount,
                    TotalAmount = item.TotalAmount,
                    IsTaxable = item.IsTaxable,
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        public void CopyTaxDetails(Domain.Entities.Invoices.Invoice original, Domain.Entities.Invoices.Invoice duplicate, OperationContext operationContext)
        {
            foreach (var tax in original.TaxDetails)
            {
                duplicate.TaxDetails.Add(new InvoiceTaxDetail
                {
                    TaxName = tax.TaxName,
                    Rate = tax.Rate,
                    TaxAmount = tax.TaxAmount,
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        public void CopyDiscounts(Domain.Entities.Invoices.Invoice original, Domain.Entities.Invoices.Invoice duplicate, OperationContext operationContext)
        {
            foreach (var discount in original.Discounts)
            {
                duplicate.Discounts.Add(new InvoiceDiscount
                {
                    Description = discount.Description,
                    DiscountType = discount.DiscountType,
                    Amount = discount.Amount,
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        public async Task<List<Domain.Entities.Invoices.InvoiceAttachment>> CopyAttachmentsAsync(List<Domain.Entities.Invoices.InvoiceAttachment> originalAttachments, int companyId, string userId)
        {
            var newAttachments = new List<Domain.Entities.Invoices.InvoiceAttachment>();
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "Invoices",
                companyId.ToString(), "Temp");

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            foreach (var attachment in originalAttachments)
            {
                if (string.IsNullOrEmpty(attachment.FilePath))
                    continue;

                var originalFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                    attachment.FilePath.TrimStart('/'));

                if (!System.IO.File.Exists(originalFilePath))
                    continue;

                var uniqueFileName = $"{Guid.NewGuid()}_{attachment.FileName}";
                var newFilePath = Path.Combine(basePath, uniqueFileName);
                var relativePath = $"/Attachments/Invoices/{companyId}/Temp/{uniqueFileName}";

                System.IO.File.Copy(originalFilePath, newFilePath);

                newAttachments.Add(new Domain.Entities.Invoices.InvoiceAttachment
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
                });
            }
            return await System.Threading.Tasks.Task.FromResult(newAttachments);
        }

        #region Private Helper Methods
        private DateTime CalculateDueDateFromOriginal(Domain.Entities.Invoices.Invoice originalInvoice)
        {
            var issueDate = DateTime.UtcNow;

            if (string.IsNullOrEmpty(originalInvoice.PaymentTerms))
                return issueDate.AddDays(30);

            var match = System.Text.RegularExpressions.Regex.Match(originalInvoice.PaymentTerms, @"Net\s+(\d+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success && int.TryParse(match.Groups[1].Value, out var days))
                return issueDate.AddDays(days);

            if (originalInvoice.PaymentTerms.Equals("Due on Receipt", StringComparison.OrdinalIgnoreCase))
                return issueDate;

            return issueDate.AddDays(30);
        }
        #endregion
    }
}
