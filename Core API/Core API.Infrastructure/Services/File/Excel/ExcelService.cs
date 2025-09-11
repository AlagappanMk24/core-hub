using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.File.Excel;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Core_API.Infrastructure.Services.File.Excel
{
    public class ExcelService(IUnitOfWork unitOfWork, ILogger<ExcelService> logger) : IExcelService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<ExcelService> _logger = logger;
        public async Task<OperationResult<byte[]>> ExportInvoicesExcelAsync(OperationContext operationContext, InvoiceFilterRequestDto invoiceFilterRequestDto)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for exporting invoices");
                    return OperationResult<byte[]>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                var result = await _unitOfWork.Invoices.GetPagedAsync(companyId, invoiceFilterRequestDto);

                if (result.Items.Count == 0)
                {
                    return OperationResult<byte[]>.FailureResult("No invoices found for export.");
                }

                IWorkbook workbook = new XSSFWorkbook();

                // --- Create Invoice Sheet ---
                ISheet invoiceSheet = workbook.CreateSheet("Invoices");

                // Define invoice headers dynamically
                List<string> invoiceHeaders =
                [
                    "Invoice Number", "PO Number", "Issue Date", "Due Date", "Status", "Invoice Type",
                    "Customer Name", "Customer Email", "Customer Phone",
                    "Subtotal", "Tax", "Total Amount", "Currency",
                    "Notes", "Payment Method", "Project Detail", "Is Automated"
                ];

                IRow invoiceHeaderRow = invoiceSheet.CreateRow(0);
                ICellStyle headerStyle = GetHeaderStyle(workbook);

                for (int i = 0; i < invoiceHeaders.Count; i++)
                {
                    ICell cell = invoiceHeaderRow.CreateCell(i);
                    cell.SetCellValue(invoiceHeaders[i]);
                    cell.CellStyle = headerStyle;
                }

                // Apply date format style
                ICellStyle dateStyle = workbook.CreateCellStyle();
                dateStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd-MMM-yyyy");

                int invoiceRowIdx = 1; // Start from row 1 for invoice data
                foreach (var invoice in result.Items)
                {
                    IRow dataRow = invoiceSheet.CreateRow(invoiceRowIdx);

                    int colIdx = 0;
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.InvoiceNumber);
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.PONumber);

                    ICell issueDateCell = dataRow.CreateCell(colIdx++);
                    issueDateCell.SetCellValue(invoice.IssueDate);
                    issueDateCell.CellStyle = dateStyle;

                    ICell dueDateCell = dataRow.CreateCell(colIdx++);
                    dueDateCell.SetCellValue(invoice.PaymentDue);
                    dueDateCell.CellStyle = dateStyle;

                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.InvoiceStatus.ToString());
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.InvoiceType.ToString());

                    // Customer Details
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.Customer?.Name ?? "Unknown");
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.Customer?.Email ?? string.Empty);
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.Customer?.PhoneNumber ?? string.Empty);

                    dataRow.CreateCell(colIdx++).SetCellValue((double)invoice.Subtotal); // Store as number for calculation
                    dataRow.CreateCell(colIdx++).SetCellValue((double)invoice.Tax); // Store as number
                    dataRow.CreateCell(colIdx++).SetCellValue((double)invoice.TotalAmount); // Store as number
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.Currency);

                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.Notes ?? string.Empty);
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.PaymentMethod ?? string.Empty);
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.ProjectDetail ?? string.Empty);
                    dataRow.CreateCell(colIdx++).SetCellValue(invoice.IsAutomated ? "Yes" : "No");

                    invoiceRowIdx++;
                }

                // Auto-size columns for invoice sheet
                for (int i = 0; i < invoiceHeaders.Count; i++)
                {
                    invoiceSheet.AutoSizeColumn(i);
                }

                // --- Create Invoice Items Sheet ---
                if (result.Items.Any(inv => inv.InvoiceItems != null && inv.InvoiceItems.Count != 0))
                {
                    ISheet itemsSheet = workbook.CreateSheet("Invoice Items");
                    List<string> itemHeaders =
                    [
                        "Invoice Number", "Description", "Quantity", "Unit Price", "Amount", "Tax Type (Item)", "Tax Amount (Item)"
                    ];

                    IRow itemHeaderRow = itemsSheet.CreateRow(0);
                    for (int i = 0; i < itemHeaders.Count; i++)
                    {
                        ICell cell = itemHeaderRow.CreateCell(i);
                        cell.SetCellValue(itemHeaders[i]);
                        cell.CellStyle = headerStyle;
                    }

                    int itemRowIdx = 1;
                    foreach (var invoice in result.Items)
                    {
                        if (invoice.InvoiceItems != null)
                        {
                            foreach (var item in invoice.InvoiceItems)
                            {
                                IRow itemDataRow = itemsSheet.CreateRow(itemRowIdx);
                                int itemColIdx = 0;
                                itemDataRow.CreateCell(itemColIdx++).SetCellValue(invoice.InvoiceNumber); // Link to parent invoice
                                itemDataRow.CreateCell(itemColIdx++).SetCellValue(item.Description);
                                itemDataRow.CreateCell(itemColIdx++).SetCellValue(item.Quantity);
                                itemDataRow.CreateCell(itemColIdx++).SetCellValue((double)item.UnitPrice);
                                itemDataRow.CreateCell(itemColIdx++).SetCellValue((double)item.Amount);
                                itemDataRow.CreateCell(itemColIdx++).SetCellValue(item.TaxType ?? string.Empty);
                                itemDataRow.CreateCell(itemColIdx++).SetCellValue((double)item.TaxAmount);
                                itemRowIdx++;
                            }
                        }
                    }
                    // Auto-size columns for invoice items sheet
                    for (int i = 0; i < itemHeaders.Count; i++)
                    {
                        itemsSheet.AutoSizeColumn(i);
                    }
                }


                // --- Create Tax Details Sheet ---
                if (result.Items.Any(inv => inv.TaxDetails != null && inv.TaxDetails.Count != 0))
                {
                    ISheet taxSheet = workbook.CreateSheet("Tax Details");
                    List<string> taxHeaders =
                    [
                        "Invoice Number", "Tax Type", "Rate (%)", "Amount"
                    ];

                    IRow taxHeaderRow = taxSheet.CreateRow(0);
                    for (int i = 0; i < taxHeaders.Count; i++)
                    {
                        ICell cell = taxHeaderRow.CreateCell(i);
                        cell.SetCellValue(taxHeaders[i]);
                        cell.CellStyle = headerStyle;
                    }

                    int taxRowIdx = 1;
                    foreach (var invoice in result.Items)
                    {
                        if (invoice.TaxDetails != null)
                        {
                            foreach (var taxDetail in invoice.TaxDetails)
                            {
                                IRow taxDataRow = taxSheet.CreateRow(taxRowIdx);
                                int taxColIdx = 0;
                                taxDataRow.CreateCell(taxColIdx++).SetCellValue(invoice.InvoiceNumber); // Link to parent invoice
                                taxDataRow.CreateCell(taxColIdx++).SetCellValue(taxDetail.TaxType);
                                taxDataRow.CreateCell(taxColIdx++).SetCellValue((double)taxDetail.Rate);
                                taxDataRow.CreateCell(taxColIdx++).SetCellValue((double)taxDetail.Amount);
                                taxRowIdx++;
                            }
                        }
                    }
                    // Auto-size columns for tax details sheet
                    for (int i = 0; i < taxHeaders.Count; i++)
                    {
                        taxSheet.AutoSizeColumn(i);
                    }
                }


                // --- Create Discounts Sheet ---
                if (result.Items.Any(inv => inv.Discounts != null && inv.Discounts.Count != 0))
                {
                    ISheet discountSheet = workbook.CreateSheet("Discounts");
                    List<string> discountHeaders =
                    [
                        "Invoice Number", "Description", "Amount", "IsPercentage"
                    ];

                    IRow discountHeaderRow = discountSheet.CreateRow(0);
                    for (int i = 0; i < discountHeaders.Count; i++)
                    {
                        ICell cell = discountHeaderRow.CreateCell(i);
                        cell.SetCellValue(discountHeaders[i]);
                        cell.CellStyle = headerStyle;
                    }

                    int discountRowIdx = 1;
                    foreach (var invoice in result.Items)
                    {
                        if (invoice.Discounts != null)
                        {
                            foreach (var discount in invoice.Discounts)
                            {
                                IRow discountDataRow = discountSheet.CreateRow(discountRowIdx);
                                int discountColIdx = 0;
                                discountDataRow.CreateCell(discountColIdx++).SetCellValue(invoice.InvoiceNumber); // Link to parent invoice
                                discountDataRow.CreateCell(discountColIdx++).SetCellValue(discount.Description);
                                discountDataRow.CreateCell(discountColIdx++).SetCellValue((double)discount.Amount);
                                discountDataRow.CreateCell(discountColIdx++).SetCellValue(discount.IsPercentage);
                                discountRowIdx++;
                            }
                        }
                    }
                    // Auto-size columns for discounts sheet
                    for (int i = 0; i < discountHeaders.Count; i++)
                    {
                        discountSheet.AutoSizeColumn(i);
                    }
                }

                // Write the workbook to a MemoryStream
                using var stream = new MemoryStream();
                workbook.Write(stream);
                return OperationResult<byte[]>.SuccessResult(stream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices to Excel for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<byte[]>.FailureResult("Failed to export invoices to Excel.");
            }
        }

        // Helper method for common header styling
        private static ICellStyle GetHeaderStyle(IWorkbook workbook)
        {
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;

            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.SetFont(headerFont);
            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            return headerStyle;
        }


        public byte[] GenerateImportTemplate()
        {
            using var workbook = new XSSFWorkbook();

            // Create Invoices sheet
            var dataSheet = workbook.CreateSheet("Invoices");
            var headerRow = dataSheet.CreateRow(0);
            var headers = new[]
            {
                "CustomerId", "InvoiceNumber", "Type", "Status", "IssueDate", "DueDate", "Currency",
                "PONumber", "ProjectDetail", "PaymentMethod", "Notes",
                "ItemDescription", "ItemQuantity", "ItemUnitPrice", "ItemTaxType", "ItemTaxAmount",
                "TaxType", "TaxRate", "TaxAmount",
                "DiscountDescription", "DiscountAmount", "DiscountIsPercentage"
            };

            // Styles for headers
            var requiredStyle = workbook.CreateCellStyle();
            //requiredStyle.FillForegroundColor = IndexedColors.Red.Index;
            requiredStyle.FillForegroundColor = IndexedColors.LightTurquoise.Index;
            requiredStyle.FillPattern = FillPattern.SolidForeground;

            var font = workbook.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 11;
            font.FontName = "Calibri";
            //font.Color = IndexedColors.White.Index;
            font.Color = IndexedColors.DarkBlue.Index;
            requiredStyle.SetFont(font);

            var optionalStyle = workbook.CreateCellStyle();
            optionalStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            optionalStyle.FillPattern = FillPattern.SolidForeground;
            optionalStyle.SetFont(font);

            // Required columns indices
            var requiredColumns = new[] { 0, 1, 2, 3, 4, 5, 6, 11, 12, 13 };

            // Set headers and comments
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                string headerText = headers[i];
                if (requiredColumns.Contains(i))
                {
                    headerText += "*"; // Add asterisk for required fields
                }
                cell.SetCellValue(headers[i]);
                cell.CellStyle = requiredColumns.Contains(i) ? requiredStyle : optionalStyle;

                // Add comment
                var comment = dataSheet.CreateDrawingPatriarch().CreateCellComment(
                    new XSSFClientAnchor(0, 0, 0, 0, i, 0, i + 2, 4));
                comment.String = new XSSFRichTextString(GetCommentForHeader(headers[i]));
                cell.CellComment = comment;
            }

            // Add sample row
            var sampleRow = dataSheet.CreateRow(1);
            sampleRow.CreateCell(0).SetCellValue(1);
            sampleRow.CreateCell(1).SetCellValue("INV001");
            sampleRow.CreateCell(2).SetCellValue("Standard");
            sampleRow.CreateCell(3).SetCellValue("Draft");
            sampleRow.CreateCell(4).SetCellValue("2025-07-01");
            sampleRow.CreateCell(5).SetCellValue("2025-07-31");
            sampleRow.CreateCell(6).SetCellValue("USD");
            sampleRow.CreateCell(7).SetCellValue("PO123");
            sampleRow.CreateCell(8).SetCellValue("ProjectX");
            sampleRow.CreateCell(9).SetCellValue("CreditCard");
            sampleRow.CreateCell(10).SetCellValue("Payment terms: 30 days");
            sampleRow.CreateCell(11).SetCellValue("Service Fee");
            sampleRow.CreateCell(12).SetCellValue(2);
            sampleRow.CreateCell(13).SetCellValue(100.00);
            sampleRow.CreateCell(14).SetCellValue("GST");
            sampleRow.CreateCell(15).SetCellValue(10.00);
            sampleRow.CreateCell(16).SetCellValue("GST");
            sampleRow.CreateCell(17).SetCellValue(10.00);
            sampleRow.CreateCell(18).SetCellValue(10.00);
            sampleRow.CreateCell(19).SetCellValue("Early Payment");
            sampleRow.CreateCell(20).SetCellValue(5.00);
            sampleRow.CreateCell(21).SetCellValue(false);

            // Auto-size columns
            for (int i = 0; i < headers.Length; i++)
            {
                dataSheet.AutoSizeColumn(i);
            }

            // Create Instructions sheet
            var instructionsSheet = workbook.CreateSheet("Instructions");
            var titleRow = instructionsSheet.CreateRow(0);
            var titleCell = titleRow.CreateCell(0);
            titleCell.SetCellValue("Instructions for Importing Invoices");
            var titleStyle = workbook.CreateCellStyle();
            var titleFont = workbook.CreateFont();
            titleFont.IsBold = true;
            titleFont.FontHeightInPoints = 14;
            titleFont.Color = IndexedColors.DarkBlue.Index; // Violet-like color
            titleStyle.SetFont(titleFont);
            titleCell.CellStyle = titleStyle;

            var rowIndex = 2;
            var instructions = new[]
            {
                "This template is used to import invoices into the invoice management system.",
                "Fill out the 'Invoices' sheet with your invoice data, following the guidelines below:",
                "- Save the file as .xlsx format.",
                "- Do not modify or delete the column headers in the 'Invoices' sheet.",
                "- Each row in the 'Invoices' sheet represents one invoice with one item, tax detail, and discount (if applicable).",
                "- Required fields in the 'Invoices' sheet are marked with a light blue background and an asterisk (*)."
            };

            foreach (var instruction in instructions)
            {
                var row = instructionsSheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(instruction);
            }

            // Field descriptions table
            var tableHeaders = new[] { "Column Name", "Required", "Description", "Valid Values", "Example" };
            var tableHeaderRow = instructionsSheet.CreateRow(rowIndex++);
            var tableHeaderStyle = workbook.CreateCellStyle();
            tableHeaderStyle.FillForegroundColor = IndexedColors.LightYellow.Index; // A light yellow for table headers
            tableHeaderStyle.FillPattern = FillPattern.SolidForeground;
            tableHeaderStyle.SetFont(font); // Use the same bold font
            for (int i = 0; i < tableHeaders.Length; i++)
            {
                var cell = tableHeaderRow.CreateCell(i);
                cell.SetCellValue(tableHeaders[i]);
                cell.CellStyle = tableHeaderStyle;
            }

            var fieldDescriptions = new[]
            {
                ["CustomerId", "Yes (*)", "ID of the customer", "Positive integer, must exist in database", "1"],
                ["InvoiceNumber", "Yes (*)", "Unique invoice number", "Up to 50 characters, unique per company", "INV001"],
                ["Type", "Yes (*)", "Invoice type", "Standard, Proforma", "Standard"],
                ["Status", "Yes (*)", "Invoice status", "Draft, Sent, Paid, Overdue, PartiallyPaid, Void", "Draft"],
                ["IssueDate", "Yes (*)", "Issue date of the invoice", "YYYY-MM-DD format", "2025-07-01"],
                ["DueDate", "Yes (*)", "Due date of the invoice", "YYYY-MM-DD, must be >= IssueDate", "2025-07-31"],
                ["Currency", "Yes (*)", "Currency code", "USD, EUR, INR", "USD"],
                ["PONumber", "No", "Purchase order number", "Up to 50 characters", "PO123"],
                ["ProjectDetail", "No", "Project details", "Up to 500 characters", "ProjectX"],
                ["PaymentMethod", "No", "Payment method", "Up to 100 characters", "CreditCard"],
                ["Notes", "No", "Additional notes", "Up to 500 characters", "Payment terms"],
                ["ItemDescription", "Yes (*)", "Description of the invoice item", "Up to 500 characters", "Service Fee"],
                ["ItemQuantity", "Yes (*)", "Quantity of the item", "Positive integer (>= 1)", "2"],
                ["ItemUnitPrice", "Yes (*)", "Unit price of the item", "Non-negative decimal", "100.00"],
                ["ItemTaxType", "No", "Tax type for the item", "Up to 50 chars, must exist if provided", "GST"],
                ["ItemTaxAmount", "No", "Tax amount for the item", "Non-negative decimal if ItemTaxType provided", "10.00"],
                ["TaxType", "No", "Tax type for the invoice", "Up to 50 chars, must exist if provided", "GST"],
                ["TaxRate", "No", "Tax rate for the invoice", "0–100% if TaxType provided", "10.00"],
                ["TaxAmount", "No", "Tax amount for the invoice", "Non-negative decimal if TaxType provided", "10.00"],
                ["DiscountDescription", "No", "Description of the discount", "Up to 50 chars if provided", "Early Payment"],
                ["DiscountAmount", "No", "Discount amount", "Non-negative decimal if DiscountDescription provided", "5.00"],
                new[] { "DiscountIsPercentage", "No", "Whether discount is a percentage", "TRUE or FALSE if DiscountDescription provided", "FALSE" }
            };

            foreach (var desc in fieldDescriptions)
            {
                var row = instructionsSheet.CreateRow(rowIndex++);
                for (int i = 0; i < desc.Length; i++)
                {
                    row.CreateCell(i).SetCellValue(desc[i]);
                }
            }

            // Tips
            rowIndex++;
            var tipsHeaderRow = instructionsSheet.CreateRow(rowIndex++);
            tipsHeaderRow.CreateCell(0).SetCellValue("Additional Tips");
            tipsHeaderRow.Cells[0].CellStyle = titleStyle;

            var tips = new[]
            {
                "Use the provided sample row in the 'Invoices' sheet as a guide.",
                "Ensure CustomerId matches an existing customer in your company.",
                "Check import results for errors and correct the file as needed.",
                "Contact support for assistance."
            };

            foreach (var tip in tips)
            {
                var row = instructionsSheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(tip);
            }

            // Auto-size columns in Instructions sheet
            for (int i = 0; i < tableHeaders.Length; i++)
            {
                instructionsSheet.AutoSizeColumn(i);
            }

            // Write to memory stream
            using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            return memoryStream.ToArray();
        }
        private static string GetCommentForHeader(string header)
        {
            return header switch
            {
                "CustomerId" => "Required. Enter the ID of an existing customer.",
                "InvoiceNumber" => "Required. Unique invoice number (max 50 chars).",
                "Type" => "Required. Must be 'Standard' or 'Proforma'.",
                "Status" => "Required. Must be 'Draft', 'Sent', 'Paid', 'Overdue', 'PartiallyPaid', or 'Void'.",
                "IssueDate" => "Required. Format: YYYY-MM-DD (e.g., 2025-07-01).",
                "DueDate" => "Required. Format: YYYY-MM-DD. Must be >= IssueDate.",
                "Currency" => "Required. Must be 'USD', 'EUR', or 'INR'.",
                "PONumber" => "Optional. Purchase order number (max 50 chars).",
                "ProjectDetail" => "Optional. Project details (max 500 chars).",
                "PaymentMethod" => "Optional. Payment method (max 100 chars).",
                "Notes" => "Optional. Additional notes (max 500 chars).",
                "ItemDescription" => "Required. Description of the item (max 500 chars).",
                "ItemQuantity" => "Required. Positive integer (>= 1).",
                "ItemUnitPrice" => "Required. Non-negative decimal (e.g., 100.00).",
                "ItemTaxType" => "Optional. Tax type (max 50 chars; must exist if provided).",
                "ItemTaxAmount" => "Optional. Non-negative decimal if ItemTaxType is provided.",
                "TaxType" => "Optional. Tax type (max 50 chars; must exist if provided).",
                "TaxRate" => "Optional. 0–100% if TaxType is provided.",
                "TaxAmount" => "Optional. Non-negative decimal if TaxType is provided.",
                "DiscountDescription" => "Optional. Discount description (max 50 chars).",
                "DiscountAmount" => "Optional. Non-negative decimal if DiscountDescription provided.",
                "DiscountIsPercentage" => "Optional. TRUE or FALSE if DiscountDescription provided.",
                _ => ""
            };
        }
    }
}
