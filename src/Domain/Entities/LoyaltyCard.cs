using System;
using System.Collections.Generic;
using System.Linq;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a customer's membership in a loyalty program.
    /// This is an Aggregate Root.
    /// </summary>
    public class LoyaltyCard
    {
        private readonly List<Transaction> _transactions;

        public LoyaltyCardId Id { get; private set; }
        public LoyaltyProgramId ProgramId { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public LoyaltyProgramType Type { get; private set; }
        public int StampsCollected { get; private set; }
        public decimal PointsBalance { get; private set; }
        public CardStatus Status { get; private set; }
        public string QrCode { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Navigation properties
        public virtual LoyaltyProgram Program { get; private set; }
        public virtual Customer Customer { get; private set; }
        
        // Collection navigation property
        public virtual IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        // Private constructor for EF Core
        private LoyaltyCard()
        {
            _transactions = new List<Transaction>();
        }

        public LoyaltyCard(
            LoyaltyProgramId programId,
            CustomerId customerId,
            LoyaltyProgramType type,
            DateTime? expiresAt = null)
        {
            Id = EntityId.New<LoyaltyCardId>();
            ProgramId = programId ?? throw new ArgumentNullException(nameof(programId));
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
            Type = type;
            StampsCollected = 0;
            PointsBalance = 0;
            Status = CardStatus.Active;
            QrCode = GenerateQrCode();
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = expiresAt;
            UpdatedAt = DateTime.UtcNow;
            _transactions = new List<Transaction>();
        }

        /// <summary>
        /// Issues stamps to the loyalty card.
        /// </summary>
        public Transaction IssueStamps(
            int quantity,
            StoreId storeId,
            Guid? staffId = null,
            string posTransactionId = null)
        {
            // Validation
            if (Type != LoyaltyProgramType.Stamp)
                throw new InvalidOperationException("Cannot issue stamps to a points-based card");

            if (Status != CardStatus.Active)
                throw new InvalidOperationException("Cannot issue stamps to an inactive card");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            if (storeId == null)
                throw new ArgumentNullException(nameof(storeId));

            // Add the transaction
            var transaction = new Transaction(
                Id,
                TransactionType.StampIssuance,
                quantity: quantity,
                storeId: storeId,
                staffId: staffId,
                posTransactionId: posTransactionId);

            // Update stamps
            StampsCollected += quantity;
            _transactions.Add(transaction);
            UpdatedAt = DateTime.UtcNow;

            return transaction;
        }

        /// <summary>
        /// Adds points to the loyalty card.
        /// </summary>
        public Transaction AddPoints(
            decimal pointsAmount,
            decimal transactionAmount,
            StoreId storeId,
            Guid? staffId = null,
            string posTransactionId = null)
        {
            // Validation
            if (Type != LoyaltyProgramType.Points)
                throw new InvalidOperationException("Cannot add points to a stamp-based card");

            if (Status != CardStatus.Active)
                throw new InvalidOperationException("Cannot add points to an inactive card");

            if (pointsAmount <= 0)
                throw new ArgumentException("Points amount must be greater than zero", nameof(pointsAmount));

            if (transactionAmount < 0)
                throw new ArgumentException("Transaction amount cannot be negative", nameof(transactionAmount));

            if (storeId == null)
                throw new ArgumentNullException(nameof(storeId));

            // Add the transaction
            var transaction = new Transaction(
                Id,
                TransactionType.PointsIssuance,
                pointsAmount: pointsAmount,
                transactionAmount: transactionAmount,
                storeId: storeId,
                staffId: staffId,
                posTransactionId: posTransactionId);

            // Update points
            PointsBalance += pointsAmount;
            _transactions.Add(transaction);
            UpdatedAt = DateTime.UtcNow;

            return transaction;
        }

        /// <summary>
        /// Redeems a reward against this loyalty card.
        /// </summary>
        public Transaction RedeemReward(
            Reward reward,
            StoreId storeId,
            Guid? staffId = null)
        {
            // Validation
            if (Status != CardStatus.Active)
                throw new InvalidOperationException("Cannot redeem rewards with an inactive card");

            if (reward == null)
                throw new ArgumentNullException(nameof(reward));

            if (!reward.ProgramId.Equals(ProgramId))
                throw new InvalidOperationException("Cannot redeem a reward from a different program");

            if (!reward.IsActive)
                throw new InvalidOperationException("Cannot redeem an inactive reward");

            if (!reward.IsValidAt(DateTime.UtcNow))
                throw new InvalidOperationException("Reward is not valid at this time");

            if (storeId == null)
                throw new ArgumentNullException(nameof(storeId));

            // Validate sufficient balance
            if (Type == LoyaltyProgramType.Stamp && StampsCollected < reward.RequiredValue)
                throw new InvalidOperationException("Insufficient stamps for reward redemption");

            if (Type == LoyaltyProgramType.Points && PointsBalance < reward.RequiredValue)
                throw new InvalidOperationException("Insufficient points for reward redemption");

            // Create transaction
            var transaction = new Transaction(
                Id,
                TransactionType.RewardRedemption,
                rewardId: reward.Id,
                storeId: storeId,
                staffId: staffId);

            // Deduct balance
            if (Type == LoyaltyProgramType.Stamp)
                StampsCollected -= reward.RequiredValue;
            else
                PointsBalance -= reward.RequiredValue;

            _transactions.Add(transaction);
            UpdatedAt = DateTime.UtcNow;

            return transaction;
        }

        /// <summary>
        /// Gets the number of stamps issued today.
        /// </summary>
        public int GetStampsIssuedToday()
        {
            if (Type != LoyaltyProgramType.Stamp)
                return 0;

            var today = DateTime.UtcNow.Date;
            
            return _transactions
                .Where(t => t.Type == TransactionType.StampIssuance && 
                       t.Timestamp.Date == today)
                .Sum(t => t.Quantity ?? 0);
        }

        /// <summary>
        /// Expires the loyalty card.
        /// </summary>
        public void Expire()
        {
            if (Status == CardStatus.Expired)
                return;

            Status = CardStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Suspends the loyalty card.
        /// </summary>
        public void Suspend()
        {
            if (Status == CardStatus.Suspended)
                return;

            Status = CardStatus.Suspended;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Reactivates a suspended loyalty card.
        /// </summary>
        public void Reactivate()
        {
            if (Status != CardStatus.Suspended)
                throw new InvalidOperationException("Can only reactivate suspended cards");

            Status = CardStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the expiration date for the loyalty card.
        /// </summary>
        public void SetExpirationDate(DateTime expirationDate)
        {
            if (expirationDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future", nameof(expirationDate));

            ExpiresAt = expirationDate;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds an existing transaction to the card.
        /// Used by the repository for loading transactions.
        /// </summary>
        internal void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
        }

        private string GenerateQrCode()
        {
            // In a real system, this would generate a unique QR code
            // For now, we'll just use a hash of the card ID
            return $"QR_{Id.ToString().Replace("lcy_", "")}";
        }
    }
} 