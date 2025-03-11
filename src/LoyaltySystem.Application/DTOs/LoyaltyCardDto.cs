using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// DTO for a loyalty card.
    /// </summary>
    public class LoyaltyCardDto
    {
        /// <summary>
        /// The unique identifier for the card.
        /// </summary>
        public LoyaltyCardId? Id { get; set; }
        
        /// <summary>
        /// The customer who owns this card.
        /// </summary>
        public CustomerId? CustomerId { get; set; }
        
        /// <summary>
        /// The loyalty program this card belongs to.
        /// </summary>
        public LoyaltyProgramId? ProgramId { get; set; }
        
        /// <summary>
        /// The name of the loyalty program.
        /// </summary>
        public string? ProgramName { get; set; }
        
        /// <summary>
        /// The name of the brand associated with this program.
        /// </summary>
        public string? BrandName { get; set; }
        
        /// <summary>
        /// The current status of the card.
        /// </summary>
        public CardStatus Status { get; set; }
        
        /// <summary>
        /// When the card was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the card expires.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// The current stamp count.
        /// </summary>
        public int StampCount { get; set; }
        
        /// <summary>
        /// The current points balance.
        /// </summary>
        public decimal PointsBalance { get; set; }
        
        /// <summary>
        /// The total number of transactions for this card.
        /// </summary>
        public int TotalTransactions { get; set; }
        
        /// <summary>
        /// The date of the last activity on this card.
        /// </summary>
        public DateTime? LastActivityDate { get; set; }
        
        /// <summary>
        /// The transactions for this card.
        /// </summary>
        public List<TransactionDto> Transactions { get; set; } = new();
    }
} 