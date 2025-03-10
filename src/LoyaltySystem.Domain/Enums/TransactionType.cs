namespace LoyaltySystem.Domain.Enums
{
    /// <summary>
    /// Defines the type of loyalty transaction.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Issuing stamps to a loyalty card.
        /// </summary>
        StampIssuance = 1,
        
        /// <summary>
        /// Adding points to a loyalty card.
        /// </summary>
        PointsIssuance = 2,
        
        /// <summary>
        /// Redeeming a reward.
        /// </summary>
        RewardRedemption = 3,
        
        /// <summary>
        /// Voiding previously issued stamps.
        /// </summary>
        StampVoid = 4,
        
        /// <summary>
        /// Voiding previously issued points.
        /// </summary>
        PointsVoid = 5
    }
} 