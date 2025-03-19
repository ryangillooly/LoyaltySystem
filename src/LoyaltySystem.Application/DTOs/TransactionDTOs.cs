using System;
using System.Collections.Generic;

namespace LoyaltySystem.Application.DTOs
{
    public class TransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string CardId { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal PointsEarned { get; set; }
        public int StampsEarned { get; set; }
        public string? RewardId { get; set; }
        public string? RewardTitle { get; set; }
        public string PosTransactionId { get; set; } = string.Empty;
    }
    
    public class RecordTransactionDto
    {
        /// <summary>
        /// ID of the loyalty card to record the transaction for
        /// </summary>
        public string LoyaltyCardId { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of the store where the transaction occurred
        /// </summary>
        public string StoreId { get; set; } = string.Empty;
        
        /// <summary>
        /// Total amount of the transaction
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Point of sale transaction identifier (if available)
        /// </summary>
        public string PosTransactionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Transaction items (optional)
        /// </summary>
        public List<TransactionItemDto> Items { get; set; } = new List<TransactionItemDto>();
        
        /// <summary>
        /// Date and time of the transaction. If not provided, the current time will be used.
        /// </summary>
        public DateTime? TransactionDate { get; set; }
    }
    
    public class TransactionItemDto
    {
        /// <summary>
        /// Product or item identifier
        /// </summary>
        public string ProductId { get; set; } = string.Empty;
        
        /// <summary>
        /// Product or item name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>
        /// Quantity of the item purchased
        /// </summary>
        public int Quantity { get; set; } = 1;
        
        /// <summary>
        /// Price per unit
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Category of the product
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }
} 