using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class Reward
{
    public Reward(string title, string description, int pointsRequired) =>
        (Title, Description, PointsRequired) = (title, description, pointsRequired);
    
    public string Title { get; set; }
    public string Description { get; set; }
    public int PointsRequired { get; set; }  // For example, if users earn points and can redeem them for rewards
    public DateTime? ExpirationDate { get; set; } // Optional. Some rewards might expire.
    public RewardType Type { get; set; } = RewardType.FreeItem; // Enum for different types of rewards. e.g., Discount, FreeItem, Cashback, etc.
}
