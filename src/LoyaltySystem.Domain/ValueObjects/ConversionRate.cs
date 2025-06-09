using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.ValueObjects;

/// <summary>
/// Represents conversion rates between stamps, points, and currency
/// Value range: StampsToPoints (1-100), PointsToCurrency (0.01-10.00)
/// </summary>
public record ConversionRate
{
    public int StampsToPoints { get; init; }
    public decimal PointsToCurrency { get; init; }

    public ConversionRate(int stampsToPoints, decimal pointsToCurrency)
    {
        if (stampsToPoints <= 0 || stampsToPoints > 100)
            throw new DomainException("Stamps to points conversion must be between 1 and 100");
        
        if (pointsToCurrency <= 0 || pointsToCurrency > 10.00m)
            throw new DomainException("Points to currency conversion must be between 0.01 and 10.00");

        StampsToPoints = stampsToPoints;
        PointsToCurrency = pointsToCurrency;
    }

    /// <summary>
    /// Converts stamps to points using the conversion rate
    /// </summary>
    public int ConvertStampsToPoints(int stamps)
    {
        if (stamps < 0)
            throw new DomainException("Stamps cannot be negative");
        
        return stamps * StampsToPoints;
    }

    /// <summary>
    /// Converts points to currency value using the conversion rate
    /// </summary>
    public decimal ConvertPointsToCurrency(int points)
    {
        if (points < 0)
            throw new DomainException("Points cannot be negative");
        
        return points * PointsToCurrency;
    }

    /// <summary>
    /// Calculates how many stamps are needed for a specific currency value
    /// </summary>
    public int CalculateStampsNeededForCurrency(decimal currencyValue)
    {
        if (currencyValue <= 0)
            throw new DomainException("Currency value must be positive");
        
        var pointsNeeded = Math.Ceiling(currencyValue / PointsToCurrency);
        var stampsNeeded = Math.Ceiling(pointsNeeded / StampsToPoints);
        
        return (int)stampsNeeded;
    }
} 