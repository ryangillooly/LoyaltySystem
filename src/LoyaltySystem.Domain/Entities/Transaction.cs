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

        /// <summary>
        /// The generated human-readable prefixed ID (e.g., tx_xxxx).
        /// Should be generated and assigned just before saving the entity for the first time.
        /// </summary>
        public string PrefixedId { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public TransactionType Type { get; set; }
        public Guid? RewardId { get; set; }
        public int? Quantity { get; set; }
        public decimal? PointsAmount { get; set; }
        public decimal? TransactionAmount { get; set; }
        public Guid StoreId { get; set; }
        public Guid? StaffId { get; set; }
        public string PosTransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual LoyaltyCard Card { get; private set; }
        public virtual Store Store { get; private set; }
        public virtual Reward Reward { get; private set; }
        
        public IReadOnlyDictionary<string, string> Metadata
        {
            get => _metadata;
            set => _metadata = value as Dictionary<string, string>;
        }
        
        public Transaction() =>
            _metadata = new Dictionary<string, string>();

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