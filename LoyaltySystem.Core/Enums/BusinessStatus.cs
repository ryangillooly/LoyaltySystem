namespace LoyaltySystem.Core.Enums;

public enum BusinessStatus
{
    Inactive,   // Business is not operational
    Pending,
    Active,     // Business is operational and open to the public
    TempClosed, // Temporarily closed, e.g., for renovations or due to an emergency
    PermanentlyClosed, // No longer in business
    ComingSoon  // Business is planned but not yet operational
}
