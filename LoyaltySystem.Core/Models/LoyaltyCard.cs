using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class LoyaltyCard
{
    public LoyaltyCard()
    {
        BusinessId = Guid.Empty;
        UserId     = Guid.Empty;
    }
    
    public LoyaltyCard(Guid userId, Guid businessId)
    {
        BusinessId = businessId;
        UserId     = userId;
    }
    
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid BusinessId { get; set; }
    public Guid UserId { get; set; }
    public int Points { get; set; } = 1;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime LastStampedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastRedeemDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public LoyaltyStatus Status { get; set; } = LoyaltyStatus.Active;

    public bool IsActive() => Status == LoyaltyStatus.Active;
    public bool IsNotActive() => Status != LoyaltyStatus.Active;
    
    public static LoyaltyCard Merge(LoyaltyCard current, LoyaltyCard updated) =>
        new (current.UserId, current.BusinessId)
        {
            Id              = current.Id,
            Points          = current.Points,
            IssueDate       = current.IssueDate,
            LastStampedDate = current.LastStampedDate,
            LastUpdatedDate = DateTime.UtcNow,
            Status          = updated.Status == current.Status ? current.Status : updated.Status
        };
}