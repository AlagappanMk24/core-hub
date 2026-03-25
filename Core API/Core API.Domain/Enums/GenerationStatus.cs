namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the outcome of a single recurring invoice generation attempt.
    /// </summary>
    public enum GenerationStatus
    {
        /// <summary>Invoice was generated and saved successfully.</summary>
        Success = 0,

        /// <summary>Generation encountered an error and did not produce an invoice.</summary>
        Failed = 1,

        /// <summary>Generation failed and is queued for an automatic retry.</summary>
        RetryPending = 2,

        /// <summary>Generation was intentionally skipped (e.g., due to a paused schedule or holiday rule).</summary>
        Skipped = 3
    }
}