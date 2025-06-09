using System;
using System.Collections.Generic;
using System.Linq;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.ValueObjects;

/// <summary>
/// Value object representing reward configuration settings with validation and business rules.
/// Encapsulates reward type, cost, availability, and redemption constraints.
/// </summary>
public record RewardConfiguration
{
    /// <summary>
    /// The type of reward (e.g., "discount", "freebie", "cashback", "gift_card")
    /// </summary>
    public string RewardType { get; init; }

    /// <summary>
    /// The cost in points required to redeem this reward (must be positive)
    /// </summary>
    public int PointsCost { get; init; }

    /// <summary>
    /// The monetary value of the reward in the base currency (0-10000 range)
    /// </summary>
    public decimal MonetaryValue { get; init; }

    /// <summary>
    /// The currency code for the monetary value (ISO 4217 format)
    /// </summary>
    public string Currency { get; init; }

    /// <summary>
    /// Whether this reward is currently active and available for redemption
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// The date when this reward configuration becomes effective
    /// </summary>
    public DateTime EffectiveFrom { get; init; }

    /// <summary>
    /// The date when this reward configuration expires (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Maximum number of times this reward can be redeemed per customer (optional)
    /// </summary>
    public int? MaxRedemptionsPerCustomer { get; init; }

    /// <summary>
    /// Maximum total number of redemptions allowed for this reward (optional)
    /// </summary>
    public int? MaxTotalRedemptions { get; init; }

    /// <summary>
    /// Current number of redemptions for this reward configuration
    /// </summary>
    public int CurrentRedemptions { get; init; }

    /// <summary>
    /// Minimum customer tier required to redeem this reward (optional)
    /// </summary>
    public string? MinimumTier { get; init; }

    /// <summary>
    /// Additional terms and conditions for this reward
    /// </summary>
    public string? Terms { get; init; }

    /// <summary>
    /// Additional configuration data as key-value pairs
    /// </summary>
    public IReadOnlyDictionary<string, string> AdditionalConfig { get; init; }

    /// <summary>
    /// Creates a new RewardConfiguration with validation
    /// </summary>
    public RewardConfiguration(
        string rewardType,
        int pointsCost,
        decimal monetaryValue,
        string currency,
        bool isActive = true,
        DateTime? effectiveFrom = null,
        DateTime? expiresAt = null,
        int? maxRedemptionsPerCustomer = null,
        int? maxTotalRedemptions = null,
        int currentRedemptions = 0,
        string? minimumTier = null,
        string? terms = null,
        Dictionary<string, string>? additionalConfig = null)
    {
        // Validate reward type
        if (string.IsNullOrWhiteSpace(rewardType))
            throw new ArgumentException("Reward type cannot be null or empty", nameof(rewardType));
        
        if (rewardType.Length > 50)
            throw new ArgumentException("Reward type cannot exceed 50 characters", nameof(rewardType));

        var validRewardTypes = new[] { "discount", "freebie", "cashback", "gift_card", "experience", "merchandise" };
        if (!validRewardTypes.Contains(rewardType.ToLowerInvariant()))
            throw new ArgumentException($"Invalid reward type. Must be one of: {string.Join(", ", validRewardTypes)}", nameof(rewardType));

        // Validate points cost
        if (pointsCost <= 0)
            throw new ArgumentException("Points cost must be positive", nameof(pointsCost));

        if (pointsCost > 1000000)
            throw new ArgumentException("Points cost cannot exceed 1,000,000", nameof(pointsCost));

        // Validate monetary value
        if (monetaryValue < 0)
            throw new ArgumentException("Monetary value cannot be negative", nameof(monetaryValue));

        if (monetaryValue > 10000)
            throw new ArgumentException("Monetary value cannot exceed 10,000", nameof(monetaryValue));

        // Validate currency
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-character ISO 4217 code", nameof(currency));

        // Validate dates
        var effective = effectiveFrom ?? DateTime.UtcNow;
        if (expiresAt.HasValue && expiresAt.Value <= effective)
            throw new ArgumentException("Expiry date must be after effective date", nameof(expiresAt));

        // Validate redemption limits
        if (maxRedemptionsPerCustomer.HasValue && maxRedemptionsPerCustomer.Value <= 0)
            throw new ArgumentException("Max redemptions per customer must be positive", nameof(maxRedemptionsPerCustomer));

        if (maxTotalRedemptions.HasValue && maxTotalRedemptions.Value <= 0)
            throw new ArgumentException("Max total redemptions must be positive", nameof(maxTotalRedemptions));

        if (currentRedemptions < 0)
            throw new ArgumentException("Current redemptions cannot be negative", nameof(currentRedemptions));

        // Validate minimum tier
        if (!string.IsNullOrWhiteSpace(minimumTier) && minimumTier.Length > 20)
            throw new ArgumentException("Minimum tier cannot exceed 20 characters", nameof(minimumTier));

        // Validate terms
        if (!string.IsNullOrWhiteSpace(terms) && terms.Length > 1000)
            throw new ArgumentException("Terms cannot exceed 1000 characters", nameof(terms));

        // Validate additional config
        if (additionalConfig?.Count > 20)
            throw new ArgumentException("Additional config cannot have more than 20 entries", nameof(additionalConfig));

        RewardType = rewardType.ToLowerInvariant();
        PointsCost = pointsCost;
        MonetaryValue = monetaryValue;
        Currency = currency.ToUpperInvariant();
        IsActive = isActive;
        EffectiveFrom = effective;
        ExpiresAt = expiresAt;
        MaxRedemptionsPerCustomer = maxRedemptionsPerCustomer;
        MaxTotalRedemptions = maxTotalRedemptions;
        CurrentRedemptions = currentRedemptions;
        MinimumTier = minimumTier?.ToLowerInvariant();
        Terms = terms;
        AdditionalConfig = additionalConfig?.AsReadOnly() ?? new Dictionary<string, string>().AsReadOnly();
    }

    /// <summary>
    /// Checks if this reward configuration is currently valid and available
    /// </summary>
    public bool IsCurrentlyAvailable()
    {
        var now = DateTime.UtcNow;
        return IsActive && 
               now >= EffectiveFrom && 
               (!ExpiresAt.HasValue || now <= ExpiresAt.Value) &&
               (!MaxTotalRedemptions.HasValue || CurrentRedemptions < MaxTotalRedemptions.Value);
    }

    /// <summary>
    /// Checks if this reward has expired
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }

    /// <summary>
    /// Checks if the total redemption limit has been reached
    /// </summary>
    public bool HasReachedTotalLimit()
    {
        return MaxTotalRedemptions.HasValue && CurrentRedemptions >= MaxTotalRedemptions.Value;
    }

    /// <summary>
    /// Calculates the points-to-value ratio for this reward
    /// </summary>
    public decimal GetPointsToValueRatio()
    {
        return MonetaryValue > 0 ? PointsCost / MonetaryValue : 0;
    }

    /// <summary>
    /// Checks if a customer tier meets the minimum requirement
    /// </summary>
    public bool MeetsMinimumTier(string? customerTier)
    {
        if (string.IsNullOrWhiteSpace(MinimumTier))
            return true;

        if (string.IsNullOrWhiteSpace(customerTier))
            return false;

        // Simple tier comparison (could be enhanced with tier hierarchy)
        var tierOrder = new[] { "bronze", "silver", "gold", "platinum", "diamond" };
        var minIndex = Array.IndexOf(tierOrder, MinimumTier);
        var customerIndex = Array.IndexOf(tierOrder, customerTier.ToLowerInvariant());

        return customerIndex >= minIndex;
    }

    /// <summary>
    /// Gets a specific additional configuration value
    /// </summary>
    public string? GetConfigValue(string key)
    {
        return AdditionalConfig.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Creates a new RewardConfiguration with updated redemption count
    /// </summary>
    public RewardConfiguration WithUpdatedRedemptions(int newRedemptionCount)
    {
        if (newRedemptionCount < 0)
            throw new ArgumentException("Redemption count cannot be negative", nameof(newRedemptionCount));

        return this with { CurrentRedemptions = newRedemptionCount };
    }

    /// <summary>
    /// Creates a new RewardConfiguration with updated active status
    /// </summary>
    public RewardConfiguration WithActiveStatus(bool isActive)
    {
        return this with { IsActive = isActive };
    }

    /// <summary>
    /// Creates a new RewardConfiguration with updated expiry date
    /// </summary>
    public RewardConfiguration WithExpiryDate(DateTime? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt.Value <= EffectiveFrom)
            throw new ArgumentException("Expiry date must be after effective date", nameof(expiresAt));

        return this with { ExpiresAt = expiresAt };
    }

    /// <summary>
    /// Creates a new RewardConfiguration with additional config data
    /// </summary>
    public RewardConfiguration WithAdditionalConfig(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Config key cannot be null or empty", nameof(key));

        var newConfig = new Dictionary<string, string>(AdditionalConfig) { [key] = value };
        
        if (newConfig.Count > 20)
            throw new ArgumentException("Additional config cannot have more than 20 entries");

        return this with { AdditionalConfig = newConfig.AsReadOnly() };
    }

    /// <summary>
    /// Static factory method for discount rewards
    /// </summary>
    public static RewardConfiguration CreateDiscount(
        int pointsCost, 
        decimal discountAmount, 
        string currency = "USD",
        int? maxRedemptionsPerCustomer = null)
    {
        return new RewardConfiguration(
            "discount",
            pointsCost,
            discountAmount,
            currency,
            maxRedemptionsPerCustomer: maxRedemptionsPerCustomer);
    }

    /// <summary>
    /// Static factory method for freebie rewards
    /// </summary>
    public static RewardConfiguration CreateFreebie(
        int pointsCost,
        decimal itemValue,
        string currency = "USD",
        string? terms = null)
    {
        return new RewardConfiguration(
            "freebie",
            pointsCost,
            itemValue,
            currency,
            terms: terms);
    }

    /// <summary>
    /// Static factory method for gift card rewards
    /// </summary>
    public static RewardConfiguration CreateGiftCard(
        int pointsCost,
        decimal cardValue,
        string currency = "USD",
        DateTime? expiresAt = null)
    {
        return new RewardConfiguration(
            "gift_card",
            pointsCost,
            cardValue,
            currency,
            expiresAt: expiresAt);
    }
} 