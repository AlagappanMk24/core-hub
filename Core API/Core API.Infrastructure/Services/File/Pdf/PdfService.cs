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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.Globalization;

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
                // Fetching invoice data with related entities
                var invoice = await _unitOfWork.Invoices.GetAsync(
                    i => i.Id == id && i.CompanyId == operationContext.CompanyId && !i.IsDeleted,
                    "InvoiceItems,TaxDetails,Discounts,Customer"
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
        public async Task<OperationResult<byte[]>> ExportInvoicesPdfAsync(OperationContext operationContext, [FromQuery] InvoiceFilterRequestDto invoiceFilterRequestDto)
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

                var document = new PdfDocument();
                var font = new XFont("Open Sans", 12, XFontStyleEx.Regular);
                var titleFont = new XFont("Open Sans", 16, XFontStyleEx.Bold);
                var smallFont = new XFont("Open Sans", 10, XFontStyleEx.Regular);

                foreach (var invoice in result.Items)
                {
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    var currency = invoice.Currency ?? "USD";
                    var yPoint = 40;

                    // Invoice Header
                    gfx.DrawString($"Invoice #{invoice.InvoiceNumber}", titleFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 40;

                    // Invoice Details
                    gfx.DrawString($"PO Number: {invoice.PONumber ?? "N/A"}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    gfx.DrawString($"Issue Date: {invoice.IssueDate:dd MMM yyyy}", smallFont, XBrushes.Black, new XRect(300, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    gfx.DrawString($"Due Date: {invoice.PaymentDue:dd MMM yyyy}", smallFont, XBrushes.Black, new XRect(300, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    gfx.DrawString($"Status: {invoice.InvoiceStatus}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    gfx.DrawString($"Invoice Type: {invoice.InvoiceType}", smallFont, XBrushes.Black, new XRect(300, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    gfx.DrawString($"Currency: {currency}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 40;

                    // Customer Information
                    gfx.DrawString("Customer Details", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                    yPoint += 10;
                    gfx.DrawString($"Name: {invoice.Customer?.Name ?? "Unknown"}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    gfx.DrawString($"Email: {invoice.Customer?.Email ?? "N/A"}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    gfx.DrawString($"Phone: {invoice.Customer?.PhoneNumber ?? "N/A"}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    if (invoice.Customer?.Address != null)
                    {
                        var address = invoice.Customer.Address;
                        var addressLines = new List<string>();
                        if (!string.IsNullOrEmpty(address.Address1)) addressLines.Add(address.Address1);
                        if (!string.IsNullOrEmpty(address.Address2)) addressLines.Add(address.Address2);
                        if (!string.IsNullOrEmpty(address.City)) addressLines.Add(address.City);
                        if (!string.IsNullOrEmpty(address.State)) addressLines.Add(address.State);
                        if (!string.IsNullOrEmpty(address.Country)) addressLines.Add(address.Country);
                        if (!string.IsNullOrEmpty(address.ZipCode)) addressLines.Add(address.ZipCode);

                        if (addressLines.Count != 0)
                        {
                            gfx.DrawString($"Address: {string.Join(", ", addressLines)}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                            yPoint += 20;
                        }
                    }
                    yPoint += 20;

                    // Invoice Items
                    if (invoice.InvoiceItems != null && invoice.InvoiceItems.Count != 0)
                    {
                        gfx.DrawString("Invoice Items", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                        gfx.DrawString("Description", font, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Qty", font, XBrushes.Black, new XRect(240, yPoint, 50, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Unit Price", font, XBrushes.Black, new XRect(290, yPoint, 70, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Tax", font, XBrushes.Black, new XRect(360, yPoint, 70, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Amount", font, XBrushes.Black, new XRect(430, yPoint, 100, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                        gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                        yPoint += 10;

                        foreach (var item in invoice.InvoiceItems)
                        {
                            if (yPoint > page.Height - 60)
                            {
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                yPoint = 40;
                                gfx.DrawString("Invoice Items (Continued)", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                                yPoint += 20;
                                gfx.DrawString("Description", font, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Qty", font, XBrushes.Black, new XRect(240, yPoint, 50, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Unit Price", font, XBrushes.Black, new XRect(290, yPoint, 70, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Tax", font, XBrushes.Black, new XRect(360, yPoint, 70, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Amount", font, XBrushes.Black, new XRect(430, yPoint, 100, 20), XStringFormats.TopLeft);
                                yPoint += 20;
                                gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                                yPoint += 10;
                            }

                            gfx.DrawString(item.Description, smallFont, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                            gfx.DrawString(item.Quantity.ToString(), smallFont, XBrushes.Black, new XRect(240, yPoint, 50, 20), XStringFormats.TopLeft);
                            gfx.DrawString($"{item.UnitPrice:F2} {currency}", smallFont, XBrushes.Black, new XRect(290, yPoint, 70, 20), XStringFormats.TopLeft);
                            gfx.DrawString(item.TaxAmount > 0 ? $"{item.TaxAmount:F2} {currency}" : "-", smallFont, XBrushes.Black, new XRect(360, yPoint, 70, 20), XStringFormats.TopLeft);
                            gfx.DrawString($"{item.Amount:F2} {currency}", smallFont, XBrushes.Black, new XRect(430, yPoint, 100, 20), XStringFormats.TopLeft);
                            yPoint += 20;
                        }
                        yPoint += 20;
                    }

                    // Tax Details
                    if (invoice.TaxDetails != null && invoice.TaxDetails.Count != 0)
                    {
                        gfx.DrawString("Tax Details", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                        gfx.DrawString("Tax Type", font, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Rate (%)", font, XBrushes.Black, new XRect(240, yPoint, 100, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Amount", font, XBrushes.Black, new XRect(340, yPoint, 100, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                        gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                        yPoint += 10;

                        foreach (var tax in invoice.TaxDetails)
                        {
                            if (yPoint > page.Height - 60)
                            {
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                yPoint = 40;
                                gfx.DrawString("Tax Details (Continued)", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                                yPoint += 20;
                                gfx.DrawString("Tax Type", font, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Rate (%)", font, XBrushes.Black, new XRect(240, yPoint, 100, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Amount", font, XBrushes.Black, new XRect(340, yPoint, 100, 20), XStringFormats.TopLeft);
                                yPoint += 20;
                                gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                                yPoint += 10;
                            }

                            gfx.DrawString(tax.TaxType, smallFont, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                            gfx.DrawString($"{tax.Rate:F2}", smallFont, XBrushes.Black, new XRect(240, yPoint, 100, 20), XStringFormats.TopLeft);
                            gfx.DrawString($"{tax.Amount:F2} {currency}", smallFont, XBrushes.Black, new XRect(340, yPoint, 100, 20), XStringFormats.TopLeft);
                            yPoint += 20;
                        }
                        yPoint += 20;
                    }

                    // Discounts
                    if (invoice.Discounts != null && invoice.Discounts.Count != 0)
                    {
                        gfx.DrawString("Discounts", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                        gfx.DrawString("Description", font, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Amount", font, XBrushes.Black, new XRect(240, yPoint, 100, 20), XStringFormats.TopLeft);
                        gfx.DrawString("Is Percentage", font, XBrushes.Black, new XRect(340, yPoint, 100, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                        gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                        yPoint += 10;

                        foreach (var discount in invoice.Discounts)
                        {
                            if (yPoint > page.Height - 60)
                            {
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                yPoint = 40;
                                gfx.DrawString("Discounts (Continued)", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                                yPoint += 20;
                                gfx.DrawString("Description", font, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Amount", font, XBrushes.Black, new XRect(240, yPoint, 100, 20), XStringFormats.TopLeft);
                                gfx.DrawString("Is Percentage", font, XBrushes.Black, new XRect(340, yPoint, 100, 20), XStringFormats.TopLeft);
                                yPoint += 20;
                                gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                                yPoint += 10;
                            }

                            gfx.DrawString(discount.Description, smallFont, XBrushes.Black, new XRect(40, yPoint, 200, 20), XStringFormats.TopLeft);
                            gfx.DrawString($"{discount.Amount:F2}{(discount.IsPercentage ? "%" : $" {currency}")}", smallFont, XBrushes.Black, new XRect(240, yPoint, 100, 20), XStringFormats.TopLeft);
                            gfx.DrawString(discount.IsPercentage ? "Yes" : "No", smallFont, XBrushes.Black, new XRect(340, yPoint, 100, 20), XStringFormats.TopLeft);
                            yPoint += 20;
                        }
                        yPoint += 20;
                    }

                    // Totals
                    gfx.DrawString($"Subtotal: {invoice.Subtotal:F2} {currency}", font, XBrushes.Black, new XRect(300, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    gfx.DrawString($"Tax: {invoice.Tax:F2} {currency}", font, XBrushes.Black, new XRect(300, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;
                    if (invoice.Discounts.Count != 0)
                    {
                        var discountAmount = invoice.Discounts.Sum(d => d.IsPercentage ? invoice.Subtotal * d.Amount / 100 : d.Amount);
                        gfx.DrawString($"Discount: {discountAmount:F2} {currency}", font, XBrushes.Black, new XRect(300, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                    }
                    gfx.DrawString($"Total: {invoice.TotalAmount:F2} {currency}", font, XBrushes.Black, new XRect(300, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPoint += 20;

                    // Additional Details
                    if (!string.IsNullOrEmpty(invoice.Notes))
                    {
                        gfx.DrawString("Notes", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                        gfx.DrawString(invoice.Notes, smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                    }
                    if (!string.IsNullOrEmpty(invoice.PaymentMethod))
                    {
                        gfx.DrawString($"Payment Method: {invoice.PaymentMethod}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                    }
                    if (!string.IsNullOrEmpty(invoice.ProjectDetail))
                    {
                        gfx.DrawString($"Project Detail: {invoice.ProjectDetail}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPoint += 20;
                    }
                    gfx.DrawString($"Automated: {(invoice.IsAutomated ? "Yes" : "No")}", smallFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, 20), XStringFormats.TopLeft);
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
                // Create a new PDF document
                using var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

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
                const double sectionSpacing = 25; // Spacing between major sections

                // Color scheme - Blue theme like the image
                var primaryViolet = XColor.FromArgb(167, 139, 250); // #a78bfa
                var secondaryViolet = XColor.FromArgb(196, 181, 253); // #c4b5fd
                var lightViolet = XColor.FromArgb(230, 220, 255); // A lighter shade for backgrounds
                var darkGray = XColor.FromArgb(64, 64, 64); // Dark gray text
                var lightGray = XColor.FromArgb(128, 128, 128); // Light gray text
                var accentWhite = XColor.FromArgb(255, 255, 255); // #ffffff

                // Brushes for convenience
                var primaryVioletBrush = new XSolidBrush(primaryViolet);
                var secondaryVioletBrush = new XSolidBrush(secondaryViolet);
                var lightVioletBrush = new XSolidBrush(lightViolet);
                var darkGrayBrush = new XSolidBrush(darkGray);
                var lightGrayBrush = new XSolidBrush(lightGray);
                var accentWhiteBrush = new XSolidBrush(accentWhite);
                var blackBrush = XBrushes.Black;

                // Get culture for currency formatting
                var culture = "invoice.Currency" switch
                {
                    "EUR" => new CultureInfo("de-DE"),
                    "INR" => new CultureInfo("hi-IN"),
                    _ => new CultureInfo("en-US")
                };

                // Header Section
                // Company info
                var companyInfo = await _companyService.GetCompanyByIdAsync(invoice.CompanyId);
                double companyY = y + 10;
                gfx.DrawString(companyInfo.Name, new XFont("Open Sans", 16, XFontStyleEx.Bold), primaryVioletBrush,
                    new XRect(margin, y, 200, 20), XStringFormats.TopLeft);
                companyY += lineHeight;
                gfx.DrawString("OUR FOCUS, YOUR FUTURE", new XFont("Open Sans", 8, XFontStyleEx.Regular), lightGrayBrush,
                    new XRect(margin, y + 20, 200, 12), XStringFormats.TopLeft);
                companyY += lineHeight;
                if (companyInfo?.Address != null)
                {
                    var address = $"{companyInfo.Address.Address1 ?? ""} {companyInfo.Address.Address2 ?? ""}".Trim();
                    if (!string.IsNullOrEmpty(address))
                    {
                        gfx.DrawString(address, fontSmall, darkGrayBrush,
                            new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                        companyY += lineHeight;
                    }
                    // This line combines the city, state, and zip code
                    var cityStateZipForCompanySection = $"{companyInfo.Address.City ?? ""}, {companyInfo.Address.State ?? ""} {companyInfo.Address.ZipCode ?? ""}".Trim(',', ' ');
                    if (!string.IsNullOrEmpty(cityStateZipForCompanySection))
                    {
                        gfx.DrawString(cityStateZipForCompanySection, fontSmall, darkGrayBrush,
                            new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                        companyY += lineHeight;
                    }
                    var country = companyInfo.Address.Country ?? "";
                    if (!string.IsNullOrEmpty(country))
                    {
                        gfx.DrawString(country, fontSmall, darkGrayBrush,
                            new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                        companyY += lineHeight;
                    }
                }
                if (!string.IsNullOrEmpty(companyInfo?.Email))
                {
                    gfx.DrawString(companyInfo.Email, fontSmall, darkGrayBrush,
                        new XRect(margin, companyY, 250, lineHeight), XStringFormats.TopLeft);
                    companyY += lineHeight;
                }

                // Invoice info
                var invoiceHeaderX = page.Width - margin - 200;
                gfx.DrawString("INVOICE", fontTitle, primaryVioletBrush,
                    new XRect(invoiceHeaderX, y, 200, 30), XStringFormats.TopRight);

                y += 35;
                gfx.DrawString($"Invoice No. #{invoice.InvoiceNumber}", fontRegular, blackBrush,
                    new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
                y += lineHeight;
                gfx.DrawString($"Issue Date: {invoice.IssueDate:dd MMM yyyy}", fontRegular, blackBrush,
                    new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);
                y += lineHeight;
                gfx.DrawString($"Due Date: {invoice.PaymentDue:dd MMM yyyy}", fontRegular, blackBrush,
                    new XRect(invoiceHeaderX, y, 200, lineHeight), XStringFormats.TopRight);

                // 3. Horizontal line is full of below of company info and invoice info
                // Calculate the maximum Y position between company info and invoice info
                double maxHeaderY = Math.Max(companyY, y) + 20; // Add some padding
                gfx.DrawLine(new XPen(primaryViolet, 1), margin, maxHeaderY, page.Width - margin, maxHeaderY);

                y = maxHeaderY + sectionSpacing; // Start next section below the line


                // Client Information Section
                // Bill To section
                gfx.DrawString("Bill To:", fontHeader, primaryVioletBrush,
                    new XRect(margin, y, 200, lineHeight), XStringFormats.TopLeft);
                y += lineHeight + 2;

                gfx.DrawString(invoice.Customer?.Name ?? "Customer Name", fontBold, XBrushes.Black,
                    new XRect(margin, y, 250, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;

                // Line 2: Address1 + Address2 (if present)
                var addressLine = invoice.Customer?.Address?.Address1;
                if (!string.IsNullOrWhiteSpace(invoice.Customer?.Address?.Address2))
                    addressLine += $", {invoice.Customer.Address.Address2}";
                if (!string.IsNullOrWhiteSpace(addressLine))
                {
                    gfx.DrawString(addressLine, fontRegular, darkGrayBrush,
                        new XRect(margin, y, 300, lineHeight), XStringFormats.TopLeft);
                    y += lineHeight;
                }
                // Line 3: City, State ZIP
                var cityStateZip = $"{invoice.Customer?.Address?.City}, {invoice.Customer?.Address?.State} {invoice.Customer?.Address?.ZipCode}";
                gfx.DrawString(cityStateZip, fontRegular, darkGrayBrush,
                    new XRect(margin, y, 300, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;


                // Line 4: Country (optional)
                if (!string.IsNullOrWhiteSpace(invoice.Customer?.Address?.Country))
                {
                    gfx.DrawString(invoice.Customer.Address.Country, fontRegular, darkGrayBrush,
                        new XRect(margin, y, 300, lineHeight), XStringFormats.TopLeft);
                    y += lineHeight;
                }

                // Line 5: Email + Phone (inline if possible)
                var contactLine = "";
                if (!string.IsNullOrWhiteSpace(invoice.Customer?.Email))
                    contactLine += $"Email: {invoice.Customer.Email}";

                if (!string.IsNullOrWhiteSpace(invoice.Customer?.PhoneNumber))
                {
                    if (!string.IsNullOrWhiteSpace(contactLine)) contactLine += " | ";
                    contactLine += $"Phone: {invoice.Customer.PhoneNumber}";
                }

                if (!string.IsNullOrWhiteSpace(contactLine))
                {
                    gfx.DrawString(contactLine, fontRegular, darkGrayBrush,
                        new XRect(margin, y, 400, lineHeight), XStringFormats.TopLeft);
                    y += lineHeight;
                }

                // Total Amount Box (top right)
                var totalBoxX = page.Width - margin - 150;
                var totalBoxY = y - (invoice.Customer?.Address?.Country != null ? 100 : 60);
                gfx.DrawRectangle(secondaryVioletBrush, totalBoxX, totalBoxY, 150, 50);
                gfx.DrawString("Total Amount", fontRegular, accentWhiteBrush,
                    new XRect(totalBoxX + 10, totalBoxY + 10, 130, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.TotalAmount.ToString("C", culture), new XFont("Open Sans", 18, XFontStyleEx.Bold), accentWhiteBrush,
                    new XRect(totalBoxX + 10, totalBoxY + 25, 130, 20), XStringFormats.TopLeft);

                y += 30;

                // Items Table
                var tableY = y;
                var tableHeight = 35;

                // Table header with blue background
                gfx.DrawRectangle(primaryVioletBrush, margin, tableY, page.Width - 2 * margin, tableHeight);

                // Column widths
                double[] colWidths = [180, 80, 60, 80, 80];
                double colX = margin + 10;

                // Header text
                gfx.DrawString("Item Description", fontHeader, XBrushes.White,
                    new XRect(colX, tableY + 10, colWidths[0], lineHeight), XStringFormats.TopLeft);
                colX += colWidths[0];
                //gfx.DrawString("Usage Period", fontHeader, XBrushes.White,
                //    new XRect(colX, tableY + 10, colWidths[1], lineHeight), XStringFormats.TopCenter);
                colX += colWidths[1];
                gfx.DrawString("Qty", fontHeader, XBrushes.White,
                    new XRect(colX, tableY + 10, colWidths[2], lineHeight), XStringFormats.TopCenter);
                colX += colWidths[2];
                gfx.DrawString("Unit Price", fontHeader, XBrushes.White,
                    new XRect(colX, tableY + 10, colWidths[3], lineHeight), XStringFormats.TopCenter);
                colX += colWidths[3];
                gfx.DrawString("Total", fontHeader, XBrushes.White,
                    new XRect(colX, tableY + 10, colWidths[4], lineHeight), XStringFormats.TopCenter);

                tableY += tableHeight;

                // Table rows
                int rowIndex = 0;
                foreach (var item in invoice.InvoiceItems)
                {
                    // Check for page overflow
                    if (tableY > page.Height - 150)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        tableY = 30;
                    }

                    var rowHeight = 30;

                    // Alternating row colors
                    if (rowIndex % 2 == 0)
                    {
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 248, 248)),
                            margin, tableY, page.Width - 2 * margin, rowHeight);
                    }

                    colX = margin + 10;

                    // Item data
                    gfx.DrawString(item.Description, fontRegular, XBrushes.Black,
                        new XRect(colX, tableY + 8, colWidths[0], lineHeight), XStringFormats.TopLeft);
                    colX += colWidths[0];
                    //gfx.DrawString("Monthly", fontRegular, XBrushes.Black,
                    //    new XRect(colX, tableY + 8, colWidths[1], lineHeight), XStringFormats.TopCenter);
                    colX += colWidths[1];
                    gfx.DrawString(item.Quantity.ToString(), fontRegular, XBrushes.Black,
                        new XRect(colX, tableY + 8, colWidths[2], lineHeight), XStringFormats.TopCenter);
                    colX += colWidths[2];
                    gfx.DrawString(item.UnitPrice.ToString("C", culture), fontRegular, XBrushes.Black,
                        new XRect(colX, tableY + 8, colWidths[3], lineHeight), XStringFormats.TopCenter);
                    colX += colWidths[3];
                    gfx.DrawString(item.Amount.ToString("C", culture), fontRegular, XBrushes.Black,
                        new XRect(colX, tableY + 8, colWidths[4], lineHeight), XStringFormats.TopCenter);

                    tableY += rowHeight;
                    rowIndex++;
                }

                tableY += 20;

                // Payment Method Section
                if (!string.IsNullOrEmpty(invoice.PaymentMethod))
                {
                    gfx.DrawString("Payment Method", fontHeader, primaryVioletBrush,
                        new XRect(margin, tableY, 200, lineHeight), XStringFormats.TopLeft);
                    tableY += lineHeight + 5;
                    gfx.DrawString(invoice.PaymentMethod, fontRegular, XBrushes.Black,
                        new XRect(margin, tableY, 300, lineHeight), XStringFormats.TopLeft);
                    tableY += lineHeight + 10;
                }

                // 1. Summary section on the right - attractive, impressive, and responsive
                const double summaryBoxWidth = 220; // Wider box for better appearance
                const double summaryPadding = 15;
                double summaryTopY = tableY;

                // Calculate total height needed for summary
                double summaryContentHeight = 0;
                summaryContentHeight += lineHeight; // Sub Total
                summaryContentHeight += invoice.TaxDetails.Count() * lineHeight;
                summaryContentHeight += invoice.Discounts.Count() * lineHeight;
                summaryContentHeight += lineHeight + 10; // Total line plus padding

                // Calculate responsive summary position (bottom-right aligned if space, otherwise after items)
                var summaryX = page.Width - margin - summaryBoxWidth;
                var summaryMinY = page.Height - margin - summaryContentHeight; // Ideal bottom alignment
                var summaryActualY = summaryTopY;
                double summaryBoxBottomY = summaryActualY + summaryContentHeight + 2 * summaryPadding;

                // Draw the outer box for summary
                gfx.DrawRoundedRectangle(new XPen(primaryViolet, 2), accentWhiteBrush, // PrimaryViolet border, white background
                   summaryX, summaryActualY - summaryPadding, summaryBoxWidth, summaryContentHeight + 2 * summaryPadding, 10, 10);

                // Draw summary items inside the box
                double currentSummaryDrawingY = summaryActualY;
                double summaryContentX = summaryX + summaryPadding;
                double summaryContentWidth = summaryBoxWidth - 2 * summaryPadding;

                gfx.DrawString($"Sub Total:", fontRegular, darkGrayBrush,
                    new XRect(summaryContentX, currentSummaryDrawingY, summaryContentWidth, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.Subtotal.ToString("C", culture), fontRegular, blackBrush,
                    new XRect(summaryContentX, currentSummaryDrawingY, summaryContentWidth, lineHeight), XStringFormats.TopRight);
                currentSummaryDrawingY += lineHeight;

                foreach (var tax in invoice.TaxDetails)
                {
                    gfx.DrawString($"Tax ({tax.Rate}%):", fontRegular, darkGrayBrush,
                        new XRect(summaryContentX, currentSummaryDrawingY, summaryContentWidth, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(tax.Amount.ToString("C", culture), fontRegular, blackBrush,
                        new XRect(summaryContentX, currentSummaryDrawingY, summaryContentWidth, lineHeight), XStringFormats.TopRight);
                    currentSummaryDrawingY += lineHeight;
                }

                foreach (var discount in invoice.Discounts)
                {
                    var discountLabel = discount.IsPercentage ? $"Discount ({discount.Amount}%)" : "Discount";
                    gfx.DrawString($"{discountLabel}:", fontRegular, darkGrayBrush,
                        new XRect(summaryContentX, currentSummaryDrawingY, summaryContentWidth, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString($"-{discount.Amount.ToString("C", culture)}", fontRegular, new XSolidBrush(XColor.FromArgb(255, 0, 0)), // Red for discount
                        new XRect(summaryContentX, currentSummaryDrawingY, summaryContentWidth, lineHeight), XStringFormats.TopRight);
                    currentSummaryDrawingY += lineHeight;
                }

                // Separator line before total
                gfx.DrawLine(new XPen(lightGray, 0.5), summaryContentX, currentSummaryDrawingY + 5, summaryContentX + summaryContentWidth, currentSummaryDrawingY + 5);
                currentSummaryDrawingY += 10;

                // Total with primary violet background - now inside the attractive summary box
                gfx.DrawRectangle(primaryVioletBrush, summaryContentX, currentSummaryDrawingY, summaryContentWidth, 25);
                gfx.DrawString("Total:", fontBold, accentWhiteBrush,
                    new XRect(summaryContentX + 5, currentSummaryDrawingY + 5, summaryContentWidth - 10, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(invoice.TotalAmount.ToString("C", culture), fontBold, accentWhiteBrush,
                    new XRect(summaryContentX + 5, currentSummaryDrawingY + 5, summaryContentWidth - 10, lineHeight), XStringFormats.TopRight);

                // Terms & Conditions
                var termsY = Math.Max(tableY, summaryBoxBottomY) + sectionSpacing;

                gfx.DrawString("Terms & Conditions", fontHeader, primaryVioletBrush,
                    new XRect(margin, termsY, 200, lineHeight), XStringFormats.TopLeft);
                termsY += lineHeight + 5;
                gfx.DrawString("Thank you for your business! Payment is due within 30 days of invoice date.", fontSmall, blackBrush,
                    new XRect(margin, termsY, page.Width - 2 * margin, lineHeight), XStringFormats.TopLeft);

                // wave footer 
                var waveY = page.Height - 60;
                var waveBrush = primaryVioletBrush;

                // Create a simple wave using curves
                var points = new XPoint[20];
                for (int i = 0; i < 20; i++)
                {
                    points[i] = new XPoint(i * (page.Width / 19), waveY + Math.Sin(i * 0.5) * 10);
                }

                // Draw wave base
                gfx.DrawRectangle(waveBrush, 0, waveY + 20, page.Width, 40);

                // Save PDF to MemoryStream
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
    }
}