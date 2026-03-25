using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.Globalization;
using System.Text;

namespace Core_API.Infrastructure.Services.File.Pdf
{
    public class PdfService : IPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyService _companyService;
        private readonly ILogger<PdfService> _logger;
        private readonly IMapper _mapper;

        public PdfService(IUnitOfWork unitOfWork, ICompanyService companyService, ILogger<PdfService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            // Initialize FontResolver
            if (GlobalFontSettings.FontResolver == null)
            {
                var fontDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts");
                GlobalFontSettings.FontResolver = new FontResolver(fontDirectory);
            }
        }

        public async Task<OperationResult<InvoiceResponseDto>> GenerateInvoicePdfAsync(int id, OperationContext operationContext)
        {
            try
            {
                // Fetch invoice with all related entities
                var invoice = await _unitOfWork.Invoices.GetAsync(
                    i => i.Id == id && i.CompanyId == operationContext.CompanyId && !i.IsDeleted,
                    includeProperties: "Customer,InvoiceItems,TaxDetails,Discounts,Payments,InvoiceAttachments"
                );
                if (invoice == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Generating PDF
                var pdfStream = await GenerateInvoicePdf(invoice);
                if (pdfStream == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Failed to generate PDF.");
                }

                // Mapping invoice to response DTO
                var response = _mapper.Map<InvoiceResponseDto>(invoice);
                response.Customer = _mapper.Map<CustomerResponseDto>(invoice.Customer);
                response.PdfStream = pdfStream;

                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to generate invoice PDF.");
            }
        }

        public async Task<OperationResult<byte[]>> ExportInvoicesPdfAsync(OperationContext operationContext, InvoiceFilterRequestDto invoiceFilterRequestDto)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for exporting invoice pdf.");
                    return OperationResult<byte[]>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                var result = await _unitOfWork.Invoices.GetPagedAsync(companyId, invoiceFilterRequestDto);
                if (result.Items.Count == 0)
                {
                    return OperationResult<byte[]>.FailureResult("No invoices found for export.");
                }

                using var document = new PdfDocument();

                foreach (var invoice in result.Items)
                {
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    await GenerateInvoicePdfContent(gfx, page, invoice);
                }

                using var memoryStream = new MemoryStream();
                document.Save(memoryStream);
                return OperationResult<byte[]>.SuccessResult(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices to PDF for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<byte[]>.FailureResult("Failed to export invoices to PDF.");
            }
        }

        private async Task<MemoryStream> GenerateInvoicePdf(Invoice invoice)
        {
            try
            {
                using var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                await GenerateInvoicePdfContent(gfx, page, invoice);

                var stream = new MemoryStream();
                document.Save(stream, false);
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}", invoice.Id);
                return null;
            }
        }

        private async Task GenerateInvoicePdfContent(XGraphics gfx, PdfPage page, Invoice invoice)
        {
            // Font definitions
            var fontTitle = new XFont("Open Sans", 24, XFontStyleEx.Bold);
            var fontHeader = new XFont("Open Sans", 12, XFontStyleEx.Bold);
            var fontRegular = new XFont("Open Sans", 10, XFontStyleEx.Regular);
            var fontBold = new XFont("Open Sans", 10, XFontStyleEx.Bold);
            var fontSmall = new XFont("Open Sans", 8, XFontStyleEx.Regular);
            var fontLargeBold = new XFont("Open Sans", 20, XFontStyleEx.Bold);

            double y = 30;
            const double margin = 40;
            const double lineHeight = 16;
            const double sectionSpacing = 25;

            // Color scheme
            var primaryViolet = XColor.FromArgb(167, 139, 250);
            var secondaryViolet = XColor.FromArgb(196, 181, 253);
            var lightViolet = XColor.FromArgb(230, 220, 255);
            var darkGray = XColor.FromArgb(64, 64, 64);
            var lightGray = XColor.FromArgb(128, 128, 128);
            var accentWhite = XColor.FromArgb(255, 255, 255);
            var successGreen = XColor.FromArgb(40, 167, 69);
            var warningOrange = XColor.FromArgb(255, 193, 7);
            var dangerRed = XColor.FromArgb(220, 53, 69);

            // Brushes
            var primaryVioletBrush = new XSolidBrush(primaryViolet);
            var secondaryVioletBrush = new XSolidBrush(secondaryViolet);
            var lightVioletBrush = new XSolidBrush(lightViolet);
            var darkGrayBrush = new XSolidBrush(darkGray);
            var lightGrayBrush = new XSolidBrush(lightGray);
            var accentWhiteBrush = new XSolidBrush(accentWhite);
            var successGreenBrush = new XSolidBrush(successGreen);
            var warningOrangeBrush = new XSolidBrush(warningOrange);
            var dangerRedBrush = new XSolidBrush(dangerRed);
            var blackBrush = XBrushes.Black;

            // Get culture for currency formatting
            var culture = invoice.Currency switch
            {
                "EUR" => new CultureInfo("de-DE"),
                "INR" => new CultureInfo("hi-IN"),
                "GBP" => new CultureInfo("en-GB"),
                "JPY" => new CultureInfo("ja-JP"),
                "CAD" => new CultureInfo("en-CA"),
                "AUD" => new CultureInfo("en-AU"),
                _ => new CultureInfo("en-US")
            };

            // Get company info
            var companyInfo = await _companyService.GetCompanyByIdAsync(invoice.CompanyId);

            // ===== HEADER SECTION =====
            // Left side - Company Info
            double companyY = y + 10;

            // Company Name
            gfx.DrawString(companyInfo.Name, new XFont("Open Sans", 16, XFontStyleEx.Bold), primaryVioletBrush,
                new XRect(margin, y, 300, 20), XStringFormats.TopLeft);
            companyY += lineHeight;

            // Company Tagline (if exists)
            if (!string.IsNullOrEmpty(companyInfo?.TaxId))
            {
                gfx.DrawString($"Tax ID: {companyInfo.TaxId}", fontSmall, lightGrayBrush,
                    new XRect(margin, y + 20, 200, 12), XStringFormats.TopLeft);
            }
            companyY += lineHeight;

            // Company Address
            if (companyInfo?.Address != null)
            {
                var address = $"{companyInfo.Address.Address1 ?? ""} {companyInfo.Address.Address2 ?? ""}".Trim();
                if (!string.IsNullOrEmpty(address))
                {
                    gfx.DrawString(address, fontSmall, darkGrayBrush,
                        new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                    companyY += lineHeight;
                }

                var cityStateZip = $"{companyInfo.Address.City ?? ""}, {companyInfo.Address.State ?? ""} {companyInfo.Address.ZipCode ?? ""}".Trim(',', ' ');
                if (!string.IsNullOrEmpty(cityStateZip))
                {
                    gfx.DrawString(cityStateZip, fontSmall, darkGrayBrush,
                        new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                    companyY += lineHeight;
                }

                if (!string.IsNullOrEmpty(companyInfo.Address.Country))
                {
                    gfx.DrawString(companyInfo.Address.Country, fontSmall, darkGrayBrush,
                        new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                    companyY += lineHeight;
                }
            }

            // Company Contact
            if (!string.IsNullOrEmpty(companyInfo?.Email))
            {
                gfx.DrawString($"Email: {companyInfo.Email}", fontSmall, darkGrayBrush,
                    new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                companyY += lineHeight;
            }

            if (!string.IsNullOrEmpty(companyInfo?.PhoneNumber))
            {
                gfx.DrawString($"Phone: {companyInfo.PhoneNumber}", fontSmall, darkGrayBrush,
                    new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                companyY += lineHeight;
            }

            // Right side - Invoice Title and Details
            var invoiceHeaderX = page.Width - margin - 200;

            // Invoice Type Badge
            var invoiceTypeBg = invoice.InvoiceType switch
            {
                InvoiceType.Recurring => secondaryVioletBrush,
                InvoiceType.Proforma => warningOrangeBrush,
                InvoiceType.CreditNote => successGreenBrush,
                InvoiceType.DebitNote => dangerRedBrush,
                _ => primaryVioletBrush
            };

            gfx.DrawRectangle(invoiceTypeBg, invoiceHeaderX, y, 200, 30);
            gfx.DrawString(invoice.InvoiceType.ToString().ToUpper(), fontHeader, accentWhiteBrush,
                new XRect(invoiceHeaderX, y + 7, 200, 20), XStringFormats.TopCenter);

            y += 40;

            // Invoice Number
            gfx.DrawString($"INVOICE #{invoice.InvoiceNumber}", fontLargeBold, blackBrush,
                new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
            y += lineHeight + 5;

            // Dates
            gfx.DrawString($"Issue Date: {invoice.IssueDate:dd MMM yyyy}", fontRegular, blackBrush,
                new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
            y += lineHeight;

            gfx.DrawString($"Due Date: {invoice.DueDate:dd MMM yyyy}", fontRegular, invoice.PaymentStatus == PaymentStatus.Overdue ? dangerRedBrush : blackBrush,
                new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
            y += lineHeight;

            // Sent Date (if available)
            if (invoice.SentDate.HasValue)
            {
                gfx.DrawString($"Sent Date: {invoice.SentDate.Value:dd MMM yyyy}", fontSmall, lightGrayBrush,
                    new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
                y += lineHeight;
            }

            // Paid Date (if available)
            if (invoice.PaidDate.HasValue)
            {
                gfx.DrawString($"Paid Date: {invoice.PaidDate.Value:dd MMM yyyy}", fontSmall, successGreenBrush,
                    new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
                y += lineHeight;
            }

            // Status Badge
            var statusColor = invoice.InvoiceStatus switch
            {
                InvoiceStatus.Paid => successGreenBrush,
                InvoiceStatus.PartiallyPaid => warningOrangeBrush,
                InvoiceStatus.Overdue => dangerRedBrush,
                InvoiceStatus.Void => lightGrayBrush,
                _ => secondaryVioletBrush
            };

            gfx.DrawRectangle(statusColor, invoiceHeaderX, y, 200, 25);
            gfx.DrawString($"Status: {invoice.InvoiceStatus.ToString().ToUpper()}", fontHeader, accentWhiteBrush,
                new XRect(invoiceHeaderX, y + 5, 200, 20), XStringFormats.TopCenter);

            // Horizontal separator line
            double maxHeaderY = Math.Max(companyY, y + 30);
            gfx.DrawLine(new XPen(primaryViolet, 1), margin, maxHeaderY, page.Width - margin, maxHeaderY);

            y = maxHeaderY + sectionSpacing;

            // ===== BILL TO / SHIP TO SECTION =====
            // Bill To
            gfx.DrawString("BILL TO:", fontHeader, primaryVioletBrush,
                new XRect(margin, y, 200, lineHeight), XStringFormats.TopLeft);

            var customerX = margin + 200;
            gfx.DrawString(invoice.Customer?.Name ?? "N/A", fontBold, blackBrush,
                new XRect(margin + 100, y, 250, lineHeight), XStringFormats.TopLeft);
            y += lineHeight + 2;

            // Customer Address
            if (invoice.Customer?.Address != null)
            {
                var addr = invoice.Customer.Address;
                var addressLines = new List<string>();

                if (!string.IsNullOrEmpty(addr.Address1)) addressLines.Add(addr.Address1);
                if (!string.IsNullOrEmpty(addr.Address2)) addressLines.Add(addr.Address2);
                if (!string.IsNullOrEmpty(addr.City)) addressLines.Add(addr.City);
                if (!string.IsNullOrEmpty(addr.State)) addressLines.Add(addr.State);
                if (!string.IsNullOrEmpty(addr.ZipCode)) addressLines.Add(addr.ZipCode);
                if (!string.IsNullOrEmpty(addr.Country)) addressLines.Add(addr.Country);

                foreach (var line in addressLines)
                {
                    gfx.DrawString(line, fontRegular, darkGrayBrush,
                        new XRect(margin + 100, y, 300, lineHeight), XStringFormats.TopLeft);
                    y += lineHeight;
                }
            }

            // Customer Contact
            var contactInfo = new StringBuilder();
            if (!string.IsNullOrEmpty(invoice.Customer?.Email))
                contactInfo.Append($"Email: {invoice.Customer.Email}");
            if (!string.IsNullOrEmpty(invoice.Customer?.PhoneNumber))
                contactInfo.Append($" | Phone: {invoice.Customer.PhoneNumber}");

            if (contactInfo.Length > 0)
            {
                gfx.DrawString(contactInfo.ToString(), fontRegular, darkGrayBrush,
                    new XRect(margin + 100, y, 400, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;
            }

            // Shipping Address (if different)
            if (invoice.ShippingAddressId.HasValue && invoice.BillingAddressId != invoice.ShippingAddressId)
            {
                y += 10;
                gfx.DrawString("SHIP TO:", fontHeader, primaryVioletBrush,
                    new XRect(margin, y, 200, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString("Same as billing address", fontRegular, darkGrayBrush,
                    new XRect(margin + 100, y, 300, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;
            }

            y += 10;

            // Reference Numbers
            if (!string.IsNullOrEmpty(invoice.PONumber))
            {
                gfx.DrawString($"PO Number: {invoice.PONumber}", fontRegular, darkGrayBrush,
                    new XRect(margin, y, 300, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;
            }

            if (!string.IsNullOrEmpty(invoice.ProjectDetail))
            {
                gfx.DrawString($"Project: {invoice.ProjectDetail}", fontRegular, darkGrayBrush,
                    new XRect(margin, y, 400, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;
            }

            // Currency and Payment Terms
            gfx.DrawString($"Currency: {invoice.Currency} (Rate: {invoice.CurrencyRate:F2})", fontRegular, darkGrayBrush,
                new XRect(margin, y, 200, lineHeight), XStringFormats.TopLeft);
            gfx.DrawString($"Terms: {invoice.PaymentTerms ?? "Net 30"}", fontRegular, darkGrayBrush,
                new XRect(margin + 250, y, 200, lineHeight), XStringFormats.TopLeft);
            y += lineHeight + sectionSpacing;

            // ===== ITEMS TABLE =====
            var tableY = y;
            var tableWidth = page.Width - 2 * margin;
            var tableHeight = 30;

            // Table Header
            gfx.DrawRectangle(primaryVioletBrush, margin, tableY, tableWidth, tableHeight);

            // Column Widths
            double[] colWidths = { 200, 60, 80, 80, 80, 80 };
            double colX = margin + 5;

            // Header Texts
            string[] headers = { "Description", "Qty", "Unit Price", "Discount", "Tax", "Total" };
            XStringFormat[] alignments = { XStringFormats.TopLeft, XStringFormats.TopCenter, XStringFormats.TopCenter,
                                          XStringFormats.TopCenter, XStringFormats.TopCenter, XStringFormats.TopRight };

            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawString(headers[i], fontHeader, accentWhiteBrush,
                    new XRect(colX, tableY + 7, colWidths[i], lineHeight), alignments[i]);
                colX += colWidths[i];
            }

            tableY += tableHeight;
            int rowIndex = 0;

            // Table Rows - Invoice Items
            foreach (var item in invoice.InvoiceItems)
            {
                // Check for page overflow
                if (tableY > page.Height - 120)
                {
                    page = ((PdfDocument)page.Owner).AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    tableY = 40;
                }

                // Alternating row background
                if (rowIndex % 2 == 0)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 248, 248)),
                        margin, tableY, tableWidth, 25);
                }

                colX = margin + 5;

                // Description (with item code if available)
                var description = item.Description;
                gfx.DrawString(description, fontRegular, blackBrush,
                    new XRect(colX, tableY + 5, colWidths[0], lineHeight), XStringFormats.TopLeft);
                colX += colWidths[0];

                // Quantity
                //gfx.DrawString(item.Quantity.ToString("N0") + $" {item.UnitOfMeasure ?? "pcs"}", fontRegular, blackBrush,
                //    new XRect(colX, tableY + 5, colWidths[1], lineHeight), XStringFormats.TopCenter);
                //colX += colWidths[1];

                // Unit Price
                gfx.DrawString(item.UnitPrice.ToString("C", culture), fontRegular, blackBrush,
                    new XRect(colX, tableY + 5, colWidths[2], lineHeight), XStringFormats.TopCenter);
                colX += colWidths[2];

                // Discount
                //if (item.DiscountAmount > 0)
                //{
                //    var discountText = item.DiscountPercentage > 0
                //        ? $"{item.DiscountPercentage:F0}%"
                //        : item.DiscountAmount.ToString("C", culture);
                //    gfx.DrawString(discountText, fontRegular, dangerRedBrush,
                //        new XRect(colX, tableY + 5, colWidths[3], lineHeight), XStringFormats.TopCenter);
                //}
                //else
                //{
                //    gfx.DrawString("-", fontRegular, lightGrayBrush,
                //        new XRect(colX, tableY + 5, colWidths[3], lineHeight), XStringFormats.TopCenter);
                //}
                colX += colWidths[3];

                // Tax
                if (item.TaxAmount > 0)
                {
                    gfx.DrawString($"{item.TaxAmount.ToString("C", culture)} ({item.TaxPercentage:F1}%)", fontRegular, blackBrush,
                        new XRect(colX, tableY + 5, colWidths[4], lineHeight), XStringFormats.TopCenter);
                }
                else
                {
                    gfx.DrawString("-", fontRegular, lightGrayBrush,
                        new XRect(colX, tableY + 5, colWidths[4], lineHeight), XStringFormats.TopCenter);
                }
                colX += colWidths[4];

                // Total (Amount)
                gfx.DrawString(item.TotalAmount.ToString("C", culture), fontBold, blackBrush,
                    new XRect(colX, tableY + 5, colWidths[5] - 10, lineHeight), XStringFormats.TopRight);

                tableY += 25;
                rowIndex++;
            }

            tableY += 20;

            // ===== PAYMENT SUMMARY SECTION =====
            const double summaryBoxWidth = 250;
            const double summaryPadding = 15;

            var summaryX = page.Width - margin - summaryBoxWidth;
            var summaryY = tableY;

            // Summary Box
            gfx.DrawRoundedRectangle(new XPen(primaryViolet, 1), lightVioletBrush,
                summaryX, summaryY, summaryBoxWidth, 150, 10, 10);

            var summaryContentX = summaryX + summaryPadding;
            var summaryContentWidth = summaryBoxWidth - 2 * summaryPadding;
            var currentY = summaryY + summaryPadding;

            // Subtotal
            gfx.DrawString("Subtotal:", fontRegular, darkGrayBrush,
                new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
            gfx.DrawString(invoice.Subtotal.ToString("C", culture), fontRegular, blackBrush,
                new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
            currentY += lineHeight;

            // Discount Total
            if (invoice.DiscountTotal > 0)
            {
                gfx.DrawString("Discount:", fontRegular, darkGrayBrush,
                    new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString($"-{invoice.DiscountTotal.ToString("C", culture)}", fontRegular, dangerRedBrush,
                    new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
                currentY += lineHeight;
            }

            // Tax Total
            if (invoice.TaxTotal > 0)
            {
                gfx.DrawString("Tax:", fontRegular, darkGrayBrush,
                    new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.TaxTotal.ToString("C", culture), fontRegular, blackBrush,
                    new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
                currentY += lineHeight;

                // Tax Breakdown
                foreach (var tax in invoice.TaxDetails)
                {
                    gfx.DrawString($"  {tax.TaxName} ({tax.Rate:F1}%):", fontSmall, lightGrayBrush,
                        new XRect(summaryContentX + 15, currentY, summaryContentWidth - 115, lineHeight - 2), XStringFormats.TopLeft);
                    gfx.DrawString(tax.TaxAmount.ToString("C", culture), fontSmall, lightGrayBrush,
                        new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight - 2), XStringFormats.TopRight);
                    currentY += lineHeight - 2;
                }
            }

            // Shipping Amount
            if (invoice.ShippingAmount > 0)
            {
                gfx.DrawString("Shipping:", fontRegular, darkGrayBrush,
                    new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.ShippingAmount.ToString("C", culture), fontRegular, blackBrush,
                    new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
                currentY += lineHeight;
            }

            // Adjustment
            if (invoice.AdjustmentAmount != 0)
            {
                gfx.DrawString(invoice.AdjustmentDescription ?? "Adjustment:", fontRegular, darkGrayBrush,
                    new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.AdjustmentAmount.ToString("C", culture), fontRegular,
                    invoice.AdjustmentAmount > 0 ? blackBrush : dangerRedBrush,
                    new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
                currentY += lineHeight;
            }

            // Separator
            gfx.DrawLine(new XPen(lightGray, 0.5), summaryContentX, currentY + 2,
                summaryContentX + summaryContentWidth, currentY + 2);
            currentY += 10;

            // Total
            gfx.DrawRectangle(primaryVioletBrush, summaryContentX, currentY, summaryContentWidth, 30);
            gfx.DrawString("TOTAL:", fontBold, accentWhiteBrush,
                new XRect(summaryContentX + 5, currentY + 7, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
            gfx.DrawString(invoice.TotalAmount.ToString("C", culture), fontBold, accentWhiteBrush,
                new XRect(summaryContentX + 100, currentY + 7, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);

            // ===== PAYMENT STATUS SECTION =====
            var paymentY = summaryY + 170;

            gfx.DrawString("PAYMENT STATUS:", fontHeader, primaryVioletBrush,
                new XRect(margin, paymentY, 200, lineHeight), XStringFormats.TopLeft);

            var paymentStatusX = margin + 150;

            // Amount Paid
            gfx.DrawString($"Amount Paid:", fontRegular, darkGrayBrush,
                new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
            gfx.DrawString(invoice.AmountPaid.ToString("C", culture), fontBold, successGreenBrush,
                new XRect(paymentStatusX, paymentY, 150, lineHeight), XStringFormats.TopLeft);

            // Amount Due
            paymentY += lineHeight;
            gfx.DrawString($"Amount Due:", fontRegular, darkGrayBrush,
                new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);

            var dueColor = invoice.AmountDue > 0 ?
                (invoice.PaymentStatus == PaymentStatus.Overdue ? dangerRedBrush : warningOrangeBrush) :
                successGreenBrush;

            gfx.DrawString(invoice.AmountDue.ToString("C", culture), fontBold, dueColor,
                new XRect(paymentStatusX, paymentY, 150, lineHeight), XStringFormats.TopLeft);

            // Amount Refunded (if any)
            if (invoice.AmountRefunded > 0)
            {
                paymentY += lineHeight;
                gfx.DrawString($"Amount Refunded:", fontRegular, darkGrayBrush,
                    new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.AmountRefunded.ToString("C", culture), fontRegular, dangerRedBrush,
                    new XRect(paymentStatusX, paymentY, 150, lineHeight), XStringFormats.TopLeft);
            }

            // Payment Method & Gateway
            if (!string.IsNullOrEmpty(invoice.PaymentMethod))
            {
                paymentY += lineHeight;
                gfx.DrawString($"Payment Method:", fontRegular, darkGrayBrush,
                    new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.PaymentMethod, fontRegular, blackBrush,
                    new XRect(paymentStatusX, paymentY, 200, lineHeight), XStringFormats.TopLeft);

                if (!string.IsNullOrEmpty(invoice.PaymentGateway))
                {
                    paymentY += lineHeight;
                    gfx.DrawString($"Gateway:", fontRegular, darkGrayBrush,
                        new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(invoice.PaymentGateway, fontRegular, blackBrush,
                        new XRect(paymentStatusX, paymentY, 200, lineHeight), XStringFormats.TopLeft);
                }

                if (!string.IsNullOrEmpty(invoice.PaymentTransactionId))
                {
                    paymentY += lineHeight;
                    gfx.DrawString($"Transaction ID:", fontRegular, darkGrayBrush,
                        new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(invoice.PaymentTransactionId, fontSmall, blackBrush,
                        new XRect(paymentStatusX, paymentY, 250, lineHeight), XStringFormats.TopLeft);
                }
            }

            // Payment History (if any)
            if (invoice.Payments != null && invoice.Payments.Any())
            {
                paymentY += lineHeight + 10;
                gfx.DrawString("PAYMENT HISTORY:", fontHeader, primaryVioletBrush,
                    new XRect(margin, paymentY, 200, lineHeight), XStringFormats.TopLeft);
                paymentY += lineHeight;

                foreach (var payment in invoice.Payments.Where(p => !p.IsDeleted).Take(3))
                {
                    var paymentText = $"{payment.PaymentDate:dd MMM yyyy} - {payment.PaymentMethod} - {payment.Amount.ToString("C", culture)} {payment.PaymentReference}";
                    gfx.DrawString(paymentText, fontSmall, darkGrayBrush,
                        new XRect(margin + 20, paymentY, 400, lineHeight - 2), XStringFormats.TopLeft);
                    paymentY += lineHeight - 2;
                }
            }

            // ===== NOTES SECTION =====
            var notesY = Math.Max(paymentY + 20, summaryY + 200);

            // Customer Notes
            if (!string.IsNullOrEmpty(invoice.CustomerNotes))
            {
                gfx.DrawString("NOTES:", fontHeader, primaryVioletBrush,
                    new XRect(margin, notesY, 200, lineHeight), XStringFormats.TopLeft);
                notesY += lineHeight + 2;

                var notesLines = WrapText(invoice.CustomerNotes, 80);
                foreach (var line in notesLines)
                {
                    gfx.DrawString(line, fontSmall, darkGrayBrush,
                        new XRect(margin + 10, notesY, 400, lineHeight - 2), XStringFormats.TopLeft);
                    notesY += lineHeight - 2;
                }
                notesY += 10;
            }

            // Terms and Conditions
            if (!string.IsNullOrEmpty(invoice.TermsAndConditions))
            {
                gfx.DrawString("TERMS & CONDITIONS:", fontHeader, primaryVioletBrush,
                    new XRect(margin, notesY, 200, lineHeight), XStringFormats.TopLeft);
                notesY += lineHeight + 2;

                var termsLines = WrapText(invoice.TermsAndConditions, 80);
                foreach (var line in termsLines)
                {
                    gfx.DrawString(line, fontSmall, darkGrayBrush,
                        new XRect(margin + 10, notesY, 400, lineHeight - 2), XStringFormats.TopLeft);
                    notesY += lineHeight - 2;
                }
                notesY += 10;
            }

            // Footer Note
            if (!string.IsNullOrEmpty(invoice.FooterNote))
            {
                var footerY = page.Height - margin - 20;
                gfx.DrawString(invoice.FooterNote, fontSmall, lightGrayBrush,
                    new XRect(margin, footerY, page.Width - 2 * margin, lineHeight), XStringFormats.TopCenter);
            }
            else
            {
                // Default footer
                var footerY = page.Height - margin - 20;
                gfx.DrawString("Thank you for your business!", fontSmall, lightGrayBrush,
                    new XRect(margin, footerY, page.Width - 2 * margin, lineHeight), XStringFormats.TopCenter);
            }

            // Automation Badge
            if (invoice.IsAutomated)
            {
                var autoBadgeX = margin;
                var autoBadgeY = page.Height - margin - 40;
                gfx.DrawRectangle(secondaryVioletBrush, autoBadgeX, autoBadgeY, 120, 15);
                gfx.DrawString("AUTO-GENERATED", new XFont("Open Sans", 6, XFontStyleEx.Bold), accentWhiteBrush,
                    new XRect(autoBadgeX, autoBadgeY + 2, 120, 12), XStringFormats.TopCenter);
            }

            // Source System
            if (!string.IsNullOrEmpty(invoice.SourceSystem))
            {
                var sourceX = page.Width - margin - 120;
                var sourceY = page.Height - margin - 40;
                gfx.DrawString($"Source: {invoice.SourceSystem}", new XFont("Open Sans", 6, XFontStyleEx.Regular), lightGrayBrush,
                    new XRect(sourceX, sourceY, 120, 12), XStringFormats.TopRight);
            }
        }

        private List<string> WrapText(string text, int maxCharsPerLine)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text)) return lines;

            var words = text.Split(' ');
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if ((currentLine.Length + word.Length + 1) <= maxCharsPerLine)
                {
                    if (currentLine.Length > 0)
                        currentLine.Append(' ');
                    currentLine.Append(word);
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                    }

                    if (word.Length > maxCharsPerLine)
                    {
                        // Split long word
                        for (int i = 0; i < word.Length; i += maxCharsPerLine)
                        {
                            var part = word.Substring(i, Math.Min(maxCharsPerLine, word.Length - i));
                            lines.Add(part);
                        }
                    }
                    else
                    {
                        currentLine.Append(word);
                    }
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return lines;
        }
    }
}