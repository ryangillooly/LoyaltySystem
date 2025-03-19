using System;

namespace LoyaltySystem.Application.Events
{
    /// <summary>
    /// Event raised when a reward is redeemed with a loyalty card.
    /// </summary>
    public class RewardRedeemedEvent : EventBase
    {
        /// <summary>
        /// The ID of the loyalty card used for redemption.
        /// </summary>
        public Guid CardId { get; private set; }
        
        /// <summary>
        /// The ID of the customer who owns the card.
        /// </summary>
        public Guid CustomerId { get; private set; }
        
        /// <summary>
        /// The ID of the reward that was redeemed.
        /// </summary>
        public Guid RewardId { get; private set; }
        
        /// <summary>
        /// The title of the reward.
        /// </summary>
        public string RewardTitle { get; private set; }
        
        /// <summary>
        /// The value of the reward (in stamps or points).
        /// </summary>
        public int RequiredValue { get; private set; }
        
        /// <summary>
        /// The store where the reward was redeemed.
        /// </summary>
        public Guid StoreId { get; private set; }
        
        /// <summary>
        /// Creates a new reward redeemed event.
        /// </summary>
        public RewardRedeemedEvent(
            Guid cardId, 
            Guid customerId, 
            Guid rewardId, 
            string rewardTitle, 
            int requiredValue, 
            Guid storeId)
        {
            CardId = cardId;
            CustomerId = customerId;
            RewardId = rewardId;
            RewardTitle = rewardTitle;
            RequiredValue = requiredValue;
            StoreId = storeId;
        }
    }
} 