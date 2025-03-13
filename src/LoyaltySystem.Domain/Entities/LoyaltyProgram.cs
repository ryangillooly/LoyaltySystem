using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a loyalty program offered by a brand.
    /// This is an Aggregate Root.
    /// </summary>
    public sealed class LoyaltyProgram
    {
        private readonly List<Reward> _rewards = new ();
        private readonly List<LoyaltyTier> _tiers = new ();
        
        public LoyaltyProgramId Id { get; set; }
        public BrandId BrandId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LoyaltyProgramType Type { get; set; }
        public ExpirationPolicy ExpirationPolicy { get; set; }
        
        // For stamp-based programs
        public int? StampThreshold { get; set; }
        
        // For points-based programs
        public decimal? PointsConversionRate { get; set; }
        
        // New points configuration for more detailed control
        public PointsConfig PointsConfig { get; set; }
        
        // Optional constraints
        public int? DailyStampLimit { get; set; }
        public decimal? MinimumTransactionAmount { get; set; }
        
        public bool HasTiers { get; set; }
        public string TermsAndConditions { get; set; }
        public int EnrollmentBonusPoints { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Brand Brand { get; set; }
        
        // Collection navigation properties
        public IReadOnlyCollection<Reward> Rewards => _rewards.AsReadOnly();
        public IReadOnlyCollection<LoyaltyTier> Tiers => _tiers.AsReadOnly();
        
        public LoyaltyProgram() { }

        public LoyaltyProgram(
            BrandId brandId,
            string name,
            LoyaltyProgramType type,
            int? stampThreshold = null,
            decimal? pointsConversionRate = null,
            PointsConfig pointsConfig = null,
            bool hasTiers = false,
            int? dailyStampLimit = null,
            decimal? minimumTransactionAmount = null,
            ExpirationPolicy expirationPolicy = null,
            string description = null,
            string termsAndConditions = null,
            int enrollmentBonusPoints = 0,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (brandId == Guid.Empty)
                throw new ArgumentException("BrandId cannot be empty", nameof(brandId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Program name cannot be empty", nameof(name));

            ValidateProgramTypeParameters(type, stampThreshold, pointsConversionRate, pointsConfig);

            Id = new LoyaltyProgramId();
            BrandId = brandId;
            Name = name;
            Description = description;
            Type = type;
            StampThreshold = type == LoyaltyProgramType.Stamp ? stampThreshold : null;
            PointsConversionRate = type == LoyaltyProgramType.Points ? pointsConversionRate : null;
            PointsConfig = type == LoyaltyProgramType.Points ? (pointsConfig ?? new PointsConfig()) : null;
            HasTiers = hasTiers;
            DailyStampLimit = dailyStampLimit;
            MinimumTransactionAmount = minimumTransactionAmount;
            ExpirationPolicy = expirationPolicy;
            TermsAndConditions = termsAndConditions;
            EnrollmentBonusPoints = enrollmentBonusPoints;
            StartDate = startDate ?? DateTime.UtcNow;
            EndDate = endDate;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
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
        /// Creates a new tier for this loyalty program.
        /// </summary>
        public LoyaltyTier CreateTier(
            string name,
            int pointThreshold,
            decimal pointMultiplier,
            int tierOrder,
            IEnumerable<string> benefits = null)
        {
            if (!HasTiers)
                throw new InvalidOperationException("Cannot create tiers for a non-tiered program");

            if (Type != LoyaltyProgramType.Points)
                throw new InvalidOperationException("Tiers are only supported for points-based programs");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tier name cannot be empty", nameof(name));

            if (pointThreshold < 0)
                throw new ArgumentException("Point threshold cannot be negative", nameof(pointThreshold));

            if (pointMultiplier <= 0)
                throw new ArgumentException("Point multiplier must be greater than zero", nameof(pointMultiplier));

            // Check if tier with same order already exists
            if (_tiers.Any(t => t.TierOrder == tierOrder))
                throw new InvalidOperationException($"A tier with order {tierOrder} already exists");

            var tier = new LoyaltyTier(
                Id,
                name,
                pointThreshold,
                pointMultiplier,
                tierOrder);

            // Add benefits if provided
            if (benefits != null)
            {
                foreach (var benefit in benefits)
                {
                    tier.AddBenefit(benefit);
                }
            }

            _tiers.Add(tier);
            return tier;
        }

        /// <summary>
        /// Updates the properties of the loyalty program.
        /// </summary>
        public void Update(
            string name,
            int? stampThreshold = null,
            decimal? pointsConversionRate = null,
            PointsConfig pointsConfig = null,
            bool? hasTiers = null,
            int? dailyStampLimit = null,
            decimal? minimumTransactionAmount = null,
            ExpirationPolicy expirationPolicy = null,
            string description = null,
            string termsAndConditions = null,
            int? enrollmentBonusPoints = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Program name cannot be empty", nameof(name));

            ValidateProgramTypeParameters(Type, stampThreshold, pointsConversionRate, pointsConfig);

            Name = name;
            
            if (!string.IsNullOrEmpty(description))
                Description = description;
                
            if (!string.IsNullOrEmpty(termsAndConditions))
                TermsAndConditions = termsAndConditions;
                
            if (startDate.HasValue)
                StartDate = startDate.Value;
                
            if (endDate.HasValue)
                EndDate = endDate;
            
            if (Type == LoyaltyProgramType.Stamp && stampThreshold.HasValue)
                StampThreshold = stampThreshold;
                
            if (Type == LoyaltyProgramType.Points)
            {
                if (pointsConversionRate.HasValue)
                    PointsConversionRate = pointsConversionRate;
                    
                if (pointsConfig != null)
                    PointsConfig = pointsConfig;
            }
            
            if (hasTiers.HasValue)
            {
                if (hasTiers.Value && Type != LoyaltyProgramType.Points)
                    throw new InvalidOperationException("Tiers are only supported for points-based programs");
                    
                HasTiers = hasTiers.Value;
            }
                
            if (dailyStampLimit.HasValue)
                DailyStampLimit = dailyStampLimit;
                
            if (minimumTransactionAmount.HasValue)
                MinimumTransactionAmount = minimumTransactionAmount;
                
            if (enrollmentBonusPoints.HasValue)
                EnrollmentBonusPoints = enrollmentBonusPoints.Value;
            
            if (expirationPolicy != null)
                ExpirationPolicy = expirationPolicy;
                
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the points configuration for a points-based program.
        /// </summary>
        public void UpdatePointsConfig(PointsConfig pointsConfig)
        {
            if (Type != LoyaltyProgramType.Points)
                throw new InvalidOperationException("Cannot update points configuration for non-points program");

            if (pointsConfig == null)
                throw new ArgumentNullException(nameof(pointsConfig));

            PointsConfig = pointsConfig;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns the tier for a given point balance, or null if not found.
        /// </summary>
        public LoyaltyTier GetTierForPoints(int pointBalance)
        {
            if (!HasTiers || Type != LoyaltyProgramType.Points)
                return null;

            // Get all tiers where the point threshold is less than or equal to the balance
            var qualifyingTiers = _tiers.Where(t => t.PointThreshold <= pointBalance)
                                      .OrderByDescending(t => t.PointThreshold)
                                      .ToList();

            // Return the highest tier (with highest threshold) that the customer qualifies for
            return qualifyingTiers.FirstOrDefault();
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
        public decimal CalculatePoints(decimal transactionAmount, LoyaltyTier customerTier = null)
        {
            if (Type != LoyaltyProgramType.Points)
                throw new InvalidOperationException("Cannot calculate points for non-points program");

            if (MinimumTransactionAmount.HasValue && transactionAmount < MinimumTransactionAmount.Value)
                return 0;

            // If we have a detailed points config, use it
            if (PointsConfig != null)
            {
                decimal tierMultiplier = 1.0m;
                if (HasTiers && customerTier != null)
                {
                    tierMultiplier = customerTier.PointMultiplier;
                }
                
                return PointsConfig.CalculatePoints(transactionAmount, tierMultiplier);
            }
            
            // Fall back to simple conversion rate
            if (!PointsConversionRate.HasValue)
                throw new InvalidOperationException("Missing points conversion rate and points config");
                
            decimal basePoints = Math.Floor(transactionAmount * PointsConversionRate.Value);
            
            // Apply tier multiplier if applicable
            if (HasTiers && customerTier != null)
            {
                return Math.Floor(basePoints * customerTier.PointMultiplier);
            }
            
            return basePoints;
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
        
        /// <summary>
        /// Adds an existing tier to the program.
        /// Used by the repository for loading from database.
        /// </summary>
        public void AddTier(LoyaltyTier tier)
        {
            if (Type != LoyaltyProgramType.Points)
                throw new InvalidOperationException("Tiers are only supported for points-based programs");
                
            _tiers.Add(tier);
        }

        /// <summary>
        /// Sets the points configuration.
        /// Used by the repository for loading from database.
        /// </summary>
        internal void SetPointsConfig(PointsConfig pointsConfig)
        {
            if (Type != LoyaltyProgramType.Points)
                throw new InvalidOperationException("Cannot set points configuration for non-points program");
                
            PointsConfig = pointsConfig;
        }
        
        private void ValidateProgramTypeParameters(
            LoyaltyProgramType type, 
            int? stampThreshold, 
            decimal? pointsConversionRate,
            PointsConfig pointsConfig = null)
        {
            if (type == LoyaltyProgramType.Stamp)
            {
                if (stampThreshold.HasValue && stampThreshold.Value <= 0)
                    throw new ArgumentException("Stamp threshold must be greater than zero", nameof(stampThreshold));
            }
            else if (type == LoyaltyProgramType.Points)
            {
                // Either need points conversion rate or a points config
                if (pointsConversionRate.HasValue && pointsConversionRate.Value <= 0)
                    throw new ArgumentException("Points conversion rate must be greater than zero", nameof(pointsConversionRate));
            }
        }
    }
} 