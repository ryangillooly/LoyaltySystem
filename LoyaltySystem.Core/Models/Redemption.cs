namespace LoyaltySystem.Core.Models;

public class Redemption
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid CardId { get; set; }
    public int PointsRedeemed { get; set; }
    public DateTime Timestamp { get; set; }
}