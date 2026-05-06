using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Core_API.Infrastructure.Services.Invoicing.InvoiceCalculation
{
    /// <summary>
    /// Implementation of invoice calculation service
    /// </summary>
    public class InvoiceCalculationService(ILogger<InvoiceCalculationService> logger) : IInvoiceCalculationService
    {
        private readonly ILogger<InvoiceCalculationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public void CalculateItemAmounts(List<InvoiceItem> items, List<TaxType> taxTypes)
        {
            foreach (var item in items)
            {
                // Calculate base amount
                item.Amount = item.Quantity * item.UnitPrice;

                // Calculate tax amount
                if (!string.IsNullOrEmpty(item.TaxType))
                {
                    var taxType = taxTypes.FirstOrDefault(t => t.Name == item.TaxType);
                    if (taxType != null)
                    {
                        item.TaxAmount = item.Amount * taxType.Rate / 100;
                        item.TaxPercentage = taxType.Rate;
                    }
                }

                // Calculate total amount
                item.TotalAmount = item.Amount + item.TaxAmount;
                item.IsTaxable = !string.IsNullOrEmpty(item.TaxType);
            }
        }
        public void CalculateTaxDetails(List<InvoiceItem> items, List<InvoiceTaxDetail> taxDetails, List<TaxType> taxTypes)
        {
            foreach (var taxDetail in taxDetails)
            {
                var taxableAmount = items
                    .Where(item => item.TaxType == taxDetail.TaxName)
                    .Sum(item => item.Amount);

                var taxType = taxTypes.FirstOrDefault(t => t.Name == taxDetail.TaxName);
                if (taxType != null)
                {
                    taxDetail.Rate = taxType.Rate;
                    taxDetail.TaxAmount = taxableAmount * taxType.Rate / 100;
                }
            }
        }
        public void CalculateDiscount(List<InvoiceDiscount> discounts, decimal subtotal)
        {
            foreach (var discount in discounts)
            {
                // No calculation needed, just validation
                if (discount.DiscountType == DiscountType.Percentage && discount.Amount > 100)
                {
                    _logger.LogWarning("Percentage discount {Amount}% exceeds 100%", discount.Amount);
                }
            }
        }
        public void CalculateInvoiceTotals(Domain.Entities.Invoices.Invoice invoice)
        {
            // Calculate Subtotal from items
            invoice.Subtotal = invoice.InvoiceItems?.Sum(i => i.Amount) ?? 0;

            // Calculate DiscountTotal 
            invoice.DiscountTotal = invoice.Discounts?.Sum(d =>
            {
                if (d.DiscountType == DiscountType.Percentage)
                {
                    // For percentage discount, Amount is the percentage value
                    return invoice.Subtotal * d.Amount / 100;
                }
                else
                {
                    // For fixed discount, Amount is the fixed amount
                    return d.Amount;
                }
            }) ?? 0;

            // Calculate TaxTotal
            invoice.TaxTotal = invoice.TaxDetails?.Sum(t => t.TaxAmount) ?? 0;

            // Calculate TotalAmount
            invoice.TotalAmount = invoice.Subtotal - invoice.DiscountTotal + invoice.TaxTotal + invoice.ShippingAmount + invoice.AdjustmentAmount;
        }
        public DateTime CalculateDueDateFromPaymentTerms(DateTime issueDate, string paymentTerms)
        {
            if (string.IsNullOrEmpty(paymentTerms))
                return issueDate.AddDays(30);

            // Parse "Net X" format
            var match = Regex.Match(paymentTerms, @"Net\s+(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var days))
                return issueDate.AddDays(days);

            // Handle "Due on Receipt"
            if (paymentTerms.Equals("Due on Receipt", StringComparison.OrdinalIgnoreCase))
                return issueDate;

            // Default to 30 days
            return issueDate.AddDays(30);
        }
        public decimal CalculateAmountDue(decimal totalAmount, decimal amountPaid)
        {
            return totalAmount - amountPaid;
        }
    }
}