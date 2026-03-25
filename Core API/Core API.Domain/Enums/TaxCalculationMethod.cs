namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Defines how tax is applied relative to the item or invoice price.
    /// </summary>
    public enum TaxCalculationMethod
    {
        /// <summary>Tax is calculated on top of the net price and added to the total (most common).</summary>
        Exclusive = 0,

        /// <summary>Tax is already embedded within the stated price; no tax is added on checkout.</summary>
        Inclusive = 1,

        /// <summary>Tax is calculated on the cumulative amount including previously applied taxes.</summary>
        Compound = 2
    }
}
