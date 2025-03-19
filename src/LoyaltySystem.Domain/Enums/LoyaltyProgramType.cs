namespace LoyaltySystem.Domain.Enums
{
    /// <summary>
    /// Defines the type of loyalty program.
    /// </summary>
    public enum LoyaltyProgramType
    {
        /// <summary>
        /// Stamp-based loyalty program where customers collect stamps.
        /// </summary>
        Stamp = 1,
        
        /// <summary>
        /// Points-based loyalty program where customers accumulate points based on purchase amounts.
        /// </summary>
        Points = 2
    }
} 