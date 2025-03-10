using System;

namespace LoyaltySystem.Application.Events
{
    /// <summary>
    /// Event raised when stamps are issued to a loyalty card.
    /// </summary>
    public class StampsIssuedEvent : EventBase
    {
        /// <summary>
        /// The ID of the loyalty card that received stamps.
        /// </summary>
        public Guid CardId { get; private set; }
        
        /// <summary>
        /// The ID of the customer who owns the card.
        /// </summary>
        public Guid CustomerId { get; private set; }
        
        /// <summary>
        /// The number of stamps issued.
        /// </summary>
        public int StampsIssued { get; private set; }
        
        /// <summary>
        /// The total number of stamps on the card after issuance.
        /// </summary>
        public int TotalStamps { get; private set; }
        
        /// <summary>
        /// The store where the stamps were issued.
        /// </summary>
        public Guid StoreId { get; private set; }
        
        /// <summary>
        /// Creates a new stamps issued event.
        /// </summary>
        public StampsIssuedEvent(Guid cardId, Guid customerId, int stampsIssued, int totalStamps, Guid storeId)
        {
            CardId = cardId;
            CustomerId = customerId;
            StampsIssued = stampsIssued;
            TotalStamps = totalStamps;
            StoreId = storeId;
        }
    }
} 