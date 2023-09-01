namespace LoyaltySystem.Core.Enums;

public enum LoyaltyStatus
{
    Inactive = 0,  // The loyalty card is not yet activated or used
    Active = 1,    // The loyalty card is active and can accumulate points
    Redeemed = 2,  // Some or all points have been redeemed
    Expired = 3,   // The loyalty card has expired and can no longer be used
    Suspended = 4  // The loyalty card is temporarily suspended, possibly for investigation or inactivity
}
