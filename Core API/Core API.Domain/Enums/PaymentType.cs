namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Payment type enumeration
    /// </summary>
    public enum PaymentType
    {
        Full,         // Full payment of invoice
        Partial,      // Partial payment
        Advance,      // Advance payment (prepayment)
        Deposit,      // Deposit payment
        Final         // Final payment (for multi-payment arrangements)
    }
}