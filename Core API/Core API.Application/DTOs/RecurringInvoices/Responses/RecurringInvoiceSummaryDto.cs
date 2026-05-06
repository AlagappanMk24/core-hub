namespace Core_API.Application.DTOs.RecurringInvoice.Response
{
    /// <summary>
    /// Summary DTO for list views of recurring invoices
    /// </summary>
    public class RecurringInvoiceSummaryDto
    {
        /// <summary>ID of the recurring invoice template</summary>
        public int Id { get; set; }

        /// <summary>Display name of the recurring template</summary>
        public string Name { get; set; }

        /// <summary>Name of the associated customer</summary>
        public string? CustomerName { get; set; }

        /// <summary>Recurrence frequency as display string</summary>
        public string Frequency { get; set; }

        /// <summary>Total amount per invoice</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Next scheduled invoice date</summary>
        public DateTime NextInvoiceDate { get; set; }

        /// <summary>Current status of the recurring template</summary>
        public string Status { get; set; }

        /// <summary>Number of invoices generated so far</summary>
        public int OccurrencesGenerated { get; set; }

        /// <summary>Maximum number of invoices (null = unlimited)</summary>
        public int? MaxOccurrences { get; set; }

        /// <summary>Total value of all generated invoices</summary>
        public decimal TotalGeneratedValue { get; set; }

        /// <summary>Whether auto-send is enabled</summary>
        public bool AutoSend { get; set; }

        /// <summary>Currency code</summary>
        public string Currency { get; set; }

        /// <summary>Progress percentage (0-100)</summary>
        public int ProgressPercentage
        {
            get
            {
                if (!MaxOccurrences.HasValue || MaxOccurrences.Value == 0)
                    return 0;
                return (int)(OccurrencesGenerated / (decimal)MaxOccurrences.Value * 100);
            }
        }

        /// <summary>Days until next invoice</summary>
        public int DaysUntilNext
        {
            get
            {
                return (NextInvoiceDate - DateTime.UtcNow.Date).Days;
            }
        }

        /// <summary>Whether the recurring invoice is active</summary>
        public bool IsActive => Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) == true;
    }
}
