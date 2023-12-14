namespace LoyaltySystem.Core.Enums;

public enum UserStatus
{
    Pending, // Pending approval or email confirmation
    Active,  // Successfully registered and approved
    Suspended, // Suspended for some reason
    Deactivated // User has been deactivated
}