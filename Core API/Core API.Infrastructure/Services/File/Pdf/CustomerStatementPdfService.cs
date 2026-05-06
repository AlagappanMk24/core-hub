using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Files;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Core_API.Infrastructure.Services.File.Pdf
{
    /// <summary>
    /// Responsible for generating customer statement PDFs using PdfSharp.
    /// Clean, readable, and maintainable PDF generation logic.
    /// </summary>
    public class CustomerStatementPdfService(
        IUnitOfWork unitOfWork,
        ILogger<CustomerStatementPdfService> logger) : ICustomerStatementPdfService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<CustomerStatementPdfService> _logger = logger;
        public async System.Threading.Tasks.Task<MemoryStream> GenerateStatementAsync(Domain.Entities.Customers.Customer customer, OperationContext operationContext)
        {
            try
            {
                var company = await GetCompanyAsync(operationContext);
                var (invoices, payments) = await GetFinancialDataAsync(customer.Id);

                var totals = CalculateFinancialTotals(invoices, payments);
                var agingSummary = CalculateAgingSummary(invoices);

                var document = CreateNewPdfDocument();
                var currentPage = document.AddPage();
                var gfx = XGraphics.FromPdfPage(currentPage);

                double yPosition = 40.0;
                var pageWidth = currentPage.Width.Point;
                const double leftMargin = 40.0;
                const double rightMargin = 40.0;

                // Draw sections sequentially
                yPosition = DrawHeader(gfx, company, pageWidth, leftMargin, rightMargin, yPosition);
                yPosition = DrawCustomerInformation(gfx, customer, leftMargin, yPosition);
                yPosition = DrawFinancialSummary(gfx, totals, invoices.Count, payments.Count,
                                               leftMargin, rightMargin, pageWidth, yPosition);

                yPosition = DrawInvoicesTable(gfx, invoices.Take(20).ToList(), leftMargin, rightMargin,
                                            pageWidth, yPosition, ref currentPage, ref document);

                if (payments.Any())
                {
                    yPosition = DrawPaymentsTable(gfx, payments.Take(20).ToList(), leftMargin, rightMargin,
                                                pageWidth, yPosition, ref currentPage, ref document);
                }

                yPosition = DrawAgingSummary(gfx, agingSummary, leftMargin, rightMargin, pageWidth,
                                           yPosition, ref currentPage, ref document);

                DrawFooter(gfx, leftMargin, rightMargin, pageWidth, currentPage.Height.Point, yPosition);

                return SaveToMemoryStream(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate customer statement PDF for customer {CustomerId}", customer.Id);
                throw;
            }
        }

        private double DrawPaymentsTable(XGraphics gfx, List<InvoicePayment> payments,
    double leftMargin, double rightMargin, double pageWidth, double yPosition,
    ref PdfPage currentPage, ref PdfDocument document)
        {
            if (!payments.Any()) return yPosition;

            var purpleBrush = new XSolidBrush(XColor.FromArgb(138, 43, 226));
            var boldFont = new XFont("Open Sans", 10, XFontStyleEx.Bold);
            var normalFont = new XFont("Open Sans", 10, XFontStyleEx.Regular);
            var smallFont = new XFont("Open Sans", 8, XFontStyleEx.Regular);

            // Add spacing before table
            yPosition += 25;

            var availableWidth = pageWidth - leftMargin - rightMargin;

            // Dynamic column widths based on content
            var colWidths = CalculatePaymentTableColumnWidths(gfx, payments, availableWidth, normalFont);

            var colX = CalculateColumnPositions(leftMargin, colWidths);

            string[] headers = { "Payment #", "Date", "Invoice #", "Method", "Amount" };

            // Draw Title
            gfx.DrawString("RECENT PAYMENTS", boldFont, purpleBrush, leftMargin, yPosition);
            yPosition += 25;

            // Draw Table Header
            yPosition = DrawTableHeader(gfx, colX, colWidths, headers, boldFont, purpleBrush, yPosition, availableWidth);

            var rowY = yPosition;
            var rowHeight = 22.0;
            var rowNum = 0;

            foreach (var payment in payments)
            {
                // Check if we need a new page
                if (rowY > currentPage.Height.Point - 100)
                {
                    currentPage = document.AddPage();
                    currentPage.Width = XUnit.FromMillimeter(210);
                    currentPage.Height = XUnit.FromMillimeter(297);
                    gfx = XGraphics.FromPdfPage(currentPage);
                    rowY = 40;

                    // Redraw header on new page
                    rowY = DrawTableHeader(gfx, colX, colWidths, headers, boldFont, purpleBrush, rowY, availableWidth);
                }

                // Alternate row background
                if (rowNum % 2 == 1)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 250, 252)),
                        leftMargin, rowY, availableWidth, rowHeight);
                }

                DrawTableRowBorders(gfx, colX, colWidths, rowY, rowHeight);

                // Draw cells
                gfx.DrawString($"PY-{payment.Id}", normalFont, XBrushes.Black,
                    colX[0] + 8, rowY + 6, XStringFormats.TopLeft);

                gfx.DrawString(payment.PaymentDate.ToString("dd MMM yyyy"), normalFont, XBrushes.Black,
                    colX[1] + 8, rowY + 6, XStringFormats.TopLeft);

                gfx.DrawString(payment.InvoiceNumber?.ToString() ?? "N/A", normalFont, XBrushes.Black,
                    colX[2] + 8, rowY + 6, XStringFormats.TopLeft);

                gfx.DrawString(payment.PaymentMethod ?? "N/A", normalFont, XBrushes.Black,
                    colX[3] + 8, rowY + 6, XStringFormats.TopLeft);

                // Amount - Right aligned
                var amountText = $"${payment.Amount:N2}";
                gfx.DrawString(amountText, normalFont, XBrushes.Black,
                    colX[4] + colWidths[4] - 8, rowY + 6, XStringFormats.TopRight);

                rowY += rowHeight;
                rowNum++;
            }

            return rowY + 25; // Extra spacing after table
        }

        private double DrawAgingSummary(XGraphics gfx, AgingSummary aging, double leftMargin,
    double rightMargin, double pageWidth, double yPosition,
    ref PdfPage currentPage, ref PdfDocument document)
        {
            var purpleBrush = new XSolidBrush(XColor.FromArgb(138, 43, 226));
            var boldFont = new XFont("Open Sans", 12, XFontStyleEx.Bold);
            var normalFont = new XFont("Open Sans", 10, XFontStyleEx.Regular);

            // Add spacing
            yPosition += 30;

            // Check if new page is needed
            if (yPosition > currentPage.Height.Point - 120)
            {
                currentPage = document.AddPage();
                currentPage.Width = XUnit.FromMillimeter(210);
                currentPage.Height = XUnit.FromMillimeter(297);
                gfx = XGraphics.FromPdfPage(currentPage);
                yPosition = 50;
            }

            gfx.DrawString("AGING SUMMARY", boldFont, purpleBrush, leftMargin, yPosition);
            yPosition += 25;

            var colWidth = (pageWidth - leftMargin - rightMargin) / 5;
            string[] categories = { "Current", "1-30 Days", "31-60 Days", "61-90 Days", "90+ Days" };
            decimal[] values = { aging.Current, aging.Days1To30, aging.Days31To60, aging.Days61To90, aging.Days90Plus };

            // Draw Header Row
            for (int i = 0; i < categories.Length; i++)
            {
                var x = leftMargin + (i * colWidth);
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(138, 43, 226)), x, yPosition, colWidth, 24);

                double centerX = x + (colWidth / 2);
                gfx.DrawString(categories[i], boldFont, XBrushes.White,
                    centerX, yPosition + 7, XStringFormats.TopCenter);
            }

            yPosition += 26;

            // Draw Values Row
            for (int i = 0; i < values.Length; i++)
            {
                var x = leftMargin + (i * colWidth);
                gfx.DrawRectangle(new XPen(XColors.LightGray, 0.6), x, yPosition, colWidth, 28);

                string amountText = $"${values[i]:N2}";
                var textSize = gfx.MeasureString(amountText, normalFont);
                var textX = x + (colWidth / 2) - (textSize.Width / 2);

                gfx.DrawString(amountText, normalFont, XBrushes.Black,
                    textX, yPosition + 9, XStringFormats.TopLeft);
            }

            return yPosition + 50;
        }
        private double[] CalculatePaymentTableColumnWidths(XGraphics gfx, List<InvoicePayment> payments,
            double availableWidth, XFont font)
        {
            double[] widths = new double[5];

            // Base widths
            widths[0] = 80;  // Payment #
            widths[1] = 85;  // Date
            widths[2] = 90;  // Invoice #
            widths[3] = 85;  // Method
            widths[4] = 90;  // Amount

            // Adjust based on actual content if needed
            double total = widths.Sum();
            if (total < availableWidth)
            {
                double extra = (availableWidth - total) / 5;
                for (int i = 0; i < widths.Length; i++)
                    widths[i] += extra;
            }

            return widths;
        }

        #region Data Retrieval & Calculation

        private async Task<Domain.Entities.Companies.Company> GetCompanyAsync(OperationContext context)
        {
            return await _unitOfWork.Companies.GetAsync(c => c.Id == context.CompanyId!.Value)
                ?? new Domain.Entities.Companies.Company { Name = "CoreInvoice" };
        }

        private async Task<(List<Domain.Entities.Invoices.Invoice> Invoices, List<InvoicePayment> Payments)> GetFinancialDataAsync(int customerId)
        {
            var startDate = DateTime.UtcNow.AddMonths(-12);

            var invoicesTask = _unitOfWork.Invoices.Query()
                .Where(i => i.CustomerId == customerId && !i.IsDeleted && i.IssueDate >= startDate)
                .Include(i => i.InvoiceItems)
                .Include(i => i.Payments)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            var paymentsTask = _unitOfWork.InvoicePayments.Query()
                .Where(p => p.CustomerId == customerId && !p.IsDeleted && p.PaymentDate >= startDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            await System.Threading.Tasks.Task.WhenAll(invoicesTask, paymentsTask);

            return (await invoicesTask, await paymentsTask);
        }

        private FinancialTotals CalculateFinancialTotals(List<Domain.Entities.Invoices.Invoice> invoices, List<InvoicePayment> payments)
        {
            var totalInvoiced = invoices.Sum(i => i.TotalAmount);
            var totalPaid = payments.Sum(p => p.Amount);
            var outstanding = totalInvoiced - totalPaid;

            return new FinancialTotals(totalInvoiced, totalPaid, outstanding);
        }

        private AgingSummary CalculateAgingSummary(List<Domain.Entities.Invoices.Invoice> invoices)
        {
            var summary = new AgingSummary();
            var today = DateTime.UtcNow.Date;

            foreach (var invoice in invoices)
            {
                if (invoice.PaymentStatus == PaymentStatus.Paid) continue;

                var outstanding = invoice.TotalAmount - (invoice.AmountPaid);
                if (outstanding <= 0) continue;

                var daysOverdue = (today - invoice.DueDate.Date).Days;

                if (daysOverdue <= 0) summary.Current += outstanding;
                else if (daysOverdue <= 30) summary.Days1To30 += outstanding;
                else if (daysOverdue <= 60) summary.Days31To60 += outstanding;
                else if (daysOverdue <= 90) summary.Days61To90 += outstanding;
                else summary.Days90Plus += outstanding;
            }

            return summary;
        }

        #endregion

        #region PDF Document Setup

        private PdfDocument CreateNewPdfDocument()
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Width = XUnit.FromMillimeter(210);  // A4
            page.Height = XUnit.FromMillimeter(297);
            return document;
        }

        private MemoryStream SaveToMemoryStream(PdfDocument document)
        {
            var stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;
            return stream;
        }

        #endregion

        #region Drawing Sections

        private double DrawHeader(XGraphics gfx, Domain.Entities.Companies.Company company, double pageWidth,
            double leftMargin, double rightMargin, double yPosition)
        {
            var purpleBrush = new XSolidBrush(XColor.FromArgb(138, 43, 226));
            var titleFont = new XFont("Open Sans", 18, XFontStyleEx.Bold);
            var normalFont = new XFont("Open Sans", 10, XFontStyleEx.Regular);
            var smallFont = new XFont("Open Sans", 8, XFontStyleEx.Regular);

            var companyY = yPosition;

            gfx.DrawString(company.Name ?? "CoreInvoice", titleFont, XBrushes.Black, leftMargin, companyY);
            companyY += 25;

            gfx.DrawString(company.Email?.Value ?? "info@coreinvoice.com", normalFont, XBrushes.Gray, leftMargin, companyY);
            companyY += 18;

            gfx.DrawString(company.PhoneNumber?.Value ?? "", normalFont, XBrushes.Gray, leftMargin, companyY);
            companyY += 18;

            if (company.Address != null)
            {
                var address = $"{company.Address.AddressLine1}, {company.Address.City}, {company.Address.State} {company.Address.ZipCode}";
                gfx.DrawString(address.TrimEnd(',', ' '), normalFont, XBrushes.Gray, leftMargin, companyY);
                companyY += 18;
            }

            // Right side title
            gfx.DrawString("CUSTOMER STATEMENT", titleFont, purpleBrush, pageWidth - rightMargin - 200, yPosition);
            gfx.DrawString($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm}", smallFont, XBrushes.Gray,
                pageWidth - rightMargin - 200, yPosition + 25);

            var separatorY = Math.Max(companyY + 20, 130);
            gfx.DrawLine(new XPen(XColors.LightGray, 1), leftMargin, separatorY, pageWidth - rightMargin, separatorY);

            return separatorY + 25;
        }

        private double DrawCustomerInformation(XGraphics gfx, Domain.Entities.Customers.Customer customer, double leftMargin, double yPosition)
        {
            var headerFont = new XFont("Open Sans", 12, XFontStyleEx.Bold);
            var boldFont = new XFont("Open Sans", 10, XFontStyleEx.Bold);
            var normalFont = new XFont("Open Sans", 10, XFontStyleEx.Regular);

            var purpleBrush = new XSolidBrush(XColor.FromArgb(138, 43, 226));

            gfx.DrawString("CUSTOMER INFORMATION", headerFont, purpleBrush, leftMargin, yPosition);
            yPosition += 28;

            var colWidth = 150.0;

            DrawKeyValuePair(gfx, "Customer Name:", customer.Name, leftMargin, colWidth, boldFont, normalFont, ref yPosition);
            DrawKeyValuePair(gfx, "Email:", customer.Email?.Value ?? "N/A", leftMargin, colWidth, boldFont, normalFont, ref yPosition);
            DrawKeyValuePair(gfx, "Phone:", customer.PhoneNumber?.Value ?? "N/A", leftMargin, colWidth, boldFont, normalFont, ref yPosition);

            var address = $"{customer.Address?.AddressLine1 ?? ""}, {customer.Address?.AddressLine2 ?? ""}, " +
                          $"{customer.Address?.City ?? ""}, {customer.Address?.State ?? ""} {customer.Address?.ZipCode ?? ""}".TrimStart(',', ' ');

            DrawKeyValuePair(gfx, "Address:", address, leftMargin, colWidth, boldFont, normalFont, ref yPosition);

            var sinceDate = customer.CreatedDate?.ToString("dd MMM yyyy") ?? "N/A";
            DrawKeyValuePair(gfx, "Customer Since:", sinceDate, leftMargin, colWidth, boldFont, normalFont, ref yPosition);

            return yPosition + 25;
        }

        private void DrawKeyValuePair(XGraphics gfx, string label, string value, double x, double colWidth,
            XFont labelFont, XFont valueFont, ref double y)
        {
            gfx.DrawString(label, labelFont, XBrushes.Black, x, y);
            gfx.DrawString(value, valueFont, XBrushes.Black, x + colWidth, y);
            y += 22;
        }

        private double DrawFinancialSummary(XGraphics gfx, FinancialTotals totals, int invoiceCount, int paymentCount,
            double leftMargin, double rightMargin, double pageWidth, double yPosition)
        {
            var headerFont = new XFont("Open Sans", 12, XFontStyleEx.Bold);
            var purpleBrush = new XSolidBrush(XColor.FromArgb(138, 43, 226));

            gfx.DrawString("FINANCIAL SUMMARY", headerFont, purpleBrush, leftMargin, yPosition);
            yPosition += 28;

            var cardWidth = (pageWidth - leftMargin - rightMargin - 40) / 3;
            var cardHeight = 85.0;
            var cardY = yPosition;

            DrawSummaryCard(gfx, leftMargin, cardY, cardWidth, cardHeight,
                "Total Invoiced", $"${totals.TotalInvoiced:N2}", $"{invoiceCount} invoices");

            DrawSummaryCard(gfx, leftMargin + cardWidth + 20, cardY, cardWidth, cardHeight,
                "Total Paid", $"${totals.TotalPaid:N2}", $"{paymentCount} payments");

            DrawSummaryCard(gfx, leftMargin + 2 * (cardWidth + 20), cardY, cardWidth, cardHeight,
                "Outstanding", $"${totals.OutstandingBalance:N2}",
                totals.OutstandingBalance > 0 ? "Due Now" : "All Paid");

            return yPosition + cardHeight + 40;
        }

        /// <summary>
        /// Calculates X positions for each column based on left margin and column widths
        /// </summary>
        private double[] CalculateColumnPositions(double leftMargin, double[] colWidths)
        {
            var colX = new double[colWidths.Length];
            colX[0] = leftMargin;

            for (int i = 1; i < colWidths.Length; i++)
            {
                colX[i] = colX[i - 1] + colWidths[i - 1];
            }

            return colX;
        }

        /// <summary>
        /// Draws a consistent table header with background and centered text
        /// </summary>
        private double DrawTableHeader(XGraphics gfx, double[] colX, double[] colWidths,
            string[] headers, XFont headerFont, XSolidBrush headerBrush,
            double yPosition, double availableWidth)
        {
            // Draw header background
            gfx.DrawRectangle(headerBrush, colX[0], yPosition, availableWidth, 24);

            // Draw header texts (centered)
            for (int i = 0; i < headers.Length; i++)
            {
                double centerX = colX[i] + (colWidths[i] / 2);
                gfx.DrawString(headers[i], headerFont, XBrushes.White,
                    centerX, yPosition + 7, XStringFormats.TopCenter);
            }

            return yPosition + 26; // Return next Y position after header
        }
        /// <summary>
        /// Internal method to draw the Invoices table with proper pagination support
        /// </summary>
        private double DrawInvoiceTableInternal(XGraphics gfx, List<Domain.Entities.Invoices.Invoice> invoices,
            double leftMargin, double rightMargin, double pageWidth, double yPosition,
            ref PdfPage currentPage, ref PdfDocument document)
        {
            if (!invoices.Any()) return yPosition;

            var purpleBrush = new XSolidBrush(XColor.FromArgb(138, 43, 226));
            var boldFont = new XFont("Open Sans", 10, XFontStyleEx.Bold);
            var normalFont = new XFont("Open Sans", 10, XFontStyleEx.Regular);
            var smallFont = new XFont("Open Sans", 8, XFontStyleEx.Regular);

            var availableWidth = pageWidth - leftMargin - rightMargin;

            // Define column widths (proportional)
            double[] colWidths = new double[]
            {
                availableWidth * 0.20,  // Invoice #
                availableWidth * 0.15,  // Date
                availableWidth * 0.15,  // Due Date
                availableWidth * 0.15,  // Status
                availableWidth * 0.17,  // Amount
                availableWidth * 0.18   // Paid
            };

            var colX = CalculateColumnPositions(leftMargin, colWidths);
            string[] headers = { "Invoice #", "Date", "Due Date", "Status", "Amount", "Paid" };

            // Draw Title
            gfx.DrawString("RECENT INVOICES", boldFont, purpleBrush, leftMargin, yPosition);
            yPosition += 25;

            // Draw Table Header
            yPosition = DrawTableHeader(gfx, colX, colWidths, headers, boldFont, purpleBrush, yPosition, availableWidth);

            var rowY = yPosition;
            var rowHeight = 22.0;
            var rowNum = 0;

            foreach (var invoice in invoices)
            {
                // Create new page if needed
                if (rowY > currentPage.Height.Point - 90)
                {
                    currentPage = document.AddPage();
                    currentPage.Width = XUnit.FromMillimeter(210);
                    currentPage.Height = XUnit.FromMillimeter(297);
                    gfx = XGraphics.FromPdfPage(currentPage);
                    rowY = 40;

                    // Redraw header on new page
                    rowY = DrawTableHeader(gfx, colX, colWidths, headers, boldFont, purpleBrush, rowY, availableWidth);
                }

                // Alternate row background
                if (rowNum % 2 == 1)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 250, 252)),
                        leftMargin, rowY, availableWidth, rowHeight);
                }

                // Draw cell borders
                DrawTableRowBorders(gfx, colX, colWidths, rowY, rowHeight);

                // Draw cells
                gfx.DrawString(invoice.InvoiceNumber, normalFont, XBrushes.Black,
                    colX[0] + 6, rowY + 5, XStringFormats.TopLeft);

                gfx.DrawString(invoice.IssueDate.ToString("dd MMM yyyy"), normalFont, XBrushes.Black,
                    colX[1] + 6, rowY + 5, XStringFormats.TopLeft);

                gfx.DrawString(invoice.DueDate.ToString("dd MMM yyyy"), normalFont, XBrushes.Black,
                    colX[2] + 6, rowY + 5, XStringFormats.TopLeft);

                // Status with color
                var statusColor = GetStatusColorX(invoice.PaymentStatus);
                gfx.DrawString(invoice.PaymentStatus.ToString(), normalFont, new XSolidBrush(statusColor),
                    colX[3] + 6, rowY + 5, XStringFormats.TopLeft);

                // Amount (Right aligned)
                var amountText = $"${invoice.TotalAmount:N2}";
                gfx.DrawString(amountText, normalFont, XBrushes.Black,
                    colX[4] + colWidths[4] - 8, rowY + 5, XStringFormats.TopRight);

                // Paid Amount (Right aligned)
                var paidAmount = invoice.Payments?.Where(p => !p.IsRefund).Sum(p => p.Amount) ?? 0;
                var paidText = $"${paidAmount:N2}";
                gfx.DrawString(paidText, normalFont, XBrushes.Black,
                    colX[5] + colWidths[5] - 8, rowY + 5, XStringFormats.TopRight);

                rowY += rowHeight;
                rowNum++;
            }

            return rowY + 20; // Return position after table
        }
        /// <summary>
        /// Draws light borders around table cells for better readability
        /// </summary>
        private void DrawTableRowBorders(XGraphics gfx, double[] colX, double[] colWidths,
            double rowY, double rowHeight)
        {
            var borderPen = new XPen(XColors.LightGray, 0.5);

            for (int i = 0; i < colX.Length; i++)
            {
                gfx.DrawRectangle(borderPen, colX[i], rowY, colWidths[i], rowHeight);
            }
        }
        #endregion

        #region Table Drawing Methods

        private double DrawInvoicesTable(XGraphics gfx, List<Domain.Entities.Invoices.Invoice> invoices, double leftMargin,
            double rightMargin, double pageWidth, double yPosition, ref PdfPage page, ref PdfDocument document)
        {
            return DrawInvoiceTableInternal(gfx, invoices, leftMargin, rightMargin, pageWidth,
                yPosition, ref page, ref document);
        }

        private void DrawFooter(XGraphics gfx, double leftMargin, double rightMargin,
    double pageWidth, double pageHeight, double yPosition)
        {
            var smallFont = new XFont("Open Sans", 8, XFontStyleEx.Regular);
            var lightGray = XBrushes.LightGray;
            var gray = XBrushes.Gray;

            var footerY = pageHeight - 75; // Position from bottom

            // Draw horizontal separator line
            gfx.DrawLine(new XPen(XColors.LightGray, 0.8),
                leftMargin, footerY - 8,
                pageWidth - rightMargin, footerY - 8);

            // Footer disclaimer text
            var footerText = "This is a system-generated statement. If you have any questions or notice discrepancies, " +
                             "please contact our support team.";

            gfx.DrawString(footerText, smallFont, gray,
                leftMargin, footerY);

            // Generated date
            gfx.DrawString($"Generated on {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC",
                smallFont, lightGray,
                leftMargin, footerY + 15);
        }
        #endregion

        #region Small Reusable Drawing Helpers

        private static void DrawSummaryCard(XGraphics gfx, double x, double y, double width, double height,
            string title, string amount, string subtitle)
        {
            var borderPen = new XPen(XColors.LightGray, 1);
            gfx.DrawRectangle(borderPen, x, y, width, height);

            var labelFont = new XFont("Open Sans", 9, XFontStyleEx.Regular);
            var amountFont = new XFont("Open Sans", 16, XFontStyleEx.Bold);
            var subtitleFont = new XFont("Open Sans", 8, XFontStyleEx.Regular);

            gfx.DrawString(title, labelFont, XBrushes.Gray, x + 12, y + 15);
            gfx.DrawString(amount, amountFont, XBrushes.Black, x + 12, y + 45);
            gfx.DrawString(subtitle, subtitleFont, XBrushes.Gray, x + 12, y + 68);
        }

        private static XColor GetStatusColorX(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Paid => XColor.FromArgb(16, 185, 129),
                PaymentStatus.PartiallyPaid => XColor.FromArgb(245, 158, 11),
                PaymentStatus.Overdue => XColor.FromArgb(239, 68, 68),
                _ => XColor.FromArgb(107, 114, 128)
            };
        }

        #endregion

        #region Private Records

        private record FinancialTotals(decimal TotalInvoiced, decimal TotalPaid, decimal OutstandingBalance);
        private record AgingSummary
        {
            public decimal Current { get; set; }
            public decimal Days1To30 { get; set; }
            public decimal Days31To60 { get; set; }
            public decimal Days61To90 { get; set; }
            public decimal Days90Plus { get; set; }
        }

        #endregion
    }
}
