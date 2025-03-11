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
} 