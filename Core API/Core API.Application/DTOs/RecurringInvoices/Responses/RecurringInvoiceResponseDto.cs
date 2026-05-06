using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.DTOs.RecurringInvoice.Response
{
    /// <summary>
    /// Detailed response DTO for recurring invoice template
    /// </summary>
    public class RecurringInvoiceResponseDto
    {
        // ── Identity 
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // ── Parties
        public int CustomerId { get; set; }
        public CustomerResponseDto Customer { get; set; }
        public int CompanyId { get; set; }
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }

        // ── Currency
        public string Currency { get; set; }
        public decimal CurrencyRate { get; set; }

        // ── Reference Numbers 
        public string? PONumber { get; set; }

        // ── Financial Summary (from InvoiceHeaderBase)
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // ── Notes & Terms 
        public string CustomerNotes { get; set; }
        public string InternalNotes { get; set; }
        public string TermsAndConditions { get; set; }
        public string FooterNote { get; set; }
        public string ProjectDetail { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentTerms { get; set; }

        // ── Frequency & Interval (Schedule)
        public string Frequency { get; set; } // Stored as string for display
        public int FrequencyInterval { get; set; }
        public int? DayOfMonth { get; set; }
        public string? DayOfWeek { get; set; } // Stored as string for display
        public int? WeekOfMonth { get; set; }
        public int? MonthOfYear { get; set; }

        // Lifecycle Dates
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PausedDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        // Occurrence Tracking
        public int? MaxOccurrences { get; set; }
        public int OccurrencesGenerated { get; set; }
        public DateTime NextInvoiceDate { get; set; }
        public DateTime? LastInvoiceDate { get; set; }

        // Status
        public string Status { get; set; }

        // Generation Settings
        public int GenerateInAdvanceDays { get; set; }

        // ── Automation Flags 
        public bool AutoSend { get; set; }
        public bool AutoEmail { get; set; }
        public bool AutoCharge { get; set; }
        public bool ReminderBeforeDue { get; set; }
        public int ReminderDaysBefore { get; set; }

        // Source Template
        public int? SourceInvoiceId { get; set; }
        public string SourceInvoiceNumber { get; set; }

        // Override Fields
        public string? OverridePONumber { get; set; }
        public string? OverrideCustomerNotes { get; set; }
        public string? OverrideTermsAndConditions { get; set; }
        public string? OverrideFooterNote { get; set; }
        public string? OverrideProjectDetail { get; set; }
        public string? OverridePaymentMethod { get; set; }
        public int? OverridePaymentTerms { get; set; }
        public decimal? OverrideShippingAmount { get; set; }
        public decimal? OverrideAdjustmentAmount { get; set; }
        public string? OverrideAdjustmentDescription { get; set; }

        // Collections
        public List<RecurringInvoiceInstanceDto> GeneratedInvoices { get; set; } = new();

        // Calculated Stats
        /// <summary>Total number of invoices generated so far</summary>
        public int TotalGeneratedCount { get; set; }

        /// <summary>Total monetary value of all generated invoices</summary>
        public decimal TotalGeneratedValue { get; set; }

        /// <summary>Average value per generated invoice</summary>
        public decimal AverageInvoiceValue { get; set; }

        /// <summary>Estimated completion date based on current progress</summary>
        public DateTime? EstimatedCompletionDate
        {
            get
            {
                if (!MaxOccurrences.HasValue || OccurrencesGenerated >= MaxOccurrences.Value)
                    return null;

                var remaining = MaxOccurrences.Value - OccurrencesGenerated;
                var daysPerOccurrence = GetDaysPerOccurrence();
                return NextInvoiceDate.AddDays(remaining * daysPerOccurrence);
            }
        }
        private int GetDaysPerOccurrence()
        {
            return Frequency?.ToLower() switch
            {
                "daily" => FrequencyInterval,
                "weekly" => FrequencyInterval * 7,
                "biweekly" => FrequencyInterval * 14,
                "monthly" => FrequencyInterval * 30,
                "bimonthly" => FrequencyInterval * 60,
                "quarterly" => FrequencyInterval * 90,
                "semiannually" => FrequencyInterval * 180,
                "annually" => FrequencyInterval * 365,
                _ => 30
            };
        }

        // Audit
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
