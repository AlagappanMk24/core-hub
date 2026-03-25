using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.File.Excel;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Core_API.Infrastructure.Services.File.Excel
{
    public class ExcelService : IExcelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(IUnitOfWork unitOfWork, ILogger<ExcelService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<byte[]>> ExportInvoicesExcelAsync(
            OperationContext operationContext,
            InvoiceFilterRequestDto filter)
        {
            try
            {
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for exporting invoices");
                    return OperationResult<byte[]>.FailureResult("Company ID is required.");
                }

                int companyId = operationContext.CompanyId.Value;
                var result = await _unitOfWork.Invoices.GetPagedAsync(companyId, filter);

                if (result.Items.Count == 0)
                {
                    return OperationResult<byte[]>.FailureResult("No invoices found for export.");
                }

                using var workbook = new XSSFWorkbook();

                // Create all sheets
                CreateInvoiceSummarySheet(workbook, result.Items);
                CreateInvoiceDetailsSheet(workbook, result.Items);
                CreateInvoiceItemsSheet(workbook, result.Items);
                CreateTaxDetailsSheet(workbook, result.Items);
                CreateDiscountsSheet(workbook, result.Items);
                CreatePaymentsSheet(workbook, result.Items);
                CreateAuditLogsSheet(workbook, result.Items);
                CreateAttachmentsSheet(workbook, result.Items);

                // Write to memory stream
                using var stream = new MemoryStream();
                workbook.Write(stream);
                stream.Position = 0;

                return OperationResult<byte[]>.SuccessResult(stream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices to Excel for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<byte[]>.FailureResult("Failed to export invoices to Excel: " + ex.Message);
            }
        }

        private void CreateInvoiceSummarySheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Invoice Summary");

            // Define headers with all Invoice fields
            var headers = new[]
            {
                "Invoice ID", "Invoice Number", "Order Number", "PO Number",
                "Issue Date", "Due Date", "Sent Date", "Paid Date",
                "Invoice Status", "Payment Status", "Invoice Type",
                "Customer ID", "Customer Name", "Company ID",
                "Currency", "Currency Rate",
                "Subtotal", "Discount Total", "Tax Total", "Shipping Amount",
                "Adjustment Amount", "Adjustment Description",
                "Total Amount", "Amount Paid", "Amount Due", "Amount Refunded",
                "Payment Method", "Payment Gateway", "Payment Terms",
                "Customer Notes", "Internal Notes", "Terms & Conditions", "Footer Note",
                "Project Detail", "Is Automated", "Is Recurring", "Recurring Invoice ID",
                "Source System", "Created By", "Created Date", "Updated By", "Updated Date"
            };

            // Create header row with styling
            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var dateStyle = CreateDateStyle(workbook);
            var currencyStyle = CreateCurrencyStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            // Add data rows
            int rowIndex = 1;
            foreach (var invoice in invoices)
            {
                var row = sheet.CreateRow(rowIndex++);
                int col = 0;

                row.CreateCell(col++).SetCellValue(invoice.Id);
                row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.PONumber ?? string.Empty);

                // Date cells with formatting
                var issueDateCell = row.CreateCell(col++);
                issueDateCell.SetCellValue(invoice.IssueDate);
                issueDateCell.CellStyle = dateStyle;

                var dueDateCell = row.CreateCell(col++);
                dueDateCell.SetCellValue(invoice.DueDate);
                dueDateCell.CellStyle = dateStyle;

                if (invoice.SentDate.HasValue)
                {
                    var sentDateCell = row.CreateCell(col++);
                    sentDateCell.SetCellValue(invoice.SentDate.Value);
                    sentDateCell.CellStyle = dateStyle;
                }
                else
                {
                    row.CreateCell(col++).SetCellValue(string.Empty);
                }

                if (invoice.PaidDate.HasValue)
                {
                    var paidDateCell = row.CreateCell(col++);
                    paidDateCell.SetCellValue(invoice.PaidDate.Value);
                    paidDateCell.CellStyle = dateStyle;
                }
                else
                {
                    row.CreateCell(col++).SetCellValue(string.Empty);
                }

                row.CreateCell(col++).SetCellValue(invoice.InvoiceStatus.ToString());
                row.CreateCell(col++).SetCellValue(invoice.PaymentStatus.ToString());
                row.CreateCell(col++).SetCellValue(invoice.InvoiceType.ToString());

                row.CreateCell(col++).SetCellValue(invoice.CustomerId);
                row.CreateCell(col++).SetCellValue(invoice.Customer?.Name ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.CompanyId);

                row.CreateCell(col++).SetCellValue(invoice.Currency ?? "USD");

                var currencyRateCell = row.CreateCell(col++);
                currencyRateCell.SetCellValue((double)invoice.CurrencyRate);
                currencyRateCell.CellStyle = currencyStyle;

                var subtotalCell = row.CreateCell(col++);
                subtotalCell.SetCellValue((double)invoice.Subtotal);
                subtotalCell.CellStyle = currencyStyle;

                var discountTotalCell = row.CreateCell(col++);
                discountTotalCell.SetCellValue((double)invoice.DiscountTotal);
                discountTotalCell.CellStyle = currencyStyle;

                var taxTotalCell = row.CreateCell(col++);
                taxTotalCell.SetCellValue((double)invoice.TaxTotal);
                taxTotalCell.CellStyle = currencyStyle;

                var shippingCell = row.CreateCell(col++);
                shippingCell.SetCellValue((double)invoice.ShippingAmount);
                shippingCell.CellStyle = currencyStyle;

                var adjustmentCell = row.CreateCell(col++);
                adjustmentCell.SetCellValue((double)invoice.AdjustmentAmount);
                adjustmentCell.CellStyle = currencyStyle;

                row.CreateCell(col++).SetCellValue(invoice.AdjustmentDescription ?? string.Empty);

                var totalCell = row.CreateCell(col++);
                totalCell.SetCellValue((double)invoice.TotalAmount);
                totalCell.CellStyle = currencyStyle;

                var amountPaidCell = row.CreateCell(col++);
                amountPaidCell.SetCellValue((double)invoice.AmountPaid);
                amountPaidCell.CellStyle = currencyStyle;

                var amountDueCell = row.CreateCell(col++);
                amountDueCell.SetCellValue((double)invoice.AmountDue);
                amountDueCell.CellStyle = currencyStyle;

                var amountRefundedCell = row.CreateCell(col++);
                amountRefundedCell.SetCellValue((double)invoice.AmountRefunded);
                amountRefundedCell.CellStyle = currencyStyle;

                row.CreateCell(col++).SetCellValue(invoice.PaymentMethod ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.PaymentGateway ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.PaymentTerms ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.CustomerNotes ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.InternalNotes ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.TermsAndConditions ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.FooterNote ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.ProjectDetail ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.IsAutomated ? "Yes" : "No");
                row.CreateCell(col++).SetCellValue(invoice.IsRecurring ? "Yes" : "No");
                row.CreateCell(col++).SetCellValue(invoice.RecurringInvoiceId?.ToString() ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.SourceSystem ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.CreatedBy ?? string.Empty);

                var createdDateCell = row.CreateCell(col++);
                createdDateCell.SetCellValue((DateTime)invoice.CreatedDate);
                createdDateCell.CellStyle = dateStyle;

                row.CreateCell(col++).SetCellValue(invoice.UpdatedBy ?? string.Empty);

                if (invoice.UpdatedDate.HasValue)
                {
                    var updatedDateCell = row.CreateCell(col++);
                    updatedDateCell.SetCellValue(invoice.UpdatedDate.Value);
                    updatedDateCell.CellStyle = dateStyle;
                }
                else
                {
                    row.CreateCell(col++).SetCellValue(string.Empty);
                }
            }

            // Auto-size columns
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreateInvoiceDetailsSheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Invoice Details");

            var headers = new[]
            {
                "Invoice ID", "Invoice Number", "Customer ID", "Customer Name",
                "Billing Address", "Shipping Address", "Payment Method", "Payment Terms",
                "Currency", "Subtotal", "Tax Total", "Discount Total", "Total Amount",
                "Amount Paid", "Amount Due", "Amount Refunded", "Payment Status"
            };

            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var currencyStyle = CreateCurrencyStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var invoice in invoices)
            {
                var row = sheet.CreateRow(rowIndex++);
                int col = 0;

                row.CreateCell(col++).SetCellValue(invoice.Id);
                row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.CustomerId);
                row.CreateCell(col++).SetCellValue(invoice.Customer?.Name ?? string.Empty);

                // Format address
                var billingAddress = invoice.Customer?.Address != null
                    ? $"{invoice.Customer.Address.Address1}, {invoice.Customer.Address.City}, {invoice.Customer.Address.State} {invoice.Customer.Address.ZipCode}, {invoice.Customer.Address.Country}"
                    : string.Empty;
                row.CreateCell(col++).SetCellValue(billingAddress);
                row.CreateCell(col++).SetCellValue(string.Empty); // Shipping address placeholder

                row.CreateCell(col++).SetCellValue(invoice.PaymentMethod ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.PaymentTerms ?? string.Empty);
                row.CreateCell(col++).SetCellValue(invoice.Currency ?? "USD");

                var subtotalCell = row.CreateCell(col++);
                subtotalCell.SetCellValue((double)invoice.Subtotal);
                subtotalCell.CellStyle = currencyStyle;

                var taxCell = row.CreateCell(col++);
                taxCell.SetCellValue((double)invoice.TaxTotal);
                taxCell.CellStyle = currencyStyle;

                var discountCell = row.CreateCell(col++);
                discountCell.SetCellValue((double)invoice.DiscountTotal);
                discountCell.CellStyle = currencyStyle;

                var totalCell = row.CreateCell(col++);
                totalCell.SetCellValue((double)invoice.TotalAmount);
                totalCell.CellStyle = currencyStyle;

                var paidCell = row.CreateCell(col++);
                paidCell.SetCellValue((double)invoice.AmountPaid);
                paidCell.CellStyle = currencyStyle;

                var dueCell = row.CreateCell(col++);
                dueCell.SetCellValue((double)invoice.AmountDue);
                dueCell.CellStyle = currencyStyle;

                var refundedCell = row.CreateCell(col++);
                refundedCell.SetCellValue((double)invoice.AmountRefunded);
                refundedCell.CellStyle = currencyStyle;

                row.CreateCell(col++).SetCellValue(invoice.PaymentStatus.ToString());
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreateInvoiceItemsSheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Invoice Items");

            var headers = new[]
            {
                "Invoice ID", "Invoice Number", "Line Number", "Description", "Item Code",
                "Product ID", "Quantity", "Unit Price", "Amount", "Discount Amount",
                "Discount %", "Tax Code", "Tax Type", "Tax %", "Tax Amount",
                "Total Amount", "Unit of Measure", "Notes", "Is Taxable"
            };

            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var currencyStyle = CreateCurrencyStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var invoice in invoices.Where(i => i.InvoiceItems?.Any() == true))
            {
                foreach (var item in invoice.InvoiceItems)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    int col = 0;

                    row.CreateCell(col++).SetCellValue(invoice.Id);
                    row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(item.Description ?? string.Empty);
                    row.CreateCell(col++).SetCellValue((double)item.Quantity);

                    var unitPriceCell = row.CreateCell(col++);
                    unitPriceCell.SetCellValue((double)item.UnitPrice);
                    unitPriceCell.CellStyle = currencyStyle;

                    var amountCell = row.CreateCell(col++);
                    amountCell.SetCellValue((double)item.Amount);
                    amountCell.CellStyle = currencyStyle;

                    var discountAmountCell = row.CreateCell(col++);
                    discountAmountCell.SetCellValue((double)item.Amount);
                    discountAmountCell.CellStyle = currencyStyle;

                    row.CreateCell(col++).SetCellValue(item.TaxType ?? string.Empty);
                    row.CreateCell(col++).SetCellValue((double)item.TaxPercentage);

                    var taxAmountCell = row.CreateCell(col++);
                    taxAmountCell.SetCellValue((double)item.TaxAmount);
                    taxAmountCell.CellStyle = currencyStyle;

                    var totalAmountCell = row.CreateCell(col++);
                    totalAmountCell.SetCellValue((double)item.TotalAmount);
                    totalAmountCell.CellStyle = currencyStyle;

                    row.CreateCell(col++).SetCellValue(item.IsTaxable ? "Yes" : "No");
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreateTaxDetailsSheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Tax Details");

            var headers = new[]
            {
                "Invoice ID", "Invoice Number", "Tax Name", "Tax Code",
                "Rate (%)", "Taxable Amount", "Tax Amount", "Is Compound",
                "Parent Tax ID", "Jurisdiction", "Region", "Calculation Method"
            };

            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var currencyStyle = CreateCurrencyStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var invoice in invoices.Where(i => i.TaxDetails?.Any() == true))
            {
                foreach (var tax in invoice.TaxDetails)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    int col = 0;

                    row.CreateCell(col++).SetCellValue(invoice.Id);
                    row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(tax.TaxName ?? string.Empty);
                    row.CreateCell(col++).SetCellValue((double)tax.Rate);


                    var taxAmountCell = row.CreateCell(col++);
                    taxAmountCell.SetCellValue((double)tax.TaxAmount);
                    taxAmountCell.CellStyle = currencyStyle;
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreateDiscountsSheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Discounts");

            var headers = new[]
            {
                "Invoice ID", "Invoice Number", "Description", "Discount Type",
                "Amount", "Percentage", "Coupon Code", "Item ID",
                "Apply Before Tax", "Valid From", "Valid To"
            };

            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var currencyStyle = CreateCurrencyStyle(workbook);
            var dateStyle = CreateDateStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var invoice in invoices.Where(i => i.Discounts?.Any() == true))
            {
                foreach (var discount in invoice.Discounts)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    int col = 0;

                    row.CreateCell(col++).SetCellValue(invoice.Id);
                    row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(discount.Description ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(discount.DiscountType.ToString());

                    var amountCell = row.CreateCell(col++);
                    amountCell.SetCellValue((double)discount.Amount);
                    amountCell.CellStyle = currencyStyle;
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreatePaymentsSheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Payments");

            var headers = new[]
            {
                "Payment ID", "Invoice ID", "Invoice Number", "Payment Date",
                "Amount", "Payment Method", "Payment Reference", "Notes",
                "Payment Status", "Bank Account ID", "Is Refund"
            };

            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var currencyStyle = CreateCurrencyStyle(workbook);
            var dateStyle = CreateDateStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var invoice in invoices.Where(i => i.Payments?.Any() == true))
            {
                foreach (var payment in invoice.Payments)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    int col = 0;

                    row.CreateCell(col++).SetCellValue(payment.Id);
                    row.CreateCell(col++).SetCellValue(invoice.Id);
                    row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);

                    var paymentDateCell = row.CreateCell(col++);
                    paymentDateCell.SetCellValue(payment.PaymentDate);
                    paymentDateCell.CellStyle = dateStyle;

                    var amountCell = row.CreateCell(col++);
                    amountCell.SetCellValue((double)payment.Amount);
                    amountCell.CellStyle = currencyStyle;

                    row.CreateCell(col++).SetCellValue(payment.PaymentMethod ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(payment.PaymentReference ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(payment.PaymentStatus ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(payment.BankAccountId?.ToString() ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(payment.IsRefund ? "Yes" : "No");
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreateAuditLogsSheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Audit Logs");

            var headers = new[]
            {
                "Log ID", "Invoice ID", "Invoice Number", "Action",
                "Description", "Changes", "IP Address", "User Agent",
                "Created By", "Created Date"
            };

            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var dateStyle = CreateDateStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var invoice in invoices.Where(i => i.AuditLogs?.Any() == true))
            {
                foreach (var log in invoice.AuditLogs)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    int col = 0;

                    row.CreateCell(col++).SetCellValue(log.Id);
                    row.CreateCell(col++).SetCellValue(invoice.Id);
                    row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(log.Action ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(log.Description ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(log.Changes ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(log.IpAddress ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(log.UserAgent ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(log.CreatedBy ?? string.Empty);

                    var createdDateCell = row.CreateCell(col++);
                    createdDateCell.SetCellValue((DateTime)log.CreatedDate);
                    createdDateCell.CellStyle = dateStyle;
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreateAttachmentsSheet(IWorkbook workbook, List<Invoice> invoices)
        {
            var sheet = workbook.CreateSheet("Attachments");

            var headers = new[]
            {
                "Attachment ID", "Invoice ID", "Invoice Number", "File Name",
                "File Path", "File URL", "Content Type", "File Size (KB)",
                "Description", "Is Public", "Created By", "Created Date"
            };

            var headerRow = sheet.CreateRow(0);
            var headerStyle = CreateHeaderStyle(workbook);
            var dateStyle = CreateDateStyle(workbook);

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var invoice in invoices.Where(i => i.InvoiceAttachments?.Any() == true))
            {
                foreach (var attachment in invoice.InvoiceAttachments)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    int col = 0;

                    row.CreateCell(col++).SetCellValue(attachment.Id);
                    row.CreateCell(col++).SetCellValue(invoice.Id);
                    row.CreateCell(col++).SetCellValue(invoice.InvoiceNumber ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(attachment.FileName ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(attachment.FilePath ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(attachment.FileUrl ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(attachment.ContentType ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(attachment.FileSize / 1024.0); // Convert to KB
                    row.CreateCell(col++).SetCellValue(attachment.Description ?? string.Empty);
                    row.CreateCell(col++).SetCellValue(attachment.IsPublic ? "Yes" : "No");
                    row.CreateCell(col++).SetCellValue(attachment.CreatedBy ?? string.Empty);

                    var createdDateCell = row.CreateCell(col++);
                    createdDateCell.SetCellValue((DateTime)attachment.CreatedDate);
                    createdDateCell.CellStyle = dateStyle;
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private ICellStyle CreateHeaderStyle(IWorkbook workbook)
        {
            var font = workbook.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Color = HSSFColor.White.Index;

            var style = workbook.CreateCellStyle();
            style.SetFont(font);
            style.FillForegroundColor = HSSFColor.DarkBlue.Index;
            style.FillPattern = FillPattern.SolidForeground;
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;

            return style;
        }

        private ICellStyle CreateDateStyle(IWorkbook workbook)
        {
            var style = workbook.CreateCellStyle();
            style.DataFormat = workbook.CreateDataFormat().GetFormat("dd-mmm-yyyy hh:mm");
            return style;
        }

        private ICellStyle CreateCurrencyStyle(IWorkbook workbook)
        {
            var style = workbook.CreateCellStyle();
            style.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.00");
            return style;
        }

        public byte[] GenerateImportTemplate()
        {
            using var workbook = new XSSFWorkbook();

            // Create Invoices sheet
            CreateImportTemplateSheet(workbook);

            // Create Instructions sheet
            CreateInstructionsSheet(workbook);

            // Write to memory stream
            using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            return memoryStream.ToArray();
        }

        private void CreateImportTemplateSheet(IWorkbook workbook)
        {
            var sheet = workbook.CreateSheet("Import Template");

            var headers = new[]
            {
                "CustomerId*", "InvoiceNumber*", "PONumber",
                "IssueDate*", "DueDate*", "InvoiceType*", "Currency*",
                "ItemDescription*", "ItemQuantity*", "ItemUnitPrice*", "ItemTaxType",
                "TaxName", "TaxRate", "TaxJurisdiction",
                "DiscountDescription", "DiscountType", "DiscountAmount",
                "PaymentMethod", "ProjectDetail", "CustomerNotes", "InternalNotes",
                "TermsAndConditions", "FooterNote", "IsAutomated"
            };

            var headerRow = sheet.CreateRow(0);
            var requiredStyle = CreateRequiredStyle(workbook);
            var optionalStyle = CreateOptionalStyle(workbook);

            var requiredColumns = new[] { 0, 1, 4, 5, 6, 7, 8, 9, 10 };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = requiredColumns.Contains(i) ? requiredStyle : optionalStyle;

                // Add comment
                var comment = sheet.CreateDrawingPatriarch().CreateCellComment(
                    new XSSFClientAnchor(0, 0, 0, 0, i, 0, i + 2, 4));
                comment.String = new XSSFRichTextString(GetCommentForHeader(headers[i]));
                cell.CellComment = comment;
            }

            // Add sample row
            var sampleRow = sheet.CreateRow(1);
            sampleRow.CreateCell(0).SetCellValue(1); // CustomerId
            sampleRow.CreateCell(1).SetCellValue("INV-2024-1001"); // InvoiceNumber
            sampleRow.CreateCell(3).SetCellValue("PO-12345"); // PONumber
            sampleRow.CreateCell(4).SetCellValue(DateTime.Now.ToString("yyyy-MM-dd")); // IssueDate
            sampleRow.CreateCell(5).SetCellValue(DateTime.Now.AddDays(30).ToString("yyyy-MM-dd")); // DueDate
            sampleRow.CreateCell(6).SetCellValue("Standard"); // InvoiceType
            sampleRow.CreateCell(7).SetCellValue("USD"); // Currency
            sampleRow.CreateCell(8).SetCellValue("Consulting Services"); // ItemDescription
            sampleRow.CreateCell(9).SetCellValue(10); // ItemQuantity
            sampleRow.CreateCell(10).SetCellValue(150.00); // ItemUnitPrice
            sampleRow.CreateCell(11).SetCellValue("GST"); // ItemTaxType
            sampleRow.CreateCell(12).SetCellValue("GST"); // TaxName
            sampleRow.CreateCell(13).SetCellValue(18.00); // TaxRate
            sampleRow.CreateCell(14).SetCellValue("Federal"); // TaxJurisdiction
            sampleRow.CreateCell(15).SetCellValue("Early Payment"); // DiscountDescription
            sampleRow.CreateCell(16).SetCellValue("Percentage"); // DiscountType
            sampleRow.CreateCell(17).SetCellValue(5.00); // DiscountAmount
            sampleRow.CreateCell(18).SetCellValue("Bank Transfer"); // PaymentMethod
            sampleRow.CreateCell(19).SetCellValue("Project Alpha"); // ProjectDetail
            sampleRow.CreateCell(20).SetCellValue("Thank you for your business"); // CustomerNotes
            sampleRow.CreateCell(21).SetCellValue("Internal note"); // InternalNotes
            sampleRow.CreateCell(22).SetCellValue("Net 30"); // TermsAndConditions
            sampleRow.CreateCell(23).SetCellValue("Powered by Core API"); // FooterNote
            sampleRow.CreateCell(24).SetCellValue("No"); // IsAutomated

            // Auto-size columns
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private void CreateInstructionsSheet(IWorkbook workbook)
        {
            var sheet = workbook.CreateSheet("Instructions");

            var titleRow = sheet.CreateRow(0);
            var titleCell = titleRow.CreateCell(0);
            titleCell.SetCellValue("Invoice Import Instructions");

            var titleStyle = workbook.CreateCellStyle();
            var titleFont = workbook.CreateFont();
            titleFont.IsBold = true;
            titleFont.FontHeightInPoints = 14;
            titleFont.Color = HSSFColor.DarkBlue.Index;
            titleStyle.SetFont(titleFont);
            titleCell.CellStyle = titleStyle;

            int rowIndex = 2;
            var instructions = new[]
            {
                "This template is used to import invoices into the system.",
                "Please follow these guidelines:",
                "1. Fields marked with * are required",
                "2. Use the first row as a sample - replace it with your data",
                "3. Do not modify or delete the header row",
                "4. Each row represents one invoice with one line item",
                "5. For multiple line items, create multiple rows with the same invoice number",
                "6. Dates must be in YYYY-MM-DD format",
                "7. Currency must be a valid ISO code (USD, EUR, INR, etc.)",
                "8. InvoiceType must be one of: Standard, Recurring, Proforma, CreditNote",
                "9. PaymentMethod can be: Bank Transfer, Credit Card, PayPal, Cash, Check",
                "10. Import will validate all data before processing"
            };

            foreach (var instruction in instructions)
            {
                var row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(instruction);
            }

            // Field descriptions table
            rowIndex += 2;
            var tableHeaderRow = sheet.CreateRow(rowIndex++);
            var tableHeaders = new[] { "Field", "Required", "Description", "Valid Values/Format", "Example" };
            var tableHeaderStyle = CreateHeaderStyle(workbook);

            for (int i = 0; i < tableHeaders.Length; i++)
            {
                var cell = tableHeaderRow.CreateCell(i);
                cell.SetCellValue(tableHeaders[i]);
                cell.CellStyle = tableHeaderStyle;
            }

            var fieldDescriptions = new[]
            {
                new[] { "CustomerId", "Yes", "ID of existing customer", "Positive integer", "1" },
                new[] { "InvoiceNumber", "Yes", "Unique invoice identifier", "Max 50 chars", "INV-2024-1001" },
                new[] { "PONumber", "No", "Purchase order number", "Max 50 chars", "PO-12345" },
                new[] { "IssueDate", "Yes", "Date invoice issued", "YYYY-MM-DD", "2024-01-15" },
                new[] { "DueDate", "Yes", "Date payment due", "YYYY-MM-DD", "2024-02-14" },
                new[] { "InvoiceType", "Yes", "Type of invoice", "Standard, Recurring, Proforma, CreditNote", "Standard" },
                new[] { "Currency", "Yes", "Currency code", "USD, EUR, INR, GBP, etc.", "USD" },
                new[] { "ItemDescription", "Yes", "Description of item/service", "Max 500 chars", "Consulting Services" },
                new[] { "ItemQuantity", "Yes", "Quantity of item", "Positive number", "10" },
                new[] { "ItemUnitPrice", "Yes", "Price per unit", "Positive decimal", "150.00" },
                new[] { "ItemTaxType", "No", "Tax type for item", "Must exist in system", "GST" },
                new[] { "TaxName", "No", "Name of tax", "Max 50 chars", "GST" },
                new[] { "TaxRate", "No", "Tax rate percentage", "0-100", "18.00" },
                new[] { "TaxJurisdiction", "No", "Tax jurisdiction", "e.g., Federal, State", "Federal" },
                new[] { "DiscountDescription", "No", "Description of discount", "Max 50 chars", "Early Payment" },
                new[] { "DiscountType", "No", "Type of discount", "Percentage, Fixed", "Percentage" },
                new[] { "DiscountAmount", "No", "Discount value", "Positive decimal", "5.00" },
                new[] { "PaymentMethod", "No", "Method of payment", "Bank Transfer, Credit Card, etc.", "Bank Transfer" },
                new[] { "ProjectDetail", "No", "Project reference", "Max 200 chars", "Project Alpha" },
                new[] { "CustomerNotes", "No", "Notes visible to customer", "Max 1000 chars", "Thank you" },
                new[] { "InternalNotes", "No", "Internal notes", "Max 1000 chars", "Internal note" },
                new[] { "TermsAndConditions", "No", "Terms text", "Max 500 chars", "Net 30" },
                new[] { "FooterNote", "No", "Footer text", "Max 500 chars", "Powered by Core API" },
                new[] { "IsAutomated", "No", "Auto-generated flag", "Yes or No", "No" }
            };

            foreach (var desc in fieldDescriptions)
            {
                var row = sheet.CreateRow(rowIndex++);
                for (int i = 0; i < desc.Length; i++)
                {
                    row.CreateCell(i).SetCellValue(desc[i]);
                }
            }

            // Auto-size columns
            for (int i = 0; i < tableHeaders.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        private ICellStyle CreateRequiredStyle(IWorkbook workbook)
        {
            var font = workbook.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            font.Color = HSSFColor.DarkBlue.Index;

            var style = workbook.CreateCellStyle();
            style.SetFont(font);
            style.FillForegroundColor = HSSFColor.LightTurquoise.Index;
            style.FillPattern = FillPattern.SolidForeground;
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;

            return style;
        }

        private ICellStyle CreateOptionalStyle(IWorkbook workbook)
        {
            var font = workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";

            var style = workbook.CreateCellStyle();
            style.SetFont(font);
            style.FillForegroundColor = HSSFColor.Grey25Percent.Index;
            style.FillPattern = FillPattern.SolidForeground;
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;

            return style;
        }

        private static string GetCommentForHeader(string header)
        {
            return header switch
            {
                "CustomerId*" => "Required. ID of existing customer in system",
                "InvoiceNumber*" => "Required. Unique invoice number",
                "IssueDate*" => "Required. Format: YYYY-MM-DD",
                "DueDate*" => "Required. Format: YYYY-MM-DD",
                "InvoiceType*" => "Required. Standard, Recurring, Proforma, or CreditNote",
                "Currency*" => "Required. ISO currency code (USD, EUR, INR, etc.)",
                "ItemDescription*" => "Required. Description of the product/service",
                "ItemQuantity*" => "Required. Quantity (positive number)",
                "ItemUnitPrice*" => "Required. Price per unit",
                "TaxName" => "Name of tax (e.g., GST, VAT, Sales Tax)",
                "TaxRate" => "Tax rate percentage (0-100)",
                "TaxJurisdiction" => "Jurisdiction where tax applies",
                "DiscountType" => "Percentage or Fixed",
                "PaymentMethod" => "Method of payment",
                "ProjectDetail" => "Project reference",
                "CustomerNotes" => "Notes visible to customer",
                _ => "Optional field"
            };
        }
    }
}