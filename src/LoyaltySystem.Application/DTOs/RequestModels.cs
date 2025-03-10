using System;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Request model for issuing stamps to a loyalty card.
    /// </summary>
    public class IssueStampsRequest
    {
        /// <summary>
        /// The ID of the loyalty card.
        /// </summary>
        public LoyaltyCardId CardId { get; set; }
        
        /// <summary>
        /// The number of stamps to issue.
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// The store where the stamps are being issued.
        /// </summary>
        public StoreId StoreId { get; set; }
        
        /// <summary>
        /// The staff member issuing the stamps (optional).
        /// </summary>
        public Guid? StaffId { get; set; }
        
        /// <summary>
        /// The POS transaction ID (optional).
        /// </summary>
        public string PosTransactionId { get; set; }
    }
    
    /// <summary>
    /// Request model for adding points to a loyalty card.
    /// </summary>
    public class AddPointsRequest
    {
        /// <summary>
        /// The ID of the loyalty card.
        /// </summary>
        public LoyaltyCardId CardId { get; set; }
        
        /// <summary>
        /// The number of points to add.
        /// </summary>
        public decimal Points { get; set; }
        
        /// <summary>
        /// The transaction amount that generated these points.
        /// </summary>
        public decimal TransactionAmount { get; set; }
        
        /// <summary>
        /// The store where the points are being added.
        /// </summary>
        public StoreId StoreId { get; set; }
        
        /// <summary>
        /// The staff member adding the points (optional).
        /// </summary>
        public Guid? StaffId { get; set; }
        
        /// <summary>
        /// The POS transaction ID (optional).
        /// </summary>
        public string PosTransactionId { get; set; }
    }
    
    /// <summary>
    /// Request model for redeeming a reward.
    /// </summary>
    public class RedeemRewardRequest
    {
        /// <summary>
        /// The ID of the loyalty card.
        /// </summary>
        public LoyaltyCardId CardId { get; set; }
        
        /// <summary>
        /// The ID of the reward to redeem.
        /// </summary>
        public RewardId RewardId { get; set; }
        
        /// <summary>
        /// The store where the reward is being redeemed.
        /// </summary>
        public StoreId StoreId { get; set; }
        
        /// <summary>
        /// The staff member processing the redemption (optional).
        /// </summary>
        public Guid? StaffId { get; set; }
        
        /// <summary>
        /// Additional redemption data (optional).
        /// </summary>
        public RedeemRequestData RedemptionData { get; set; }
    }
    
    /// <summary>
    /// Additional data for reward redemption.
    /// </summary>
    public class RedeemRequestData
    {
        /// <summary>
        /// Optional customer notes or special instructions.
        /// </summary>
        public string CustomerNotes { get; set; }
        
        /// <summary>
        /// Optional flag to indicate if this is a digital redemption.
        /// </summary>
        public bool IsDigitalRedemption { get; set; }
    }
} 