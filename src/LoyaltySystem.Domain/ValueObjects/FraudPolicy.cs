using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.ValueObjects;

/// <summary>
/// Represents fraud prevention policies for loyalty programs
/// </summary>
public record FraudPolicy
{
    public TimeSpan? CooldownBetweenScans { get; init; }
    public int? MaxRedemptionsPerDay { get; init; }
    public int? MaxPointsPerVelocityWindow { get; init; }
    public TimeSpan? VelocityWindow { get; init; }
    public int? MaxDistanceMeters { get; init; }
    public bool BlockOutsideOperatingHours { get; init; }

    public FraudPolicy(
        TimeSpan? cooldown = null,
        int? maxRedemptions = null,
        int? maxPoints = null,
        TimeSpan? velocityWindow = null,
        int? maxDistance = null,
        bool blockOutsideHours = false)
    {
        if (cooldown.HasValue && cooldown <= TimeSpan.Zero)
            throw new DomainException("Cooldown must be positive");
        if (maxRedemptions.HasValue && maxRedemptions <= 0)
            throw new DomainException("Max redemptions must be positive");
        if (maxPoints.HasValue && maxPoints <= 0)
            throw new DomainException("Max points must be positive");
        if (velocityWindow.HasValue && velocityWindow <= TimeSpan.Zero)
            throw new DomainException("Velocity window must be positive");
        if (maxDistance.HasValue && maxDistance <= 0)
            throw new DomainException("Max distance must be positive");

        CooldownBetweenScans = cooldown;
        MaxRedemptionsPerDay = maxRedemptions;
        MaxPointsPerVelocityWindow = maxPoints;
        VelocityWindow = velocityWindow;
        MaxDistanceMeters = maxDistance;
        BlockOutsideOperatingHours = blockOutsideHours;
    }
} 