using System;

namespace LoyaltySystem.Application.Events
{
    /// <summary>
    /// Event raised when points are added to a loyalty card.
    /// </summary>
    public class PointsAddedEvent : EventBase
    {
        /// <summary>
        /// The ID of the loyalty card that received points.
        /// </summary>
        public Guid CardId { get; private set; }
        
        /// <summary>
        /// The ID of the customer who owns the card.
        /// </summary>
        public Guid CustomerId { get; private set; }
        
        /// <summary>
        /// The number of points added.
        /// </summary>
        public decimal PointsAdded { get; private set; }
        
        /// <summary>
        /// The total points balance after the addition.
        /// </summary>
        public decimal PointsBalance { get; private set; }
        
        /// <summary>
        /// The monetary amount of the transaction that earned points.
        /// </summary>
        public decimal TransactionAmount { get; private set; }
        
        /// <summary>
        /// The store where the points were added.
        /// </summary>
        public Guid StoreId { get; private set; }
        
        /// <summary>
        /// Creates a new points added event.
        /// </summary>
        public PointsAddedEvent(
            Guid cardId, 
            Guid customerId, 
            decimal pointsAdded, 
            decimal pointsBalance, 
            decimal transactionAmount, 
            Guid storeId)
        {
            CardId = cardId;
            CustomerId = customerId;
            PointsAdded = pointsAdded;
            PointsBalance = pointsBalance;
            TransactionAmount = transactionAmount;
            StoreId = storeId;
        }
    }
} 