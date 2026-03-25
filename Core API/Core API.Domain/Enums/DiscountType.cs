namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the method by which a discount is calculated and applied.
    /// </summary>
    public enum DiscountType
    {
        /// <summary>Discount is a percentage of the line or invoice total.</summary>
        Percentage = 0,

        /// <summary>Discount is a fixed monetary amount deducted from the total.</summary>
        Fixed = 1,

        /// <summary>Discount offered as an incentive for settling the invoice before the due date.</summary>
        EarlyPayment = 2,

        /// <summary>Discount applied when order quantity meets a defined volume threshold.</summary>
        Volume = 3,

        /// <summary>Time-limited promotional discount applied via a coupon code or campaign.</summary>
        Promotional = 4
    }
}