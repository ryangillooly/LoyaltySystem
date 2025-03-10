using System;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Request model for redeeming a reward with a loyalty card.
    /// </summary>
    public class RedeemRewardRequest
    {
        /// <summary>
        /// The identifier of the loyalty card.
        /// Can be null if using QrCode instead.
        /// </summary>
        public Guid? CardId { get; set; }
        
        /// <summary>
        /// The QR code of the loyalty card.
        /// Can be null if using CardId instead.
        /// </summary>
        public string QrCode { get; set; }
        
        /// <summary>
        /// The identifier of the reward to redeem.
        /// </summary>
        public Guid RewardId { get; set; }
        
        /// <summary>
        /// The store where the reward is being redeemed.
        /// </summary>
        public Guid StoreId { get; set; }
        
        /// <summary>
        /// The staff member processing the redemption.
        /// </summary>
        public Guid? StaffId { get; set; }
    }
} 