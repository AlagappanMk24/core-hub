using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Application.DTOs.Company.Response;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.ComponentModel.Design;
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
                IQueryable<Domain.Entities.Invoice> query = _unitOfWork.Invoices.Query()
                         .Where(i => !i.IsDeleted);

                // Super Admin can access any invoice
                if (operationContext.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin generating invoice pdf {InvoiceId}", id);
                    // No company filter for Super Admin
                }
                else if (operationContext.CompanyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }
                else
                {
                    _logger.LogWarning("Company ID is required for non-super admin users.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }

                // Apply customer filter if user is a customer
                if (operationContext.CustomerId.HasValue)
                {
                    query = query.Where(i => i.CustomerId == operationContext.CustomerId.Value);
                }

                var invoice = await query
                     .Include(i => i.Customer)
                     .Include(i => i.InvoiceItems)
                     .Include(i => i.TaxDetails)
                     .Include(i => i.Discounts)
                     .Include(i => i.InvoiceAttachments)
                     .Include(i => i.Payments)
                     .FirstOrDefaultAsync(i => i.Id == id);

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
                int? companyId = null;
                // 1. Resolve Company ID based on Role
                if (operationContext.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin exporting invoices. Filtered Company: {CompanyId}", companyId ?? 0);
                }
                else
                {
                    // Standard users are strictly locked to their own CompanyId
                    if (!operationContext.CompanyId.HasValue)
                    {
                        return OperationResult<byte[]>.FailureResult("Access Denied: No Company ID associated with your account.");
                    }
                    companyId = operationContext.CompanyId.Value;
                }

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
                    var companyInfo = await _companyService.GetCompanyByIdAsync(invoice.CompanyId);
                    //await GenerateInvoicePdfContent(gfx, page, invoice);
                    DrawInvoice(gfx, page, invoice, companyInfo);
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

        //private async Task<MemoryStream> GenerateInvoicePdf(Invoice invoice)
        //{
        //    try
        //    {
        //        using var document = new PdfDocument();
        //        var page = document.AddPage();
        //        var gfx = XGraphics.FromPdfPage(page);

        //        await GenerateInvoicePdfContent(gfx, page, invoice);

        //        var stream = new MemoryStream();
        //        document.Save(stream, false);
        //        stream.Position = 0;
        //        return stream;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}", invoice.Id);
        //        return null;
        //    }
        //}

        //private async Task GenerateInvoicePdfContent(XGraphics gfx, PdfPage page, Invoice invoice)
        //{
        //    // Font definitions
        //    var fontTitle = new XFont("Open Sans", 24, XFontStyleEx.Bold);
        //    var fontHeader = new XFont("Open Sans", 12, XFontStyleEx.Bold);
        //    var fontRegular = new XFont("Open Sans", 10, XFontStyleEx.Regular);
        //    var fontBold = new XFont("Open Sans", 10, XFontStyleEx.Bold);
        //    var fontSmall = new XFont("Open Sans", 8, XFontStyleEx.Regular);
        //    var fontLargeBold = new XFont("Open Sans", 20, XFontStyleEx.Bold);

        //    double y = 30;
        //    const double margin = 40;
        //    const double lineHeight = 16;
        //    const double sectionSpacing = 25;

        //    // Color scheme
        //    var primaryViolet = XColor.FromArgb(167, 139, 250);
        //    var secondaryViolet = XColor.FromArgb(196, 181, 253);
        //    var lightViolet = XColor.FromArgb(230, 220, 255);
        //    var darkGray = XColor.FromArgb(64, 64, 64);
        //    var lightGray = XColor.FromArgb(128, 128, 128);
        //    var accentWhite = XColor.FromArgb(255, 255, 255);
        //    var successGreen = XColor.FromArgb(40, 167, 69);
        //    var warningOrange = XColor.FromArgb(255, 193, 7);
        //    var dangerRed = XColor.FromArgb(220, 53, 69);

        //    // Brushes
        //    var primaryVioletBrush = new XSolidBrush(primaryViolet);
        //    var secondaryVioletBrush = new XSolidBrush(secondaryViolet);
        //    var lightVioletBrush = new XSolidBrush(lightViolet);
        //    var darkGrayBrush = new XSolidBrush(darkGray);
        //    var lightGrayBrush = new XSolidBrush(lightGray);
        //    var accentWhiteBrush = new XSolidBrush(accentWhite);
        //    var successGreenBrush = new XSolidBrush(successGreen);
        //    var warningOrangeBrush = new XSolidBrush(warningOrange);
        //    var dangerRedBrush = new XSolidBrush(dangerRed);
        //    var blackBrush = XBrushes.Black;

        //    // Get culture for currency formatting
        //    var culture = invoice.Currency switch
        //    {
        //        "EUR" => new CultureInfo("de-DE"),
        //        "INR" => new CultureInfo("hi-IN"),
        //        "GBP" => new CultureInfo("en-GB"),
        //        "JPY" => new CultureInfo("ja-JP"),
        //        "CAD" => new CultureInfo("en-CA"),
        //        "AUD" => new CultureInfo("en-AU"),
        //        _ => new CultureInfo("en-US")
        //    };

        //    // Get company info
        //    var companyInfo = await _companyService.GetCompanyByIdAsync(invoice.CompanyId);

        //    // ===== HEADER SECTION =====
        //    // Left side - Company Info
        //    double companyY = y + 10;

        //    // Company Name
        //    gfx.DrawString(companyInfo.Name, new XFont("Open Sans", 16, XFontStyleEx.Bold), primaryVioletBrush,
        //        new XRect(margin, y, 300, 20), XStringFormats.TopLeft);
        //    companyY += lineHeight;

        //    // Company Tagline (if exists)
        //    if (!string.IsNullOrEmpty(companyInfo?.TaxId))
        //    {
        //        gfx.DrawString($"Tax ID: {companyInfo.TaxId}", fontSmall, lightGrayBrush,
        //            new XRect(margin, y + 20, 200, 12), XStringFormats.TopLeft);
        //    }
        //    companyY += lineHeight;

        //    // Company Address
        //    if (companyInfo?.Address != null)
        //    {
        //        var address = $"{companyInfo.Address.Address1 ?? ""} {companyInfo.Address.Address2 ?? ""}".Trim();
        //        if (!string.IsNullOrEmpty(address))
        //        {
        //            gfx.DrawString(address, fontSmall, darkGrayBrush,
        //                new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
        //            companyY += lineHeight;
        //        }

        //        var cityStateZip = $"{companyInfo.Address.City ?? ""}, {companyInfo.Address.State ?? ""} {companyInfo.Address.ZipCode ?? ""}".Trim(',', ' ');
        //        if (!string.IsNullOrEmpty(cityStateZip))
        //        {
        //            gfx.DrawString(cityStateZip, fontSmall, darkGrayBrush,
        //                new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
        //            companyY += lineHeight;
        //        }

        //        if (!string.IsNullOrEmpty(companyInfo.Address.Country))
        //        {
        //            gfx.DrawString(companyInfo.Address.Country, fontSmall, darkGrayBrush,
        //                new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
        //            companyY += lineHeight;
        //        }
        //    }

        //    // Company Contact
        //    if (!string.IsNullOrEmpty(companyInfo?.Email))
        //    {
        //        gfx.DrawString($"Email: {companyInfo.Email}", fontSmall, darkGrayBrush,
        //            new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
        //        companyY += lineHeight;
        //    }

        //    if (!string.IsNullOrEmpty(companyInfo?.PhoneNumber))
        //    {
        //        gfx.DrawString($"Phone: {companyInfo.PhoneNumber}", fontSmall, darkGrayBrush,
        //            new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
        //        companyY += lineHeight;
        //    }

        //    // Right side - Invoice Title and Details
        //    var invoiceHeaderX = page.Width - margin - 200;

        //    // Invoice Type Badge
        //    var invoiceTypeBg = invoice.InvoiceType switch
        //    {
        //        InvoiceType.Recurring => secondaryVioletBrush,
        //        InvoiceType.Proforma => warningOrangeBrush,
        //        InvoiceType.CreditNote => successGreenBrush,
        //        InvoiceType.DebitNote => dangerRedBrush,
        //        _ => primaryVioletBrush
        //    };

        //    gfx.DrawRectangle(invoiceTypeBg, invoiceHeaderX, y, 200, 30);
        //    gfx.DrawString(invoice.InvoiceType.ToString().ToUpper(), fontHeader, accentWhiteBrush,
        //        new XRect(invoiceHeaderX, y + 7, 200, 20), XStringFormats.TopCenter);

        //    y += 40;

        //    // Invoice Number
        //    gfx.DrawString($"INVOICE #{invoice.InvoiceNumber}", fontLargeBold, blackBrush,
        //        new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
        //    y += lineHeight + 5;

        //    // Dates
        //    gfx.DrawString($"Issue Date: {invoice.IssueDate:dd MMM yyyy}", fontRegular, blackBrush,
        //        new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
        //    y += lineHeight;

        //    gfx.DrawString($"Due Date: {invoice.DueDate:dd MMM yyyy}", fontRegular, invoice.PaymentStatus == PaymentStatus.Overdue ? dangerRedBrush : blackBrush,
        //        new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
        //    y += lineHeight;

        //    // Sent Date (if available)
        //    if (invoice.SentDate.HasValue)
        //    {
        //        gfx.DrawString($"Sent Date: {invoice.SentDate.Value:dd MMM yyyy}", fontSmall, lightGrayBrush,
        //            new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
        //        y += lineHeight;
        //    }

        //    // Paid Date (if available)
        //    if (invoice.PaidDate.HasValue)
        //    {
        //        gfx.DrawString($"Paid Date: {invoice.PaidDate.Value:dd MMM yyyy}", fontSmall, successGreenBrush,
        //            new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
        //        y += lineHeight;
        //    }

        //    // Status Badge
        //    var statusColor = invoice.InvoiceStatus switch
        //    {
        //        InvoiceStatus.Paid => successGreenBrush,
        //        InvoiceStatus.PartiallyPaid => warningOrangeBrush,
        //        InvoiceStatus.Overdue => dangerRedBrush,
        //        InvoiceStatus.Void => lightGrayBrush,
        //        _ => secondaryVioletBrush
        //    };

        //    gfx.DrawRectangle(statusColor, invoiceHeaderX, y, 200, 25);
        //    gfx.DrawString($"Status: {invoice.InvoiceStatus.ToString().ToUpper()}", fontHeader, accentWhiteBrush,
        //        new XRect(invoiceHeaderX, y + 5, 200, 20), XStringFormats.TopCenter);

        //    // Horizontal separator line
        //    double maxHeaderY = Math.Max(companyY, y + 30);
        //    gfx.DrawLine(new XPen(primaryViolet, 1), margin, maxHeaderY, page.Width - margin, maxHeaderY);

        //    y = maxHeaderY + sectionSpacing;

        //    // ===== BILL TO / SHIP TO SECTION =====
        //    // Bill To
        //    gfx.DrawString("BILL TO:", fontHeader, primaryVioletBrush,
        //        new XRect(margin, y, 200, lineHeight), XStringFormats.TopLeft);

        //    var customerX = margin + 200;
        //    gfx.DrawString(invoice.Customer?.Name ?? "N/A", fontBold, blackBrush,
        //        new XRect(margin + 100, y, 250, lineHeight), XStringFormats.TopLeft);
        //    y += lineHeight + 2;

        //    // Customer Address
        //    if (invoice.Customer?.Address != null)
        //    {
        //        var addr = invoice.Customer.Address;
        //        var addressLines = new List<string>();

        //        if (!string.IsNullOrEmpty(addr.Address1)) addressLines.Add(addr.Address1);
        //        if (!string.IsNullOrEmpty(addr.Address2)) addressLines.Add(addr.Address2);
        //        if (!string.IsNullOrEmpty(addr.City)) addressLines.Add(addr.City);
        //        if (!string.IsNullOrEmpty(addr.State)) addressLines.Add(addr.State);
        //        if (!string.IsNullOrEmpty(addr.ZipCode)) addressLines.Add(addr.ZipCode);
        //        if (!string.IsNullOrEmpty(addr.Country)) addressLines.Add(addr.Country);

        //        foreach (var line in addressLines)
        //        {
        //            gfx.DrawString(line, fontRegular, darkGrayBrush,
        //                new XRect(margin + 100, y, 300, lineHeight), XStringFormats.TopLeft);
        //            y += lineHeight;
        //        }
        //    }

        //    // Customer Contact
        //    var contactInfo = new StringBuilder();
        //    if (!string.IsNullOrEmpty(invoice.Customer?.Email))
        //        contactInfo.Append($"Email: {invoice.Customer.Email}");
        //    if (!string.IsNullOrEmpty(invoice.Customer?.PhoneNumber))
        //        contactInfo.Append($" | Phone: {invoice.Customer.PhoneNumber}");

        //    if (contactInfo.Length > 0)
        //    {
        //        gfx.DrawString(contactInfo.ToString(), fontRegular, darkGrayBrush,
        //            new XRect(margin + 100, y, 400, lineHeight), XStringFormats.TopLeft);
        //        y += lineHeight;
        //    }

        //    // Shipping Address (if different)
        //    if (invoice.ShippingAddressId.HasValue && invoice.BillingAddressId != invoice.ShippingAddressId)
        //    {
        //        y += 10;
        //        gfx.DrawString("SHIP TO:", fontHeader, primaryVioletBrush,
        //            new XRect(margin, y, 200, lineHeight), XStringFormats.TopLeft);
        //        gfx.DrawString("Same as billing address", fontRegular, darkGrayBrush,
        //            new XRect(margin + 100, y, 300, lineHeight), XStringFormats.TopLeft);
        //        y += lineHeight;
        //    }

        //    y += 10;

        //    // Reference Numbers
        //    if (!string.IsNullOrEmpty(invoice.PONumber))
        //    {
        //        gfx.DrawString($"PO Number: {invoice.PONumber}", fontRegular, darkGrayBrush,
        //            new XRect(margin, y, 300, lineHeight), XStringFormats.TopLeft);
        //        y += lineHeight;
        //    }

        //    if (!string.IsNullOrEmpty(invoice.ProjectDetail))
        //    {
        //        gfx.DrawString($"Project: {invoice.ProjectDetail}", fontRegular, darkGrayBrush,
        //            new XRect(margin, y, 400, lineHeight), XStringFormats.TopLeft);
        //        y += lineHeight;
        //    }

        //    // Currency and Payment Terms
        //    gfx.DrawString($"Currency: {invoice.Currency} (Rate: {invoice.CurrencyRate:F2})", fontRegular, darkGrayBrush,
        //        new XRect(margin, y, 200, lineHeight), XStringFormats.TopLeft);
        //    gfx.DrawString($"Terms: {invoice.PaymentTerms ?? "Net 30"}", fontRegular, darkGrayBrush,
        //        new XRect(margin + 250, y, 200, lineHeight), XStringFormats.TopLeft);
        //    y += lineHeight + sectionSpacing;

        //    // ===== ITEMS TABLE =====
        //    var tableY = y;
        //    var tableWidth = page.Width - 2 * margin;
        //    var tableHeight = 30;

        //    // Table Header
        //    gfx.DrawRectangle(primaryVioletBrush, margin, tableY, tableWidth, tableHeight);

        //    // Column Widths
        //    double[] colWidths = { 200, 60, 80, 80, 80, 80 };
        //    double colX = margin + 5;

        //    // Header Texts
        //    string[] headers = { "Description", "Qty", "Unit Price", "Discount", "Tax", "Total" };
        //    XStringFormat[] alignments = { XStringFormats.TopLeft, XStringFormats.TopCenter, XStringFormats.TopCenter,
        //                                  XStringFormats.TopCenter, XStringFormats.TopCenter, XStringFormats.TopRight };

        //    for (int i = 0; i < headers.Length; i++)
        //    {
        //        gfx.DrawString(headers[i], fontHeader, accentWhiteBrush,
        //            new XRect(colX, tableY + 7, colWidths[i], lineHeight), alignments[i]);
        //        colX += colWidths[i];
        //    }

        //    tableY += tableHeight;
        //    int rowIndex = 0;

        //    // Table Rows - Invoice Items
        //    foreach (var item in invoice.InvoiceItems)
        //    {
        //        // Check for page overflow
        //        if (tableY > page.Height - 120)
        //        {
        //            page = ((PdfDocument)page.Owner).AddPage();
        //            gfx = XGraphics.FromPdfPage(page);
        //            tableY = 40;
        //        }

        //        // Alternating row background
        //        if (rowIndex % 2 == 0)
        //        {
        //            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 248, 248)),
        //                margin, tableY, tableWidth, 25);
        //        }

        //        colX = margin + 5;

        //        // Description (with item code if available)
        //        var description = item.Description;
        //        gfx.DrawString(description, fontRegular, blackBrush,
        //            new XRect(colX, tableY + 5, colWidths[0], lineHeight), XStringFormats.TopLeft);
        //        colX += colWidths[0];

        //        // Quantity
        //        //gfx.DrawString(item.Quantity.ToString("N0") + $" {item.UnitOfMeasure ?? "pcs"}", fontRegular, blackBrush,
        //        //    new XRect(colX, tableY + 5, colWidths[1], lineHeight), XStringFormats.TopCenter);
        //        //colX += colWidths[1];

        //        // Unit Price
        //        gfx.DrawString(item.UnitPrice.ToString("C", culture), fontRegular, blackBrush,
        //            new XRect(colX, tableY + 5, colWidths[2], lineHeight), XStringFormats.TopCenter);
        //        colX += colWidths[2];

        //        // Discount
        //        //if (item.DiscountAmount > 0)
        //        //{
        //        //    var discountText = item.DiscountPercentage > 0
        //        //        ? $"{item.DiscountPercentage:F0}%"
        //        //        : item.DiscountAmount.ToString("C", culture);
        //        //    gfx.DrawString(discountText, fontRegular, dangerRedBrush,
        //        //        new XRect(colX, tableY + 5, colWidths[3], lineHeight), XStringFormats.TopCenter);
        //        //}
        //        //else
        //        //{
        //        //    gfx.DrawString("-", fontRegular, lightGrayBrush,
        //        //        new XRect(colX, tableY + 5, colWidths[3], lineHeight), XStringFormats.TopCenter);
        //        //}
        //        colX += colWidths[3];

        //        // Tax
        //        if (item.TaxAmount > 0)
        //        {
        //            gfx.DrawString($"{item.TaxAmount.ToString("C", culture)} ({item.TaxPercentage:F1}%)", fontRegular, blackBrush,
        //                new XRect(colX, tableY + 5, colWidths[4], lineHeight), XStringFormats.TopCenter);
        //        }
        //        else
        //        {
        //            gfx.DrawString("-", fontRegular, lightGrayBrush,
        //                new XRect(colX, tableY + 5, colWidths[4], lineHeight), XStringFormats.TopCenter);
        //        }
        //        colX += colWidths[4];

        //        // Total (Amount)
        //        gfx.DrawString(item.TotalAmount.ToString("C", culture), fontBold, blackBrush,
        //            new XRect(colX, tableY + 5, colWidths[5] - 10, lineHeight), XStringFormats.TopRight);

        //        tableY += 25;
        //        rowIndex++;
        //    }

        //    tableY += 20;

        //    // ===== PAYMENT SUMMARY SECTION =====
        //    const double summaryBoxWidth = 250;
        //    const double summaryPadding = 15;

        //    var summaryX = page.Width - margin - summaryBoxWidth;
        //    var summaryY = tableY;

        //    // Summary Box
        //    gfx.DrawRoundedRectangle(new XPen(primaryViolet, 1), lightVioletBrush,
        //        summaryX, summaryY, summaryBoxWidth, 150, 10, 10);

        //    var summaryContentX = summaryX + summaryPadding;
        //    var summaryContentWidth = summaryBoxWidth - 2 * summaryPadding;
        //    var currentY = summaryY + summaryPadding;

        //    // Subtotal
        //    gfx.DrawString("Subtotal:", fontRegular, darkGrayBrush,
        //        new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
        //    gfx.DrawString(invoice.Subtotal.ToString("C", culture), fontRegular, blackBrush,
        //        new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
        //    currentY += lineHeight;

        //    // Discount Total
        //    if (invoice.DiscountTotal > 0)
        //    {
        //        gfx.DrawString("Discount:", fontRegular, darkGrayBrush,
        //            new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
        //        gfx.DrawString($"-{invoice.DiscountTotal.ToString("C", culture)}", fontRegular, dangerRedBrush,
        //            new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
        //        currentY += lineHeight;
        //    }

        //    // Tax Total
        //    if (invoice.TaxTotal > 0)
        //    {
        //        gfx.DrawString("Tax:", fontRegular, darkGrayBrush,
        //            new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
        //        gfx.DrawString(invoice.TaxTotal.ToString("C", culture), fontRegular, blackBrush,
        //            new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
        //        currentY += lineHeight;

        //        // Tax Breakdown
        //        foreach (var tax in invoice.TaxDetails)
        //        {
        //            gfx.DrawString($"  {tax.TaxName} ({tax.Rate:F1}%):", fontSmall, lightGrayBrush,
        //                new XRect(summaryContentX + 15, currentY, summaryContentWidth - 115, lineHeight - 2), XStringFormats.TopLeft);
        //            gfx.DrawString(tax.TaxAmount.ToString("C", culture), fontSmall, lightGrayBrush,
        //                new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight - 2), XStringFormats.TopRight);
        //            currentY += lineHeight - 2;
        //        }
        //    }

        //    // Shipping Amount
        //    if (invoice.ShippingAmount > 0)
        //    {
        //        gfx.DrawString("Shipping:", fontRegular, darkGrayBrush,
        //            new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
        //        gfx.DrawString(invoice.ShippingAmount.ToString("C", culture), fontRegular, blackBrush,
        //            new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
        //        currentY += lineHeight;
        //    }

        //    // Adjustment
        //    if (invoice.AdjustmentAmount != 0)
        //    {
        //        gfx.DrawString(invoice.AdjustmentDescription ?? "Adjustment:", fontRegular, darkGrayBrush,
        //            new XRect(summaryContentX, currentY, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
        //        gfx.DrawString(invoice.AdjustmentAmount.ToString("C", culture), fontRegular,
        //            invoice.AdjustmentAmount > 0 ? blackBrush : dangerRedBrush,
        //            new XRect(summaryContentX + 100, currentY, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);
        //        currentY += lineHeight;
        //    }

        //    // Separator
        //    gfx.DrawLine(new XPen(lightGray, 0.5), summaryContentX, currentY + 2,
        //        summaryContentX + summaryContentWidth, currentY + 2);
        //    currentY += 10;

        //    // Total
        //    gfx.DrawRectangle(primaryVioletBrush, summaryContentX, currentY, summaryContentWidth, 30);
        //    gfx.DrawString("TOTAL:", fontBold, accentWhiteBrush,
        //        new XRect(summaryContentX + 5, currentY + 7, summaryContentWidth - 100, lineHeight), XStringFormats.TopLeft);
        //    gfx.DrawString(invoice.TotalAmount.ToString("C", culture), fontBold, accentWhiteBrush,
        //        new XRect(summaryContentX + 100, currentY + 7, summaryContentWidth - 100 - 10, lineHeight), XStringFormats.TopRight);

        //    // ===== PAYMENT STATUS SECTION =====
        //    var paymentY = summaryY + 170;

        //    gfx.DrawString("PAYMENT STATUS:", fontHeader, primaryVioletBrush,
        //        new XRect(margin, paymentY, 200, lineHeight), XStringFormats.TopLeft);

        //    var paymentStatusX = margin + 150;

        //    // Amount Paid
        //    gfx.DrawString($"Amount Paid:", fontRegular, darkGrayBrush,
        //        new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
        //    gfx.DrawString(invoice.AmountPaid.ToString("C", culture), fontBold, successGreenBrush,
        //        new XRect(paymentStatusX, paymentY, 150, lineHeight), XStringFormats.TopLeft);

        //    // Amount Due
        //    paymentY += lineHeight;
        //    gfx.DrawString($"Amount Due:", fontRegular, darkGrayBrush,
        //        new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);

        //    var dueColor = invoice.AmountDue > 0 ?
        //        (invoice.PaymentStatus == PaymentStatus.Overdue ? dangerRedBrush : warningOrangeBrush) :
        //        successGreenBrush;

        //    gfx.DrawString(invoice.AmountDue.ToString("C", culture), fontBold, dueColor,
        //        new XRect(paymentStatusX, paymentY, 150, lineHeight), XStringFormats.TopLeft);

        //    // Amount Refunded (if any)
        //    if (invoice.AmountRefunded > 0)
        //    {
        //        paymentY += lineHeight;
        //        gfx.DrawString($"Amount Refunded:", fontRegular, darkGrayBrush,
        //            new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
        //        gfx.DrawString(invoice.AmountRefunded.ToString("C", culture), fontRegular, dangerRedBrush,
        //            new XRect(paymentStatusX, paymentY, 150, lineHeight), XStringFormats.TopLeft);
        //    }

        //    // Payment Method & Gateway
        //    if (!string.IsNullOrEmpty(invoice.PaymentMethod))
        //    {
        //        paymentY += lineHeight;
        //        gfx.DrawString($"Payment Method:", fontRegular, darkGrayBrush,
        //            new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
        //        gfx.DrawString(invoice.PaymentMethod, fontRegular, blackBrush,
        //            new XRect(paymentStatusX, paymentY, 200, lineHeight), XStringFormats.TopLeft);

        //        if (!string.IsNullOrEmpty(invoice.PaymentGateway))
        //        {
        //            paymentY += lineHeight;
        //            gfx.DrawString($"Gateway:", fontRegular, darkGrayBrush,
        //                new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
        //            gfx.DrawString(invoice.PaymentGateway, fontRegular, blackBrush,
        //                new XRect(paymentStatusX, paymentY, 200, lineHeight), XStringFormats.TopLeft);
        //        }

        //        if (!string.IsNullOrEmpty(invoice.PaymentTransactionId))
        //        {
        //            paymentY += lineHeight;
        //            gfx.DrawString($"Transaction ID:", fontRegular, darkGrayBrush,
        //                new XRect(margin, paymentY, 120, lineHeight), XStringFormats.TopLeft);
        //            gfx.DrawString(invoice.PaymentTransactionId, fontSmall, blackBrush,
        //                new XRect(paymentStatusX, paymentY, 250, lineHeight), XStringFormats.TopLeft);
        //        }
        //    }

        //    // Payment History (if any)
        //    if (invoice.Payments != null && invoice.Payments.Any())
        //    {
        //        paymentY += lineHeight + 10;
        //        gfx.DrawString("PAYMENT HISTORY:", fontHeader, primaryVioletBrush,
        //            new XRect(margin, paymentY, 200, lineHeight), XStringFormats.TopLeft);
        //        paymentY += lineHeight;

        //        foreach (var payment in invoice.Payments.Where(p => !p.IsDeleted).Take(3))
        //        {
        //            var paymentText = $"{payment.PaymentDate:dd MMM yyyy} - {payment.PaymentMethod} - {payment.Amount.ToString("C", culture)} {payment.PaymentReference}";
        //            gfx.DrawString(paymentText, fontSmall, darkGrayBrush,
        //                new XRect(margin + 20, paymentY, 400, lineHeight - 2), XStringFormats.TopLeft);
        //            paymentY += lineHeight - 2;
        //        }
        //    }

        //    // ===== NOTES SECTION =====
        //    var notesY = Math.Max(paymentY + 20, summaryY + 200);

        //    // Customer Notes
        //    if (!string.IsNullOrEmpty(invoice.CustomerNotes))
        //    {
        //        gfx.DrawString("NOTES:", fontHeader, primaryVioletBrush,
        //            new XRect(margin, notesY, 200, lineHeight), XStringFormats.TopLeft);
        //        notesY += lineHeight + 2;

        //        var notesLines = WrapText(invoice.CustomerNotes, 80);
        //        foreach (var line in notesLines)
        //        {
        //            gfx.DrawString(line, fontSmall, darkGrayBrush,
        //                new XRect(margin + 10, notesY, 400, lineHeight - 2), XStringFormats.TopLeft);
        //            notesY += lineHeight - 2;
        //        }
        //        notesY += 10;
        //    }

        //    // Terms and Conditions
        //    if (!string.IsNullOrEmpty(invoice.TermsAndConditions))
        //    {
        //        gfx.DrawString("TERMS & CONDITIONS:", fontHeader, primaryVioletBrush,
        //            new XRect(margin, notesY, 200, lineHeight), XStringFormats.TopLeft);
        //        notesY += lineHeight + 2;

        //        var termsLines = WrapText(invoice.TermsAndConditions, 80);
        //        foreach (var line in termsLines)
        //        {
        //            gfx.DrawString(line, fontSmall, darkGrayBrush,
        //                new XRect(margin + 10, notesY, 400, lineHeight - 2), XStringFormats.TopLeft);
        //            notesY += lineHeight - 2;
        //        }
        //        notesY += 10;
        //    }

        //    // Footer Note
        //    if (!string.IsNullOrEmpty(invoice.FooterNote))
        //    {
        //        var footerY = page.Height - margin - 20;
        //        gfx.DrawString(invoice.FooterNote, fontSmall, lightGrayBrush,
        //            new XRect(margin, footerY, page.Width - 2 * margin, lineHeight), XStringFormats.TopCenter);
        //    }
        //    else
        //    {
        //        // Default footer
        //        var footerY = page.Height - margin - 20;
        //        gfx.DrawString("Thank you for your business!", fontSmall, lightGrayBrush,
        //            new XRect(margin, footerY, page.Width - 2 * margin, lineHeight), XStringFormats.TopCenter);
        //    }

        //    // Automation Badge
        //    if (invoice.IsAutomated)
        //    {
        //        var autoBadgeX = margin;
        //        var autoBadgeY = page.Height - margin - 40;
        //        gfx.DrawRectangle(secondaryVioletBrush, autoBadgeX, autoBadgeY, 120, 15);
        //        gfx.DrawString("AUTO-GENERATED", new XFont("Open Sans", 6, XFontStyleEx.Bold), accentWhiteBrush,
        //            new XRect(autoBadgeX, autoBadgeY + 2, 120, 12), XStringFormats.TopCenter);
        //    }

        //    // Source System
        //    if (!string.IsNullOrEmpty(invoice.SourceSystem))
        //    {
        //        var sourceX = page.Width - margin - 120;
        //        var sourceY = page.Height - margin - 40;
        //        gfx.DrawString($"Source: {invoice.SourceSystem}", new XFont("Open Sans", 6, XFontStyleEx.Regular), lightGrayBrush,
        //            new XRect(sourceX, sourceY, 120, 12), XStringFormats.TopRight);
        //    }
        //}
        private async Task<MemoryStream> GenerateInvoicePdf(Domain.Entities.Invoice invoice)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            var gfx = XGraphics.FromPdfPage(page);
            var companyInfo = await _companyService.GetCompanyByIdAsync(invoice.CompanyId);

            DrawInvoice(gfx, page, invoice, companyInfo);

            var stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;
            return stream;
        }
        private void DrawInvoice(XGraphics gfx, PdfPage page, Domain.Entities.Invoice invoice, CompanyResponseDto companyInfo)
        {
            // Fonts
            var fontTitle = new XFont("Open Sans", 18, XFontStyleEx.Bold);
            var fontHeader = new XFont("Open Sans", 11, XFontStyleEx.Bold);
            var fontNormal = new XFont("Open Sans", 9, XFontStyleEx.Regular);
            var fontBold = new XFont("Open Sans", 9, XFontStyleEx.Bold);
            var fontSmall = new XFont("Open Sans", 8, XFontStyleEx.Regular);
            var fontVerySmall = new XFont("Open Sans", 7, XFontStyleEx.Regular);
            var fontItalic = new XFont("Open Sans", 8, XFontStyleEx.Italic);

            double margin = 40;
            double y = 40;
            double pageWidth = page.Width;
            double pageHeight = page.Height;
            var currencySymbol = GetCurrencySymbol(invoice.Currency);
            var currencyDisplayName = GetCurrencyDisplayName(invoice.Currency);
            var totalInWords = NumberToWords(invoice.TotalAmount, invoice.Currency);

            // ===== HEADER SECTION =====
            // Company Logo/Name
            gfx.DrawString(companyInfo?.Name ?? "COMPANY NAME", fontTitle, XBrushes.Black,
                new XRect(margin, y, 250, 30), XStringFormats.TopLeft);
            y += 28;

            // Company Details
            var companyY = y;
            if (!string.IsNullOrEmpty(companyInfo?.TaxId))
            {
                gfx.DrawString($"GST/VAT: {companyInfo.TaxId}", fontSmall, XBrushes.Gray,
                    new XRect(margin, companyY, 250, 15), XStringFormats.TopLeft);
                companyY += 15;
            }

            if (companyInfo?.Address != null)
            {
                var address = $"{companyInfo.Address.Address1 ?? ""} {companyInfo.Address.Address2 ?? ""}".Trim();
                if (!string.IsNullOrEmpty(address))
                {
                    gfx.DrawString(address, fontSmall, XBrushes.Gray,
                        new XRect(margin, companyY, 250, 15), XStringFormats.TopLeft);
                    companyY += 15;
                }

                var cityState = $"{companyInfo.Address.City ?? ""}, {companyInfo.Address.State ?? ""} {companyInfo.Address.ZipCode ?? ""}".Trim();
                if (!string.IsNullOrEmpty(cityState))
                {
                    gfx.DrawString(cityState, fontSmall, XBrushes.Gray,
                        new XRect(margin, companyY, 250, 15), XStringFormats.TopLeft);
                    companyY += 15;
                }

                if (!string.IsNullOrEmpty(companyInfo.Address.Country))
                {
                    gfx.DrawString(companyInfo.Address.Country, fontSmall, XBrushes.Gray,
                        new XRect(margin, companyY, 250, 15), XStringFormats.TopLeft);
                    companyY += 15;
                }
            }

            if (!string.IsNullOrEmpty(companyInfo?.Email))
            {
                gfx.DrawString(companyInfo.Email, fontSmall, XBrushes.Gray,
                    new XRect(margin, companyY, 250, 15), XStringFormats.TopLeft);
                companyY += 15;
            }

            if (!string.IsNullOrEmpty(companyInfo?.PhoneNumber))
            {
                gfx.DrawString(companyInfo.PhoneNumber, fontSmall, XBrushes.Gray,
                    new XRect(margin, companyY, 250, 15), XStringFormats.TopLeft);
                companyY += 15;
            }

            // Right side - INVOICE title
            gfx.DrawString("INVOICE", new XFont("Open Sans", 24, XFontStyleEx.Bold), new XSolidBrush(XColor.FromArgb(138, 43, 226)),
                new XRect(pageWidth - margin - 150, 40, 150, 35), XStringFormats.TopRight);

            // Invoice Number
            gfx.DrawString($"#{invoice.InvoiceNumber}", fontNormal, XBrushes.Black,
                new XRect(pageWidth - margin - 150, 80, 150, 20), XStringFormats.TopRight);

            double totalBoxX = pageWidth - margin - 85;
            double totalBoxY = 108;
            double totalBoxWidth = 85;
            double totalBoxHeight = 38;
            double cornerRadius = 3;

            // Create rounded rectangle
            var path = new XGraphicsPath();
            path.AddArc(totalBoxX, totalBoxY, cornerRadius * 2, cornerRadius * 2, 180, 90);
            path.AddArc(totalBoxX + totalBoxWidth - cornerRadius * 2, totalBoxY, cornerRadius * 2, cornerRadius * 2, 270, 90);
            path.AddArc(totalBoxX + totalBoxWidth - cornerRadius * 2, totalBoxY + totalBoxHeight - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            path.AddArc(totalBoxX, totalBoxY + totalBoxHeight - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            path.CloseFigure();

            // Fill background with gradient effect
            var totalBoxBg = new XSolidBrush(XColor.FromArgb(138, 43, 226));
            gfx.DrawPath(totalBoxBg, path);

            // Total Amount Label
            gfx.DrawString("TOTAL AMOUNT", new XFont("Open Sans", 6.5, XFontStyleEx.Bold), XBrushes.White,
                new XRect(totalBoxX + 3, totalBoxY + 5, totalBoxWidth - 6, 10), XStringFormats.TopCenter);

            // Total Amount Value
            var totalAmountText = $"{currencySymbol}{invoice.TotalAmount:N2}";
            gfx.DrawString(totalAmountText, new XFont("Open Sans", 9, XFontStyleEx.Bold), XBrushes.White,
                new XRect(totalBoxX + 3, totalBoxY + 17, totalBoxWidth - 6, 16), XStringFormats.TopCenter);

            // ===== BILL TO SECTION =====
            y = Math.Max(companyY + 10, 130);

            // Separator line
            gfx.DrawLine(new XPen(XColors.LightGray, 0.5), margin, y, pageWidth - margin, y);
            y += 15;

            gfx.DrawString("BILL TO", fontHeader, new XSolidBrush(XColor.FromArgb(138, 43, 226)),
                new XRect(margin, y, 100, 20), XStringFormats.TopLeft);
            y += 20;

            // Customer Name
            gfx.DrawString(invoice.Customer?.Name ?? "N/A", fontBold, XBrushes.Black,
                new XRect(margin, y, 250, 18), XStringFormats.TopLeft);
            y += 18;

            // Customer Address
            if (invoice.Customer?.Address != null)
            {
                var addr = invoice.Customer.Address;
                if (!string.IsNullOrEmpty(addr.Address1))
                {
                    gfx.DrawString(addr.Address1, fontNormal, XBrushes.Black,
                        new XRect(margin, y, 250, 16), XStringFormats.TopLeft);
                    y += 16;
                }

                var cityLine = $"{addr.City ?? ""} {addr.State ?? ""} {addr.ZipCode ?? ""}".Trim();
                if (!string.IsNullOrEmpty(cityLine))
                {
                    gfx.DrawString(cityLine, fontNormal, XBrushes.Black,
                        new XRect(margin, y, 250, 16), XStringFormats.TopLeft);
                    y += 16;
                }

                if (!string.IsNullOrEmpty(addr.Country))
                {
                    gfx.DrawString(addr.Country, fontNormal, XBrushes.Black,
                        new XRect(margin, y, 250, 16), XStringFormats.TopLeft);
                    y += 16;
                }
            }

            // Customer Contact
            if (!string.IsNullOrEmpty(invoice.Customer?.Email))
            {
                gfx.DrawString(invoice.Customer.Email, fontNormal, XBrushes.Black,
                    new XRect(margin, y, 250, 16), XStringFormats.TopLeft);
                y += 16;
            }

            if (!string.IsNullOrEmpty(invoice.Customer?.PhoneNumber))
            {
                gfx.DrawString(invoice.Customer.PhoneNumber, fontNormal, XBrushes.Black,
                    new XRect(margin, y, 250, 16), XStringFormats.TopLeft);
                y += 16;
            }

            // ===== INVOICE DETAILS (Right Side) =====
            double detailsX = pageWidth - margin - 200;
            double detailsY = 130 + 55;

            gfx.DrawString("INVOICE DETAILS", fontHeader, new XSolidBrush(XColor.FromArgb(138, 43, 226)),
                new XRect(detailsX, detailsY, 200, 20), XStringFormats.TopLeft);
            detailsY += 22;

            // Issue Date
            gfx.DrawString("Issue Date:", fontNormal, XBrushes.Gray,
                new XRect(detailsX, detailsY, 85, 16), XStringFormats.TopLeft);
            gfx.DrawString(invoice.IssueDate.ToString("dd MMM yyyy"), fontNormal, XBrushes.Black,
                new XRect(detailsX + 90, detailsY, 110, 16), XStringFormats.TopRight);
            detailsY += 18;

            // Due Date
            gfx.DrawString("Due Date:", fontNormal, XBrushes.Gray,
                new XRect(detailsX, detailsY, 85, 16), XStringFormats.TopLeft);
            gfx.DrawString(invoice.DueDate.ToString("dd MMM yyyy"), fontNormal, XBrushes.Black,
                new XRect(detailsX + 90, detailsY, 110, 16), XStringFormats.TopRight);
            detailsY += 18;

            // PO Number
            if (!string.IsNullOrEmpty(invoice.PONumber))
            {
                gfx.DrawString("PO Number:", fontNormal, XBrushes.Gray,
                    new XRect(detailsX, detailsY, 85, 16), XStringFormats.TopLeft);
                gfx.DrawString(invoice.PONumber, fontNormal, XBrushes.Black,
                    new XRect(detailsX + 90, detailsY, 110, 16), XStringFormats.TopRight);
                detailsY += 18;
            }

            // Payment Terms
            if (!string.IsNullOrEmpty(invoice.PaymentTerms))
            {
                gfx.DrawString("Payment Terms:", fontNormal, XBrushes.Gray,
                    new XRect(detailsX, detailsY, 85, 16), XStringFormats.TopLeft);
                gfx.DrawString(invoice.PaymentTerms, fontNormal, XBrushes.Black,
                    new XRect(detailsX + 90, detailsY, 110, 16), XStringFormats.TopRight);
                detailsY += 18;
            }

            // ===== ITEMS TABLE =====
            double tableY = Math.Max(y, detailsY) + 25;

            // Column definitions - Adjusted for better alignment
            double[] colWidths = { 280, 50, 60, 60, 70, 70 };
            double[] colX = {
                margin + 10,           // Description
                margin + 190,          // Quantity
                margin + 250,          // Unit Price
                margin + 320,          // Tax %
                margin + 380,          // Tax Amount
                margin + 440           // Total
            };

            string[] headers = { "Description", "Qty", "Unit Price", "Tax %", "Tax", "Amount" };
            XStringFormat[] alignments = {
                XStringFormats.TopLeft,    // Description
                XStringFormats.TopCenter,  // Qty
                XStringFormats.TopCenter,  // Unit Price
                XStringFormats.TopCenter,  // Tax %
                XStringFormats.TopCenter,  // Tax
                XStringFormats.TopRight    // Amount
            };

            // Draw table header background
            var headerBg = new XSolidBrush(XColor.FromArgb(138, 43, 226));
            gfx.DrawRectangle(headerBg, margin, tableY, pageWidth - 2 * margin, 28);

            // Draw headers
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawString(headers[i], fontHeader, XBrushes.White,
                    new XRect(colX[i], tableY + 7, colWidths[i] - 5, 18), alignments[i]);
            }

            tableY += 28;
            int rowNum = 0;
            double defaultRowHeight = 25;

            // Draw items with proper text wrapping
            foreach (var item in invoice.InvoiceItems)
            {
                // Calculate required height based on description text wrapping
                var descriptionLines = WrapTextForPdf(item.Description ?? "", 45);
                var rowHeight = Math.Max(defaultRowHeight, descriptionLines.Count * 14);

                // Check page break
                if (tableY + rowHeight > pageHeight - 150)
                {
                    page = ((PdfDocument)page.Owner).AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    tableY = 40;

                    // Redraw header on new page
                    gfx.DrawRectangle(headerBg, margin, tableY, pageWidth - 2 * margin, 28);
                    for (int i = 0; i < headers.Length; i++)
                    {
                        gfx.DrawString(headers[i], fontHeader, XBrushes.White,
                            new XRect(colX[i], tableY + 7, colWidths[i] - 5, 18), alignments[i]);
                    }
                    tableY += 28;
                }

                // Alternating row background
                if (rowNum % 2 == 1)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 248, 248)),
                        margin, tableY, pageWidth - 2 * margin, rowHeight);
                }

                // Item Description (with wrapping)
                double descY = tableY + 4;
                foreach (var line in descriptionLines)
                {
                    gfx.DrawString(line, fontNormal, XBrushes.Black,
                        new XRect(colX[0], descY, colWidths[0] - 10, 14), XStringFormats.TopLeft);
                    descY += 14;
                }

                // Quantity
                gfx.DrawString(item.Quantity.ToString("N0"), fontNormal, XBrushes.Black,
                    new XRect(colX[1], tableY + (rowHeight - 14) / 2, colWidths[1] - 5, 14), XStringFormats.TopCenter);

                // Unit Price
                gfx.DrawString($"{currencySymbol}{item.UnitPrice:N2}", fontNormal, XBrushes.Black,
                    new XRect(colX[2], tableY + (rowHeight - 14) / 2, colWidths[2] - 5, 14), XStringFormats.TopCenter);

                // Tax Percentage
                var taxPercentage = (item.TaxAmount > 0 && item.UnitPrice > 0)
                    ? (item.TaxAmount / (item.Quantity * item.UnitPrice) * 100).ToString("F1")
                    : "0";
                gfx.DrawString($"{taxPercentage}%", fontNormal, XBrushes.Black,
                    new XRect(colX[3], tableY + (rowHeight - 14) / 2, colWidths[3] - 5, 14), XStringFormats.TopCenter);

                // Tax Amount
                gfx.DrawString($"{currencySymbol}{item.TaxAmount:N2}", fontNormal, XBrushes.Black,
                    new XRect(colX[4], tableY + (rowHeight - 14) / 2, colWidths[4] - 5, 14), XStringFormats.TopCenter);

                // Total Amount
                gfx.DrawString($"{currencySymbol}{item.TotalAmount:N2}", fontNormal, XBrushes.Black,
                    new XRect(colX[5], tableY + (rowHeight - 14) / 2, colWidths[5] - 10, 14), XStringFormats.TopRight);

                tableY += rowHeight;
                rowNum++;
            }

            // Add empty row for spacing
            tableY += 10;

            // ===== TWO COLUMN LAYOUT FOR SUMMARY & PAYMENT INFO =====
            double leftColumnX = margin;
            double rightColumnX = pageWidth - margin - 220;
            double columnWidth = 220;
            double columnStartY = tableY + 30;

            // ===== PAYMENT INFORMATION SECTION (Left Column) =====
            double paymentInfoY = columnStartY;

            // Draw payment info box background
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 248, 248)),
                leftColumnX - 10, paymentInfoY - 5, columnWidth + 10, 130);

            gfx.DrawString("PAYMENT INFORMATION", fontHeader, new XSolidBrush(XColor.FromArgb(138, 43, 226)),
                new XRect(leftColumnX, paymentInfoY, columnWidth, 20), XStringFormats.TopLeft);
            paymentInfoY += 25;

            // Payment Method
            if (!string.IsNullOrEmpty(invoice.PaymentMethod))
            {
                gfx.DrawString("Payment Method:", fontNormal, XBrushes.Gray,
                    new XRect(leftColumnX, paymentInfoY, 100, 16), XStringFormats.TopLeft);
                gfx.DrawString(invoice.PaymentMethod, fontNormal, XBrushes.Black,
                    new XRect(leftColumnX + 105, paymentInfoY, columnWidth - 105, 16), XStringFormats.TopLeft);
                paymentInfoY += 18;
            }

            // Payment Gateway
            if (!string.IsNullOrEmpty(invoice.PaymentGateway))
            {
                gfx.DrawString("Gateway:", fontNormal, XBrushes.Gray,
                    new XRect(leftColumnX, paymentInfoY, 100, 16), XStringFormats.TopLeft);
                gfx.DrawString(invoice.PaymentGateway, fontNormal, XBrushes.Black,
                    new XRect(leftColumnX + 105, paymentInfoY, columnWidth - 105, 16), XStringFormats.TopLeft);
                paymentInfoY += 18;
            }

            // Transaction ID
            if (!string.IsNullOrEmpty(invoice.PaymentTransactionId))
            {
                gfx.DrawString("Transaction ID:", fontNormal, XBrushes.Gray,
                    new XRect(leftColumnX, paymentInfoY, 100, 16), XStringFormats.TopLeft);
                gfx.DrawString(invoice.PaymentTransactionId, fontSmall, XBrushes.Black,
                    new XRect(leftColumnX + 105, paymentInfoY, columnWidth - 105, 16), XStringFormats.TopLeft);
                paymentInfoY += 18;
            }

            // Bank Details (can be customized or fetched from company settings)
            gfx.DrawString("Bank Name:", fontNormal, XBrushes.Gray,
                new XRect(leftColumnX, paymentInfoY, 100, 16), XStringFormats.TopLeft);
            gfx.DrawString( "[Your Bank Name]", fontNormal, XBrushes.Black,
                new XRect(leftColumnX + 105, paymentInfoY, columnWidth - 105, 16), XStringFormats.TopLeft);
            paymentInfoY += 18;

            gfx.DrawString("Account Name:", fontNormal, XBrushes.Gray,
                new XRect(leftColumnX, paymentInfoY, 100, 16), XStringFormats.TopLeft);
            gfx.DrawString( "[Company Name]", fontNormal, XBrushes.Black,
                new XRect(leftColumnX + 105, paymentInfoY, columnWidth - 105, 16), XStringFormats.TopLeft);
            paymentInfoY += 18;

            gfx.DrawString("Account Number:", fontNormal, XBrushes.Gray,
                new XRect(leftColumnX, paymentInfoY, 100, 16), XStringFormats.TopLeft);
            gfx.DrawString( "[Account Number]", fontNormal, XBrushes.Black,
                new XRect(leftColumnX + 105, paymentInfoY, columnWidth - 105, 16), XStringFormats.TopLeft);
            paymentInfoY += 18;

            gfx.DrawString("IFSC/SWIFT:", fontNormal, XBrushes.Gray,
                new XRect(leftColumnX, paymentInfoY, 100, 16), XStringFormats.TopLeft);
            gfx.DrawString( "[IFSC/SWIFT Code]", fontNormal, XBrushes.Black,
                new XRect(leftColumnX + 105, paymentInfoY, columnWidth - 105, 16), XStringFormats.TopLeft);

            // ===== SUMMARY SECTION (Right Column) =====
            double summaryY = columnStartY;
            double summaryWidth = 220;

            // Draw summary box background
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 248, 248)),
                rightColumnX - 10, summaryY - 5, summaryWidth + 10, 240);

            // Subtotal
            gfx.DrawString("Subtotal:", fontNormal, XBrushes.Gray,
                new XRect(rightColumnX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
            gfx.DrawString($"{currencySymbol}{invoice.Subtotal:N2}", fontNormal, XBrushes.Black,
                new XRect(rightColumnX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
            summaryY += 20;

            // Discount
            if (invoice.DiscountTotal > 0)
            {
                gfx.DrawString("Discount:", fontNormal, XBrushes.Gray,
                    new XRect(rightColumnX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
                gfx.DrawString($"-{currencySymbol}{invoice.DiscountTotal:N2}", fontNormal, new XSolidBrush(XColor.FromArgb(220, 53, 69)),
                    new XRect(rightColumnX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
                summaryY += 20;
            }

            // Tax
            if (invoice.TaxTotal > 0)
            {
                gfx.DrawString("Tax:", fontNormal, XBrushes.Gray,
                    new XRect(rightColumnX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
                gfx.DrawString($"{currencySymbol}{invoice.TaxTotal:N2}", fontNormal, XBrushes.Black,
                    new XRect(rightColumnX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
                summaryY += 20;
            }

            // Shipping
            if (invoice.ShippingAmount > 0)
            {
                gfx.DrawString("Shipping:", fontNormal, XBrushes.Gray,
                    new XRect(rightColumnX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
                gfx.DrawString($"{currencySymbol}{invoice.ShippingAmount:N2}", fontNormal, XBrushes.Black,
                    new XRect(rightColumnX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
                summaryY += 20;
            }

            // ADJUSTMENT AMOUNT
            if (invoice.AdjustmentAmount != 0)
            {
                var adjustmentColor = invoice.AdjustmentAmount > 0
                    ? new XSolidBrush(XColor.FromArgb(220, 53, 69))  // Red for positive adjustment (additional charge)
                    : new XSolidBrush(XColor.FromArgb(40, 167, 69)); // Green for negative adjustment (credit)

                var adjustmentSign = invoice.AdjustmentAmount > 0 ? "+" : "";
                var adjustmentLabel = !string.IsNullOrEmpty(invoice.AdjustmentDescription)
                    ? $"Adjustment ({invoice.AdjustmentDescription}):"
                    : "Adjustment:";

                gfx.DrawString(adjustmentLabel, fontNormal, XBrushes.Gray,
                    new XRect(rightColumnX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
                gfx.DrawString($"{adjustmentSign}{currencySymbol}{Math.Abs(invoice.AdjustmentAmount):N2}", fontNormal, adjustmentColor,
                    new XRect(rightColumnX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
                summaryY += 20;
            }

            // Separator line
            gfx.DrawLine(new XPen(XColors.LightGray, 0.5), rightColumnX, summaryY, rightColumnX + summaryWidth, summaryY);
            summaryY += 10;

            // Total
            var totalBg = new XSolidBrush(XColor.FromArgb(138, 43, 226));
            gfx.DrawRectangle(totalBg, rightColumnX, summaryY, summaryWidth, 32);
            gfx.DrawString("TOTAL", fontBold, XBrushes.White,
                new XRect(rightColumnX + 10, summaryY + 8, 80, 18), XStringFormats.TopLeft);
            gfx.DrawString($"{currencySymbol}{invoice.TotalAmount:N2}", fontBold, XBrushes.White,
                new XRect(rightColumnX + summaryWidth - 20, summaryY + 8, 10, 18), XStringFormats.TopRight);
            summaryY += 40;

            // Total in Words
            gfx.DrawString("Total in Words:", fontBold, XBrushes.Black,
                new XRect(rightColumnX, summaryY, 120, 18), XStringFormats.TopLeft);

            var wordsLines = WrapTextForPdf(totalInWords, 25);
            double wordsY = summaryY;
            foreach (var line in wordsLines)
            {
                gfx.DrawString(line, fontItalic, new XSolidBrush(XColor.FromArgb(138, 43, 226)),
                    new XRect(rightColumnX + 100, wordsY, summaryWidth - 100, 16), XStringFormats.TopLeft);
                wordsY += 16;
            }
            summaryY = Math.Max(summaryY + 30, wordsY + 10);

            // Amount Paid
            if (invoice.AmountPaid > 0)
            {
                gfx.DrawString("Amount Paid:", fontNormal, XBrushes.Gray,
                    new XRect(rightColumnX, summaryY, 120, 18), XStringFormats.TopLeft);
                gfx.DrawString($"{currencySymbol}{invoice.AmountPaid:N2}", fontNormal, new XSolidBrush(XColor.FromArgb(40, 167, 69)),
                    new XRect(rightColumnX + 120, summaryY, 90, 18), XStringFormats.TopRight);
                summaryY += 20;

                var balanceDue = invoice.TotalAmount - invoice.AmountPaid;
                if (balanceDue > 0)
                {
                    gfx.DrawString("Balance Due:", fontBold, XBrushes.Gray,
                        new XRect(rightColumnX, summaryY, 120, 18), XStringFormats.TopLeft);
                    gfx.DrawString($"{currencySymbol}{balanceDue:N2}", fontBold, new XSolidBrush(XColor.FromArgb(220, 53, 69)),
                        new XRect(rightColumnX + 120, summaryY, 90, 18), XStringFormats.TopRight);
                }
            }

            // ===== NOTES SECTION =====
            double notesY = tableY + 260;

            if (!string.IsNullOrEmpty(invoice.CustomerNotes))
            {
                gfx.DrawString("NOTES", fontHeader, new XSolidBrush(XColor.FromArgb(138, 43, 226)),
                    new XRect(margin, notesY, 100, 18), XStringFormats.TopLeft);
                notesY += 20;

                var notes = WrapTextForPdf(invoice.CustomerNotes, 90);
                foreach (var line in notes)
                {
                    if (notesY > pageHeight - 80) break;
                    gfx.DrawString(line, fontSmall, XBrushes.Gray,
                        new XRect(margin, notesY, pageWidth - 2 * margin, 14), XStringFormats.TopLeft);
                    notesY += 14;
                }
            }

            // ===== FOOTER =====
            var footerY = pageHeight - 50;
            var footerText = !string.IsNullOrEmpty(invoice.FooterNote) ? invoice.FooterNote : "Thank you for your business!";

            gfx.DrawLine(new XPen(XColors.LightGray, 0.5), margin, footerY - 5, pageWidth - margin, footerY - 5);
            gfx.DrawString(footerText, fontSmall, XBrushes.Gray,
                new XRect(margin, footerY, pageWidth - 2 * margin, 15), XStringFormats.TopCenter);

            // Payment instructions
            if (!string.IsNullOrEmpty(invoice.PaymentMethod))
            {
                gfx.DrawString($"Payment Method: {invoice.PaymentMethod}", fontVerySmall, XBrushes.LightGray,
                    new XRect(margin, footerY + 15, 200, 12), XStringFormats.TopLeft);
            }
        }
        private List<string> WrapTextForPdf(string text, int maxCharsPerLine)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text)) return lines;

            // Split by new lines first
            var paragraphs = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var paragraph in paragraphs)
            {
                if (paragraph.Length <= maxCharsPerLine)
                {
                    lines.Add(paragraph);
                    continue;
                }

                var words = paragraph.Split(' ');
                var currentLine = new StringBuilder();

                foreach (var word in words)
                {
                    if (currentLine.Length + word.Length + 1 <= maxCharsPerLine)
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
            }

            return lines;
        }
        private string NumberToWords(decimal number, string currencyCode)
        {
            if (number == 0)
                return "Zero";

            var dollars = (long)Math.Floor(number);
            var cents = (int)((number - dollars) * 100);

            var mainPart = NumberToWordsInternal(dollars);
            var currencyName = GetCurrencyName(mainPart, dollars, currencyCode);
            var result = mainPart + " " + currencyName;

            if (cents > 0)
            {
                var centsPart = NumberToWordsInternal(cents);
                var centName = GetCentName(cents, currencyCode);
                result += " AND " + centsPart + " " + centName;
            }

            return result;
        }
        private string GetCurrencyName(string amountText, long amount, string currencyCode)
        {
            var isPlural = amount != 1;

            return currencyCode.ToUpper() switch
            {
                "USD" => isPlural ? "US DOLLARS" : "US DOLLAR",
                "EUR" => isPlural ? "EUROS" : "EURO",
                "GBP" => isPlural ? "POUNDS" : "POUND",
                "INR" => isPlural ? "RUPEES" : "RUPEE",
                "JPY" => isPlural ? "YEN" : "YEN", // Yen doesn't change for plural
                "CAD" => isPlural ? "CANADIAN DOLLARS" : "CANADIAN DOLLAR",
                "AUD" => isPlural ? "AUSTRALIAN DOLLARS" : "AUSTRALIAN DOLLAR",
                "CHF" => isPlural ? "SWISS FRANCS" : "SWISS FRANC",
                "CNY" => isPlural ? "YUAN" : "YUAN",
                "NZD" => isPlural ? "NEW ZEALAND DOLLARS" : "NEW ZEALAND DOLLAR",
                "SGD" => isPlural ? "SINGAPORE DOLLARS" : "SINGAPORE DOLLAR",
                "HKD" => isPlural ? "HONG KONG DOLLARS" : "HONG KONG DOLLAR",
                "KRW" => isPlural ? "WON" : "WON",
                "RUB" => isPlural ? "RUBLES" : "RUBLE",
                "BRL" => isPlural ? "REAIS" : "REAL",
                "ZAR" => isPlural ? "RAND" : "RAND",
                _ => isPlural ? "UNITS" : "UNIT"
            };
        }
        private string GetCentName(int cents, string currencyCode)
        {
            var isPlural = cents != 1;

            return currencyCode.ToUpper() switch
            {
                "USD" => isPlural ? "CENTS" : "CENT",
                "EUR" => isPlural ? "CENTS" : "CENT",
                "GBP" => isPlural ? "PENCE" : "PENNY",
                "INR" => isPlural ? "PAISE" : "PAISA",
                "JPY" => "", // Yen doesn't have subunits in practice
                "CAD" => isPlural ? "CENTS" : "CENT",
                "AUD" => isPlural ? "CENTS" : "CENT",
                "CHF" => isPlural ? "RAPPEN" : "RAPPEN",
                "CNY" => isPlural ? "JIAO" : "JIAO",
                "NZD" => isPlural ? "CENTS" : "CENT",
                "SGD" => isPlural ? "CENTS" : "CENT",
                "HKD" => isPlural ? "CENTS" : "CENT",
                "KRW" => "", // Won doesn't have subunits
                "RUB" => isPlural ? "KOPECKS" : "KOPECK",
                "BRL" => isPlural ? "CENTAVOS" : "CENTAVO",
                "ZAR" => isPlural ? "CENTS" : "CENT",
                _ => isPlural ? "CENTS" : "CENT"
            };
        }
        private string NumberToWordsInternal(long number)
        {
            if (number == 0)
                return "Zero";

            var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 20)
                return unitsMap[number];

            if (number < 100)
                return tensMap[number / 10] + (number % 10 > 0 ? " " + unitsMap[number % 10] : "");

            if (number < 1000)
                return unitsMap[number / 100] + " Hundred" + (number % 100 > 0 ? " " + NumberToWordsInternal(number % 100) : "");

            if (number < 1000000)
                return NumberToWordsInternal(number / 1000) + " Thousand" + (number % 1000 > 0 ? " " + NumberToWordsInternal(number % 1000) : "");

            if (number < 1000000000)
                return NumberToWordsInternal(number / 1000000) + " Million" + (number % 1000000 > 0 ? " " + NumberToWordsInternal(number % 1000000) : "");

            return NumberToWordsInternal(number / 1000000000) + " Billion" + (number % 1000000000 > 0 ? " " + NumberToWordsInternal(number % 1000000000) : "");
        }
        private string GetCurrencySymbol(string currency)
        {
            return currency.ToUpper() switch
            {
                "USD" => "$",
                "EUR" => "€",
                "GBP" => "£",
                "INR" => "₹",
                "JPY" => "¥",
                "CAD" => "C$",
                "AUD" => "A$",
                "CHF" => "CHF",
                "CNY" => "¥",
                "NZD" => "NZ$",
                "SGD" => "S$",
                "HKD" => "HK$",
                "KRW" => "₩",
                "RUB" => "₽",
                "BRL" => "R$",
                "ZAR" => "R",
                _ => "$"
            };
        }
        private string GetCurrencyDisplayName(string currencyCode)
        {
            return currencyCode.ToUpper() switch
            {
                "USD" => "US Dollar",
                "EUR" => "Euro",
                "GBP" => "British Pound",
                "INR" => "Indian Rupee",
                "JPY" => "Japanese Yen",
                "CAD" => "Canadian Dollar",
                "AUD" => "Australian Dollar",
                "CHF" => "Swiss Franc",
                "CNY" => "Chinese Yuan",
                "NZD" => "New Zealand Dollar",
                "SGD" => "Singapore Dollar",
                "HKD" => "Hong Kong Dollar",
                "KRW" => "South Korean Won",
                "RUB" => "Russian Ruble",
                "BRL" => "Brazilian Real",
                "ZAR" => "South African Rand",
                _ => currencyCode
            };
        }
    }
}