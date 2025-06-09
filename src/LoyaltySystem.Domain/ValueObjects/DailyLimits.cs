using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.ValueObjects;

/// <summary>
/// Represents daily limits for loyalty program transactions
/// Value ranges: MaxStamps (1-1000), MaxPoints (1-100000), MaxRedemptions (1-100)
/// </summary>
public record DailyLimits
{
    public int? MaxStampsPerDay { get; init; }
    public int? MaxPointsPerDay { get; init; }
    public int? MaxRedemptionsPerDay { get; init; }
    public decimal? MaxCurrencyValuePerDay { get; init; }

    public DailyLimits(
        int? maxStamps = null,
        int? maxPoints = null,
        int? maxRedemptions = null,
        decimal? maxCurrencyValue = null)
    {
        if (maxStamps.HasValue && (maxStamps <= 0 || maxStamps > 1000))
            throw new DomainException("Max stamps per day must be between 1 and 1000");
        
        if (maxPoints.HasValue && (maxPoints <= 0 || maxPoints > 100000))
            throw new DomainException("Max points per day must be between 1 and 100000");
        
        if (maxRedemptions.HasValue && (maxRedemptions <= 0 || maxRedemptions > 100))
            throw new DomainException("Max redemptions per day must be between 1 and 100");
        
        if (maxCurrencyValue.HasValue && (maxCurrencyValue <= 0 || maxCurrencyValue > 10000))
            throw new DomainException("Max currency value per day must be between 0.01 and 10000");

        MaxStampsPerDay = maxStamps;
        MaxPointsPerDay = maxPoints;
        MaxRedemptionsPerDay = maxRedemptions;
        MaxCurrencyValuePerDay = maxCurrencyValue;
    }

    /// <summary>
    /// Validates if a stamp transaction is within daily limits
    /// </summary>
    public bool IsStampTransactionAllowed(int currentDailyStamps, int newStamps)
    {
        if (!MaxStampsPerDay.HasValue) return true;
        return currentDailyStamps + newStamps <= MaxStampsPerDay.Value;
    }

    /// <summary>
    /// Validates if a points transaction is within daily limits
    /// </summary>
    public bool IsPointsTransactionAllowed(int currentDailyPoints, int newPoints)
    {
        if (!MaxPointsPerDay.HasValue) return true;
        return currentDailyPoints + newPoints <= MaxPointsPerDay.Value;
    }

    /// <summary>
    /// Validates if a redemption is within daily limits
    /// </summary>
    public bool IsRedemptionAllowed(int currentDailyRedemptions)
    {
        if (!MaxRedemptionsPerDay.HasValue) return true;
        return currentDailyRedemptions < MaxRedemptionsPerDay.Value;
    }

    /// <summary>
    /// Validates if a currency redemption is within daily limits
    /// </summary>
    public bool IsCurrencyRedemptionAllowed(decimal currentDailyCurrency, decimal newRedemptionValue)
    {
        if (!MaxCurrencyValuePerDay.HasValue) return true;
        return currentDailyCurrency + newRedemptionValue <= MaxCurrencyValuePerDay.Value;
    }
} 