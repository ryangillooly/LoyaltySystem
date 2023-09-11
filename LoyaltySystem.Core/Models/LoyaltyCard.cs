using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class LoyaltyCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BusinessId { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int StampCount { get; set; } = 0;
    public DateTime DateIssued { get; set; } = DateTime.UtcNow;
    public DateTime DateLastStamped { get; set; } = DateTime.UtcNow;
    public LoyaltyStatus Status { get; set; } = LoyaltyStatus.Active;
}