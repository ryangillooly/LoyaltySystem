namespace LoyaltySystem.Domain.Enums;

/// <summary>
/// Specifies the type of identifier used for authentication
/// </summary>
public enum LoginIdentifierType
{
    /// <summary>
    /// Use email address to authenticate the user
    /// </summary>
    Email,
    
    /// <summary>
    /// Use username to authenticate the user
    /// </summary>
    Username
} 