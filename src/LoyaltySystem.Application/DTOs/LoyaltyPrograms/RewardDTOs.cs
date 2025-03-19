using System;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class RedeemRewardDto
    {
        /// <summary>
        /// ID of the reward to redeem
        /// </summary>
        public string RewardId { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of the loyalty card to redeem the reward from
        /// </summary>
        public string LoyaltyCardId { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional store ID where the reward is being redeemed
        /// </summary>
        public string? StoreId { get; set; }
        
        /// <summary>
        /// Optional additional notes for the redemption
        /// </summary>
        public string? Notes { get; set; }
    }
    
    public class RewardRedemptionDto
    {
        /// <summary>
        /// Unique identifier for the redemption
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of the redeemed reward
        /// </summary>
        public string RewardId { get; set; } = string.Empty;
        
        /// <summary>
        /// Title of the redeemed reward
        /// </summary>
        public string RewardTitle { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of the loyalty card used for redemption
        /// </summary>
        public string LoyaltyCardId { get; set; } = string.Empty;
        
        /// <summary>
        /// Program ID the reward belongs to
        /// </summary>
        public string ProgramId { get; set; } = string.Empty;
        
        /// <summary>
        /// Program name the reward belongs to
        /// </summary>
        public string ProgramName { get; set; } = string.Empty;
        
        /// <summary>
        /// Date and time when the reward was redeemed
        /// </summary>
        public DateTime RedeemedAt { get; set; }
        
        /// <summary>
        /// Points or stamps used for redemption
        /// </summary>
        public int PointsUsed { get; set; }
        
        /// <summary>
        /// Unique redemption code for verification
        /// </summary>
        public string RedemptionCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Expiration date of the redemption code
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Whether the redemption has been used
        /// </summary>
        public bool IsUsed { get; set; }
        
        /// <summary>
        /// ID of the store where the redemption was created
        /// </summary>
        public string? StoreId { get; set; }
        
        /// <summary>
        /// Name of the store where the redemption was created
        /// </summary>
        public string? StoreName { get; set; }
    }
} 