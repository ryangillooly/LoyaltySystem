using LoyaltySystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a loyalty program offered by a brand.
    /// This is an Aggregate Root.
    /// </summary>
    public class LoyaltyProgram
    {
        private readonly List<Reward> _rewards;

        public LoyaltyProgramId Id { get; private set; }
        public Guid BrandId { get; private set; }
        public string Name { get; private set; }
        public LoyaltyProgramType Type { get; private set; }
        
        // For stamp-based programs
        public int? StampThreshold { get; private set; }
        
        // For points-based programs
        public decimal? PointsConversionRate { get; private set; }
        
        // Optional constraints
        public int? DailyStampLimit { get; private set; }
        public decimal? MinimumTransactionAmount { get; private set; }
        
        // Expiration settings
        public ExpirationPolicy ExpirationPolicy { get; private set; }
        
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Navigation property
        public virtual Brand Brand { get; private set; }
        
        // Collection navigation property
        public virtual IReadOnlyCollection<Reward> Rewards => _rewards.AsReadOnly();

        // Private constructor for EF Core
        private LoyaltyProgram()
        {
            _rewards = new List<Reward>();
        }

        public LoyaltyProgram(
            Guid brandId,
            string name,
            LoyaltyProgramType type,
            int? stampThreshold = null,
            decimal? pointsConversionRate = null,
            int? dailyStampLimit = null,
            decimal? minimumTransactionAmount = null,
            ExpirationPolicy expirationPolicy = null)
        {
            if (brandId == Guid.Empty)
                throw new ArgumentException("BrandId cannot be empty", nameof(brandId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Program name cannot be empty", nameof(name));

            // Type-specific validation
            ValidateProgramTypeParameters(type, stampThreshold, pointsConversionRate);

            Id = new LoyaltyProgramId();
            BrandId = brandId;
            Name = name;
            Type = type;
            StampThreshold = type == LoyaltyProgramType.Stamp ? stampThreshold : null;
            PointsConversionRate = type == LoyaltyProgramType.Points ? pointsConversionRate : null;
            DailyStampLimit = dailyStampLimit;
            MinimumTransactionAmount = minimumTransactionAmount;
            ExpirationPolicy = expirationPolicy ?? new ExpirationPolicy();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _rewards = new List<Reward>();
        }

        /// <summary>
        /// Creates a new reward for this loyalty program.
        /// </summary>
        public Reward CreateReward(
            string title,
            string description,
            int requiredValue,
            DateTime? validFrom = null,
            DateTime? validTo = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Reward title cannot be empty", nameof(title));

            if (requiredValue <= 0)
                throw new ArgumentException("Required value must be greater than zero", nameof(requiredValue));

            var reward = new Reward(
                Id,
                title,
                description,
                requiredValue,
                validFrom,
                validTo);

            _rewards.Add(reward);
            return reward;
        }

        /// <summary>
        /// Updates the properties of the loyalty program.
        /// </summary>
        public void Update(
            string name,
            int? stampThreshold = null,
            decimal? pointsConversionRate = null,
            int? dailyStampLimit = null,
            decimal? minimumTransactionAmount = null,
            ExpirationPolicy expirationPolicy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Program name cannot be empty", nameof(name));

            // Type-specific validation
            ValidateProgramTypeParameters(Type, stampThreshold, pointsConversionRate);

            Name = name;
            
            if (Type == LoyaltyProgramType.Stamp)
                StampThreshold = stampThreshold;
                
            if (Type == LoyaltyProgramType.Points)
                PointsConversionRate = pointsConversionRate;
                
            DailyStampLimit = dailyStampLimit;
            MinimumTransactionAmount = minimumTransactionAmount;
            
            if (expirationPolicy != null)
                ExpirationPolicy = expirationPolicy;
                
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Activates the loyalty program.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deactivates the loyalty program.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the program is valid for stamp issuance.
        /// </summary>
        public bool IsValidForStampIssuance()
        {
            return IsActive && Type == LoyaltyProgramType.Stamp;
        }

        /// <summary>
        /// Checks if the program is valid for points issuance.
        /// </summary>
        public bool IsValidForPointsIssuance(decimal transactionAmount)
        {
            if (!IsActive || Type != LoyaltyProgramType.Points)
                return false;

            // If minimum transaction amount is set, validate it
            if (MinimumTransactionAmount.HasValue)
                return transactionAmount >= MinimumTransactionAmount.Value;

            return true;
        }

        /// <summary>
        /// Calculates points based on transaction amount and conversion rate.
        /// </summary>
        public decimal CalculatePoints(decimal transactionAmount)
        {
            if (Type != LoyaltyProgramType.Points || !PointsConversionRate.HasValue)
                throw new InvalidOperationException("Cannot calculate points for non-points program or missing conversion rate");

            if (MinimumTransactionAmount.HasValue && transactionAmount < MinimumTransactionAmount.Value)
                return 0;

            return Math.Floor(transactionAmount * PointsConversionRate.Value);
        }
        
        /// <summary>
        /// Sets the expiration policy.
        /// Used by the repository for loading from database.
        /// </summary>
        internal void SetExpirationPolicy(ExpirationPolicy expirationPolicy)
        {
            ExpirationPolicy = expirationPolicy;
        }
        
        /// <summary>
        /// Adds an existing reward to the program.
        /// Used by the repository for loading from database.
        /// </summary>
        public void AddReward(Reward reward)
        {
            _rewards.Add(reward);
        }
        
        private void ValidateProgramTypeParameters(
            LoyaltyProgramType type, 
            int? stampThreshold, 
            decimal? pointsConversionRate)
        {
            if (type == LoyaltyProgramType.Stamp)
            {
                if (!stampThreshold.HasValue)
                    throw new ArgumentException("Stamp threshold is required for stamp-based programs", nameof(stampThreshold));
                    
                if (stampThreshold.Value <= 0)
                    throw new ArgumentException("Stamp threshold must be greater than zero", nameof(stampThreshold));
            }
            else if (type == LoyaltyProgramType.Points)
            {
                if (!pointsConversionRate.HasValue)
                    throw new ArgumentException("Points conversion rate is required for points-based programs", nameof(pointsConversionRate));
                    
                if (pointsConversionRate.Value <= 0)
                    throw new ArgumentException("Points conversion rate must be greater than zero", nameof(pointsConversionRate));
            }
        }
    }
} 