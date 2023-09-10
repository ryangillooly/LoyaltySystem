namespace LoyaltySystem.Core.Enums;

public enum UserStatus
{
    Pending = 1, // Pending approval or email confirmation
    Active = 2,  // Successfully registered and approved
    Suspended = 3, // Suspended for some reason
    Deactivated = 4 // User has been deactivated
}