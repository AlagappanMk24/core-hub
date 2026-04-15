using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Services.Invoice
{
    /// <summary>
    /// Service for invoice financial calculations
    /// </summary>
    public interface IInvoiceCalculationService
    {
        /// <summary>
        /// Calculates item amounts and tax amounts
        /// </summary>
        void CalculateItemAmounts(List<InvoiceItem> items, List<TaxType> taxTypes);

        /// <summary>
        /// Calculates tax details from items
        /// </summary>
        void CalculateTaxDetails(List<InvoiceItem> items, List<InvoiceTaxDetail> taxDetails, List<TaxType> taxTypes);

        /// <summary>
        /// Validates and processes discounts
        /// </summary>
        void CalculateDiscount(List<InvoiceDiscount> discounts, decimal subtotal);

        /// <summary>
        /// Calculates all invoice totals (subtotal, tax, discount, total)
        /// </summary>
        void CalculateInvoiceTotals(Domain.Entities.Invoice invoice);

        /// <summary>
        /// Calculates due date based on payment terms
        /// </summary>
        DateTime CalculateDueDateFromPaymentTerms(DateTime issueDate, string paymentTerms);

        /// <summary>
        /// Calculates the amount due for an invoice
        /// </summary>
        decimal CalculateAmountDue(decimal totalAmount, decimal amountPaid);
    }
}