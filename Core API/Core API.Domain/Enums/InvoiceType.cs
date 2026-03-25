namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Categorises the type of invoice being issued.
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>Regular one-time invoice for goods or services.</summary>
        Standard = 0,

        /// <summary>An instance generated from a recurring invoice template.</summary>
        Recurring = 1,

        /// <summary>Preliminary invoice sent before final billing to confirm agreement.</summary>
        Proforma = 2,

        /// <summary>Issued to reduce the amount owed by a customer (negative invoice).</summary>
        CreditNote = 3,

        /// <summary>Issued to request additional payment beyond the original invoice.</summary>
        DebitNote = 4,

        /// <summary>Used for international trade to declare goods being exported.</summary>
        Commercial = 5,

        /// <summary>Generated from logged billable hours on a timesheet.</summary>
        Timesheet = 6,

        /// <summary>Generated from submitted expense claims.</summary>
        Expense = 7
    }
}