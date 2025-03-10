using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a loyalty transaction (e.g., stamp issuance, points addition, reward redemption).
    /// </summary>
    public class Transaction
    {
        // This field is exposed for Entity Framework Core
        internal Dictionary<string, string> _metadata;

        public Guid Id { get; private set; }
        public Guid CardId { get; private set; }
        public TransactionType Type { get; private set; }
        public Guid? RewardId { get; private set; }
        public int? Quantity { get; private set; }
        public decimal? PointsAmount { get; private set; }
        public decimal? TransactionAmount { get; private set; }
        public Guid StoreId { get; private set; }
        public Guid? StaffId { get; private set; }
        public string PosTransactionId { get; private set; }
        public DateTime Timestamp { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Navigation properties
        public virtual LoyaltyCard Card { get; private set; }
        public virtual Store Store { get; private set; }
        public virtual Reward Reward { get; private set; }
        
        // Metadata access
        public IReadOnlyDictionary<string, string> Metadata => _metadata;

        // Private constructor for EF Core
        private Transaction()
        {
            _metadata = new Dictionary<string, string>();
        }

        public Transaction(
            Guid cardId,
            TransactionType type,
            Guid? rewardId = null,
            int? quantity = null,
            decimal? pointsAmount = null,
            decimal? transactionAmount = null,
            Guid storeId = default,
            Guid? staffId = null,
            string posTransactionId = null,
            Dictionary<string, string> metadata = null)
        {
            if (cardId == Guid.Empty)
                throw new ArgumentException("Card ID cannot be empty", nameof(cardId));

            if (storeId == Guid.Empty)
                throw new ArgumentException("Store ID cannot be empty", nameof(storeId));

            // Type-specific validation
            ValidateTransactionType(type, rewardId, quantity, pointsAmount);

            Id = Guid.NewGuid();
            CardId = cardId;
            Type = type;
            RewardId = rewardId;
            Quantity = quantity;
            PointsAmount = pointsAmount;
            TransactionAmount = transactionAmount;
            StoreId = storeId;
            StaffId = staffId;
            PosTransactionId = posTransactionId;
            Timestamp = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            _metadata = metadata ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Adds or updates metadata for this transaction.
        /// </summary>
        public void AddMetadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Metadata key cannot be empty", nameof(key));

            _metadata[key] = value;
        }

        private void ValidateTransactionType(
            TransactionType type,
            Guid? rewardId,
            int? quantity,
            decimal? pointsAmount)
        {
            switch (type)
            {
                case TransactionType.StampIssuance:
                case TransactionType.StampVoid:
                    if (!quantity.HasValue || quantity.Value <= 0)
                        throw new ArgumentException("Stamp transactions require a positive quantity", nameof(quantity));
                    break;
                    
                case TransactionType.PointsIssuance:
                case TransactionType.PointsVoid:
                    if (!pointsAmount.HasValue || pointsAmount.Value <= 0)
                        throw new ArgumentException("Points transactions require a positive points amount", nameof(pointsAmount));
                    break;
                    
                case TransactionType.RewardRedemption:
                    if (!rewardId.HasValue || rewardId.Value == Guid.Empty)
                        throw new ArgumentException("Reward redemption requires a valid reward ID", nameof(rewardId));
                    break;
                    
                default:
                    throw new ArgumentException("Invalid transaction type", nameof(type));
            }
        }
    }
} 