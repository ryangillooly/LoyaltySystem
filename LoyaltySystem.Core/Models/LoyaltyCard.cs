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
    public DateTime DateIssued { get; set; } = DateTime.UtcNow;
    public DateTime DateLastStamped { get; set; } = DateTime.UtcNow;
    public LoyaltyStatus Status { get; set; } = LoyaltyStatus.Active;
    
    public static LoyaltyCard Merge(LoyaltyCard current, LoyaltyCard updated) =>
        new (current.UserId, current.BusinessId)
        {
            Id              = current.Id,
            Points          = current.Points,
            DateIssued      = current.DateIssued,
            DateLastStamped = current.DateLastStamped,
            Status          = updated.Status == current.Status ? current.Status : updated.Status
        };
}