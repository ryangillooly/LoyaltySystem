namespace LoyaltySystem.Domain.Enums
{
    /// <summary>
    /// Represents the status of a user.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// The user is active and can log in.
        /// </summary>
        Active = 1,
        
        /// <summary>
        /// The user is inactive and cannot log in.
        /// </summary>
        Inactive = 2,
        
        /// <summary>
        /// The user account is locked.
        /// </summary>
        Locked = 3
    }
} 