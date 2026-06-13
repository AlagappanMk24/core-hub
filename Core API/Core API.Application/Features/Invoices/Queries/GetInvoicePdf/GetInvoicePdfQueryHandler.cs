using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Companies;
using Core_API.Application.DTOs.Companies.Responses;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Text;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoicePdf;

/// <summary>
/// Handler for GetInvoicePdfQuery
/// </summary>
public sealed class GetInvoicePdfQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetInvoicePdfQueryHandler> logger,
    ICompanyService companyService) : IRequestHandler<GetInvoicePdfQuery, OperationResult<InvoiceResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<GetInvoicePdfQueryHandler> _logger = logger;
    private readonly ICompanyService _companyService = companyService;

    public async Task<OperationResult<InvoiceResponseDto>> Handle(
        GetInvoicePdfQuery request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        try
        {
            _logger.LogInformation("Generating PDF for invoice {InvoiceId}, UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}, CompanyId: {CompanyId}",
                request.Id, context.UserId, context.IsSuperAdmin, context.CompanyId);

            // Build query with proper filtering
            IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query()
                .Where(i => !i.IsDeleted);

            // Super Admin can access any invoice
            if (context.IsSuperAdmin)
            {
                _logger.LogInformation("Super Admin generating invoice PDF for invoice {InvoiceId}", request.Id);
                // No company filter for Super Admin
            }
            else if (context.CompanyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == context.CompanyId.Value);
            }
            else
            {
                _logger.LogWarning("Company ID is required for non-super admin users.");
                return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
            }

            // Apply customer filter if user is a customer
            if (context.CustomerId.HasValue)
            {
                query = query.Where(i => i.CustomerId == context.CustomerId.Value);
            }

            // Get invoice with all related data
            var invoice = await query
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .Include(i => i.TaxDetails)
                .Include(i => i.Discounts)
                .Include(i => i.InvoiceAttachments)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to user", request.Id);
                return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
            }

            // Check customer access if user is a customer
            if (context.CustomerId.HasValue && invoice.CustomerId != context.CustomerId.Value)
            {
                _logger.LogWarning("Customer {CustomerId} attempted to access invoice {InvoiceId} belonging to customer {InvoiceCustomerId}",
                    context.CustomerId, request.Id, invoice.CustomerId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Access denied to this invoice.");
            }

            // Get company information for PDF header
            var companyInfo = await _companyService.GetCompanyByIdAsync(invoice.CompanyId);

            // Generate PDF
            var pdfStream = await GenerateInvoicePdf(invoice, companyInfo);

            if (pdfStream == null || pdfStream.Length == 0)
            {
                _logger.LogError("Failed to generate PDF for invoice {InvoiceId}", request.Id);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to generate PDF.");
            }

            // Map to response DTO
            var response = _mapper.Map<InvoiceResponseDto>(invoice);
            response.Customer = _mapper.Map<CustomerResponseDto>(invoice.Customer);
            response.PdfStream = pdfStream;

            _logger.LogInformation("PDF generated successfully for invoice {InvoiceNumber} (ID: {InvoiceId})",
                invoice.InvoiceNumber, invoice.Id);

            return OperationResult<InvoiceResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}", request.Id);
            return OperationResult<InvoiceResponseDto>.FailureResult("Failed to generate invoice PDF: " + ex.Message);
        }
    }

    #region Private PDF Generation Methods

    private async Task<MemoryStream> GenerateInvoicePdf(Domain.Entities.Invoices.Invoice invoice, CompanyResponseDto? companyInfo)
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        page.Size = PageSize.A4;
        page.Orientation = PageOrientation.Portrait;

        var gfx = XGraphics.FromPdfPage(page);

        DrawInvoice(gfx, page, invoice, companyInfo);

        var stream = new MemoryStream();
        document.Save(stream);
        stream.Position = 0;
        return stream;
    }

    private void DrawInvoice(XGraphics gfx, PdfPage page, Domain.Entities.Invoices.Invoice invoice, CompanyResponseDto? companyInfo)
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
            var address = $"{companyInfo.Address.AddressLine1 ?? ""} {companyInfo.Address.AddressLine2 ?? ""}".Trim();
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

            if (!string.IsNullOrEmpty(companyInfo.Address.CountryCode))
            {
                gfx.DrawString(companyInfo.Address.CountryCode, fontSmall, XBrushes.Gray,
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

        // Fill background
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
            if (!string.IsNullOrEmpty(addr.AddressLine1))
            {
                gfx.DrawString(addr.AddressLine1, fontNormal, XBrushes.Black,
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

            if (!string.IsNullOrEmpty(addr.CountryCode))
            {
                gfx.DrawString(addr.CountryCode, fontNormal, XBrushes.Black,
                    new XRect(margin, y, 250, 16), XStringFormats.TopLeft);
                y += 16;
            }
        }

        // Customer Contact
        if (!string.IsNullOrEmpty(invoice.Customer?.Email?.Value))
        {
            gfx.DrawString(invoice.Customer.Email.Value, fontNormal, XBrushes.Black,
                new XRect(margin, y, 250, 16), XStringFormats.TopLeft);
            y += 16;
        }

        if (!string.IsNullOrEmpty(invoice.Customer?.PhoneNumber?.Value))
        {
            gfx.DrawString(invoice.Customer.PhoneNumber.Value, fontNormal, XBrushes.Black,
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

        double[] colWidths = { 280, 50, 60, 60, 70, 70 };
        double[] colX = {
            margin + 10, margin + 190, margin + 250,
            margin + 320, margin + 380, margin + 440
        };

        string[] headers = { "Description", "Qty", "Unit Price", "Tax %", "Tax", "Amount" };
        XStringFormat[] alignments = {
            XStringFormats.TopLeft, XStringFormats.TopCenter, XStringFormats.TopCenter,
            XStringFormats.TopCenter, XStringFormats.TopCenter, XStringFormats.TopRight
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

        // Draw items
        foreach (var item in invoice.InvoiceItems)
        {
            var descriptionLines = WrapTextForPdf(item.Description ?? "", 45);
            var rowHeight = Math.Max(defaultRowHeight, descriptionLines.Count * 14);

            // Check page break
            if (tableY + rowHeight > pageHeight - 150)
            {
                page = ((PdfDocument)page.Owner).AddPage();
                gfx = XGraphics.FromPdfPage(page);
                tableY = 40;

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

            // Item Description
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

        tableY += 10;

        // ===== SUMMARY SECTION =====
        double summaryX = pageWidth - margin - 220;
        double summaryY = tableY + 30;
        double summaryWidth = 220;

        // Draw summary box
        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 248, 248)),
            summaryX - 10, summaryY - 5, summaryWidth + 10, 180);

        // Subtotal
        gfx.DrawString("Subtotal:", fontNormal, XBrushes.Gray,
            new XRect(summaryX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
        gfx.DrawString($"{currencySymbol}{invoice.Subtotal:N2}", fontNormal, XBrushes.Black,
            new XRect(summaryX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
        summaryY += 20;

        // Discount
        if (invoice.DiscountTotal > 0)
        {
            gfx.DrawString("Discount:", fontNormal, XBrushes.Gray,
                new XRect(summaryX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
            gfx.DrawString($"-{currencySymbol}{invoice.DiscountTotal:N2}", fontNormal, new XSolidBrush(XColor.FromArgb(220, 53, 69)),
                new XRect(summaryX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
            summaryY += 20;
        }

        // Tax
        if (invoice.TaxTotal > 0)
        {
            gfx.DrawString("Tax:", fontNormal, XBrushes.Gray,
                new XRect(summaryX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
            gfx.DrawString($"{currencySymbol}{invoice.TaxTotal:N2}", fontNormal, XBrushes.Black,
                new XRect(summaryX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
            summaryY += 20;
        }

        // Shipping
        if (invoice.ShippingAmount > 0)
        {
            gfx.DrawString("Shipping:", fontNormal, XBrushes.Gray,
                new XRect(summaryX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
            gfx.DrawString($"{currencySymbol}{invoice.ShippingAmount:N2}", fontNormal, XBrushes.Black,
                new XRect(summaryX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
            summaryY += 20;
        }

        // Adjustment
        if (invoice.AdjustmentAmount != 0)
        {
            var adjustmentColor = invoice.AdjustmentAmount > 0
                ? new XSolidBrush(XColor.FromArgb(220, 53, 69))
                : new XSolidBrush(XColor.FromArgb(40, 167, 69));
            var adjustmentSign = invoice.AdjustmentAmount > 0 ? "+" : "";
            var adjustmentLabel = !string.IsNullOrEmpty(invoice.AdjustmentDescription)
                ? $"Adjustment ({invoice.AdjustmentDescription}):"
                : "Adjustment:";

            gfx.DrawString(adjustmentLabel, fontNormal, XBrushes.Gray,
                new XRect(summaryX, summaryY + 5, 120, 18), XStringFormats.TopLeft);
            gfx.DrawString($"{adjustmentSign}{currencySymbol}{Math.Abs(invoice.AdjustmentAmount):N2}", fontNormal, adjustmentColor,
                new XRect(summaryX + 120, summaryY + 5, 90, 18), XStringFormats.TopRight);
            summaryY += 20;
        }

        // Separator line
        gfx.DrawLine(new XPen(XColors.LightGray, 0.5), summaryX, summaryY, summaryX + summaryWidth, summaryY);
        summaryY += 10;

        // Total
        var totalBg = new XSolidBrush(XColor.FromArgb(138, 43, 226));
        gfx.DrawRectangle(totalBg, summaryX, summaryY, summaryWidth, 32);
        gfx.DrawString("TOTAL", fontBold, XBrushes.White,
            new XRect(summaryX + 10, summaryY + 8, 80, 18), XStringFormats.TopLeft);
        gfx.DrawString($"{currencySymbol}{invoice.TotalAmount:N2}", fontBold, XBrushes.White,
            new XRect(summaryX + summaryWidth - 20, summaryY + 8, 10, 18), XStringFormats.TopRight);
        summaryY += 40;

        // Total in Words
        gfx.DrawString("Total in Words:", fontBold, XBrushes.Black,
            new XRect(summaryX, summaryY, 120, 18), XStringFormats.TopLeft);

        var wordsLines = WrapTextForPdf(totalInWords, 25);
        double wordsY = summaryY;
        foreach (var line in wordsLines)
        {
            gfx.DrawString(line, fontItalic, new XSolidBrush(XColor.FromArgb(138, 43, 226)),
                new XRect(summaryX + 100, wordsY, summaryWidth - 100, 16), XStringFormats.TopLeft);
            wordsY += 16;
        }

        // Amount Paid
        if (invoice.AmountPaid > 0)
        {
            summaryY = Math.Max(summaryY + 30, wordsY + 10);
            gfx.DrawString("Amount Paid:", fontNormal, XBrushes.Gray,
                new XRect(summaryX, summaryY, 120, 18), XStringFormats.TopLeft);
            gfx.DrawString($"{currencySymbol}{invoice.AmountPaid:N2}", fontNormal, new XSolidBrush(XColor.FromArgb(40, 167, 69)),
                new XRect(summaryX + 120, summaryY, 90, 18), XStringFormats.TopRight);
            summaryY += 20;

            var balanceDue = invoice.TotalAmount - invoice.AmountPaid;
            if (balanceDue > 0)
            {
                gfx.DrawString("Balance Due:", fontBold, XBrushes.Gray,
                    new XRect(summaryX, summaryY, 120, 18), XStringFormats.TopLeft);
                gfx.DrawString($"{currencySymbol}{balanceDue:N2}", fontBold, new XSolidBrush(XColor.FromArgb(220, 53, 69)),
                    new XRect(summaryX + 120, summaryY, 90, 18), XStringFormats.TopRight);
            }
        }

        // ===== NOTES SECTION =====
        double notesY = tableY + 280;

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

        if (!string.IsNullOrEmpty(invoice.PaymentMethod))
        {
            gfx.DrawString($"Payment Method: {invoice.PaymentMethod}", fontVerySmall, XBrushes.LightGray,
                new XRect(margin, footerY + 15, 200, 12), XStringFormats.TopLeft);
        }
    }

    #endregion

    #region Helper Methods

    private List<string> WrapTextForPdf(string text, int maxCharsPerLine)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text)) return lines;

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
            if (!string.IsNullOrEmpty(centName))
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
            "JPY" => "YEN",
            "CAD" => isPlural ? "CANADIAN DOLLARS" : "CANADIAN DOLLAR",
            "AUD" => isPlural ? "AUSTRALIAN DOLLARS" : "AUSTRALIAN DOLLAR",
            "CHF" => isPlural ? "SWISS FRANCS" : "SWISS FRANC",
            "CNY" => "YUAN",
            "NZD" => isPlural ? "NEW ZEALAND DOLLARS" : "NEW ZEALAND DOLLAR",
            "SGD" => isPlural ? "SINGAPORE DOLLARS" : "SINGAPORE DOLLAR",
            "HKD" => isPlural ? "HONG KONG DOLLARS" : "HONG KONG DOLLAR",
            "KRW" => "WON",
            "RUB" => isPlural ? "RUBLES" : "RUBLE",
            "BRL" => isPlural ? "REAIS" : "REAL",
            "ZAR" => "RAND",
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
            "JPY" => "",
            "CAD" => isPlural ? "CENTS" : "CENT",
            "AUD" => isPlural ? "CENTS" : "CENT",
            "CHF" => isPlural ? "RAPPEN" : "RAPPEN",
            "CNY" => "JIAO",
            "NZD" => isPlural ? "CENTS" : "CENT",
            "SGD" => isPlural ? "CENTS" : "CENT",
            "HKD" => isPlural ? "CENTS" : "CENT",
            "KRW" => "",
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

    #endregion
}