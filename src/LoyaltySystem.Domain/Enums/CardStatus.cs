namespace LoyaltySystem.Domain.Enums
{
    /// <summary>
    /// Defines the status of a loyalty card.
    /// </summary>
    public enum CardStatus
    {
        /// <summary>
        /// Card is active and can be used for transactions.
        /// </summary>
        Active,
        
        /// <summary>
        /// Card has expired and cannot be used.
        /// </summary>
        Expired,
        
        /// <summary>
        /// Card has been suspended by staff and cannot be used.
        /// </summary>
        Suspended
    }
} 