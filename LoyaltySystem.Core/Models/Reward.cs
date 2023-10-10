using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class Reward
{
    public Reward()
    {
    }

    public Reward(Guid id, string title, string description, int pointsRequired) =>
        (Id, Title, Description, PointsRequired) = (id, title, description, pointsRequired);
    
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string Description { get; set; }
    public int PointsRequired { get; set; }  // For example, if users earn points and can redeem them for rewards
    public DateTime? ExpirationDate { get; set; } // Optional. Some rewards might expire.
    public RewardType Type { get; set; } = RewardType.FreeItem; // Enum for different types of rewards. e.g., Discount, FreeItem, Cashback, etc.
}
