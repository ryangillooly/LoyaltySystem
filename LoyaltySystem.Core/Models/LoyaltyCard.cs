using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class LoyaltyCard
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid UserId { get; set; }
    public int StampCount { get; set; }
    public DateTime DateIssued { get; set; }
    public DateTime DateLastStamped { get; set; }
    public LoyaltyStatus Status { get; set; }
}