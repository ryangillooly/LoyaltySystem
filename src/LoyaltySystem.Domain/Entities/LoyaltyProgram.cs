using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Entities;

/// <summary>
/// Represents a loyalty program offered by a brand.
/// This is an Aggregate Root.
/// </summary>
public sealed class LoyaltyProgram : Entity<LoyaltyProgramId>
{
    private readonly List<Reward> _rewards = new ();
    private readonly List<LoyaltyTier> _tiers = new ();
        
    public BrandId BrandId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public LoyaltyProgramType Type { get; set; }
    public ExpirationPolicy ExpirationPolicy { get; set; }
    public int? StampThreshold { get; set; }
    public decimal? PointsConversionRate { get; set; }
    public PointsConfig PointsConfig { get; set; }
    public int? DailyStampLimit { get; set; }
    public decimal? MinimumTransactionAmount { get; set; }
    public PointsRoundingRule PointsRoundingRule { get; set; }
    public int MinimumPointsForRedemption { get; set; }
    public int PointsPerPound { get; set; }
    public bool HasTiers { get; set; }
    public string TermsAndConditions { get; set; }
    public int EnrollmentBonusPoints { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Collection navigation properties
    public IReadOnlyCollection<Reward> Rewards => _rewards.AsReadOnly();
    public IReadOnlyCollection<LoyaltyTier> Tiers => _tiers.AsReadOnly();
    

    /// <summary>
    /// Initializes a new instance of the <see cref="LoyaltyProgram"/> class with the specified configuration, including program type, thresholds, conversion rates, and optional settings.
    /// </summary>
    /// <param name="brandId">The unique identifier of the brand offering the loyalty program.</param>
    /// <param name="name">The name of the loyalty program.</param>
    /// <param name="type">The type of the loyalty program (e.g., stamp-based or points-based).</param>
    /// <param name="stampThreshold">The number of stamps required for a reward, applicable to stamp-based programs.</param>
    /// <param name="pointsConversionRate">The conversion rate from transaction amount to points, applicable to points-based programs.</param>
    /// <param name="pointsConfig">Optional configuration for points calculation and rules, applicable to points-based programs.</param>
    /// <param name="hasTiers">Indicates whether the program supports loyalty tiers.</param>
    /// <param name="dailyStampLimit">Optional daily limit for stamp issuance.</param>
    /// <param name="minimumTransactionAmount">Optional minimum transaction amount required to earn points.</param>
    /// <param name="expirationPolicy">Optional expiration policy for earned rewards or points.</param>
    /// <param name="description">Optional description of the loyalty program.</param>
    /// <param name="termsAndConditions">Optional terms and conditions for the program.</param>
    /// <param name="enrollmentBonusPoints">Optional bonus points awarded upon enrollment.</param>
    /// <param name="id">Optional unique identifier for the loyalty program.</param>
    /// <param name="startDate">Optional start date for the program; defaults to current UTC time if not provided.</param>
    /// <param name="endDate">Optional end date for the program.</param>
    /// <param name="isActive">Optional flag indicating whether the program is active; defaults to true.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="brandId"/> is empty or <paramref name="name"/> is null or whitespace, or if required parameters are invalid for the specified program type.</exception>
    public LoyaltyProgram
    (
        BrandId brandId,
        string name,
        LoyaltyProgramType type,
        int? stampThreshold,
        decimal? pointsConversionRate = null,
        PointsConfig pointsConfig = null,
        bool hasTiers = false,
        int? dailyStampLimit = null,
        decimal? minimumTransactionAmount = null,
        ExpirationPolicy expirationPolicy = null,
        string description = null,
        string termsAndConditions = null,
        int enrollmentBonusPoints = 0,
        LoyaltyProgramId? id = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool? isActive = null
    )
    : base(new LoyaltyProgramId())
    {
        if (brandId == Guid.Empty)
            throw new ArgumentException("BrandId cannot be empty", nameof(brandId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Program name cannot be empty", nameof(name));

        ValidateProgramTypeParameters(type, stampThreshold, pointsConversionRate, pointsConfig);
        
        Type = type ;
        IsActive = isActive.Value;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        BrandId = brandId ?? throw new ArgumentNullException(nameof(brandId));
        Description = description;
        StampThreshold = type == LoyaltyProgramType.Stamp ? stampThreshold : null;
        PointsConversionRate = type == LoyaltyProgramType.Points ? pointsConversionRate : null;
            
        // Only initialize PointsConfig if explicitly provided or if no conversion rate
        if (type == LoyaltyProgramType.Points)
        {
            if (pointsConfig is { })
                PointsConfig = pointsConfig;
            else if (!pointsConversionRate.HasValue)
                PointsConfig = new PointsConfig();
            else
                PointsConfig = null;
        }
        else
            PointsConfig = null;
            
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
    /// <summary>
    /// Creates a new reward for the loyalty program and adds it to the program's rewards.
    /// </summary>
    /// <param name="title">The title of the reward. Must not be empty.</param>
    /// <param name="description">A description of the reward.</param>
    /// <param name="requiredValue">The value required to redeem the reward. Must be greater than zero.</param>
    /// <param name="validFrom">Optional start date for reward validity.</param>
    /// <param name="validTo">Optional end date for reward validity.</param>
    /// <returns>The created <see cref="Reward"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the title is empty or requiredValue is not greater than zero.</exception>
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
    /// <summary>
    /// Creates and adds a new loyalty tier to the program with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the tier.</param>
    /// <param name="pointThreshold">The minimum points required to qualify for this tier.</param>
    /// <param name="pointMultiplier">The multiplier applied to points earned in this tier.</param>
    /// <param name="tierOrder">The order of the tier within the program.</param>
    /// <param name="benefits">Optional list of benefits associated with the tier.</param>
    /// <returns>The created <see cref="LoyaltyTier"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the program does not support tiers, is not points-based, or if a tier with the same order already exists.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the tier name is empty, the point threshold is negative, or the point multiplier is not greater than zero.
    /// </exception>
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
    /// <summary>
    /// Updates the properties of the loyalty program with the provided values, validating parameters according to the program type.
    /// </summary>
    /// <param name="name">The new name of the loyalty program. Cannot be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown if the name is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if tiers are enabled for a non-points-based program.</exception>
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
    /// <summary>
    /// Updates the points configuration for a points-based loyalty program.
    /// </summary>
    /// <param name="pointsConfig">The new points configuration to apply.</param>
    /// <exception cref="InvalidOperationException">Thrown if the program is not points-based.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pointsConfig"/> is null.</exception>
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
    /// <summary>
    /// Returns the highest loyalty tier that a customer qualifies for based on their point balance, or null if the program does not support tiers or is not points-based.
    /// </summary>
    /// <param name="pointBalance">The customer's current point balance.</param>
    /// <returns>The qualifying <see cref="LoyaltyTier"/> with the highest threshold, or null if none qualify.</returns>
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
    /// <summary>
    /// Marks the loyalty program as active and updates the last modified timestamp.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the loyalty program.
    /// <summary>
    /// Deactivates the loyalty program and updates the last modified timestamp.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the program is valid for stamp issuance.
    /// <summary>
    /// Determines whether the loyalty program is active and of the stamp type, allowing stamp issuance.
    /// </summary>
    /// <returns>True if the program is active and stamp-based; otherwise, false.</returns>
    public bool IsValidForStampIssuance()
    {
        return IsActive && Type == LoyaltyProgramType.Stamp;
    }

    /// <summary>
    /// Checks if the program is valid for points issuance.
    /// <summary>
    /// Determines whether points can be issued for a transaction based on program status, type, and minimum transaction amount.
    /// </summary>
    /// <param name="transactionAmount">The amount of the transaction to evaluate.</param>
    /// <returns>True if the program is active, points-based, and the transaction meets the minimum amount (if set); otherwise, false.</returns>
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
    /// <summary>
    /// Calculates the number of points earned for a transaction amount, applying tier multipliers and points configuration if applicable.
    /// </summary>
    /// <param name="transactionAmount">The amount of the transaction for which points are to be calculated.</param>
    /// <param name="customerTier">The customer's loyalty tier, used to apply a point multiplier if tiers are enabled.</param>
    /// <returns>The number of points earned for the transaction.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if called on a non-points program or if both points configuration and conversion rate are missing.
    /// </exception>
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
    /// <summary>
    /// Sets the expiration policy for the loyalty program. Intended for internal or repository use.
    /// </summary>
    internal void SetExpirationPolicy(ExpirationPolicy expirationPolicy)
    {
        ExpirationPolicy = expirationPolicy;
    }
        
    /// <summary>
    /// Adds an existing reward to the program.
    /// Used by the repository for loading from database.
    /// <summary>
    /// Adds an existing reward to the loyalty program.
    /// </summary>
    public void AddReward(Reward reward)
    {
        _rewards.Add(reward);
    }
        
    /// <summary>
    /// Adds an existing tier to the program.
    /// Used by the repository for loading from database.
    /// <summary>
    /// Adds an existing loyalty tier to the program if it is points-based.
    /// </summary>
    /// <param name="tier">The loyalty tier to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if the program is not points-based.</exception>
    public void AddTier(LoyaltyTier tier)
    {
        if (Type != LoyaltyProgramType.Points)
            throw new InvalidOperationException("Tiers are only supported for points-based programs");
                
        _tiers.Add(tier);
    }

    /// <summary>
    /// Sets the points configuration.
    /// Used by the repository for loading from database.
    /// <summary>
    /// Sets the points configuration for a points-based loyalty program.
    /// </summary>
    /// <param name="pointsConfig">The points configuration to assign.</param>
    /// <exception cref="InvalidOperationException">Thrown if the program type is not points-based.</exception>
    internal void SetPointsConfig(PointsConfig pointsConfig)
    {
        if (Type != LoyaltyProgramType.Points)
            throw new InvalidOperationException("Cannot set points configuration for non-points program");
                
        PointsConfig = pointsConfig;
    }
        
    /// <summary>
    /// Validates that the provided parameters are appropriate for the specified loyalty program type.
    /// </summary>
    /// <param name="type">The type of loyalty program (stamp or points).</param>
    /// <param name="stampThreshold">The stamp threshold for stamp-based programs.</param>
    /// <param name="pointsConversionRate">The points conversion rate for points-based programs.</param>
    /// <param name="pointsConfig">Optional points configuration for points-based programs.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the stamp threshold is not greater than zero for stamp programs,
    /// or if the points conversion rate is not greater than zero for points programs.
    /// </exception>
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