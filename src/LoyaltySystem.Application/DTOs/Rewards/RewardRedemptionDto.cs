namespace LoyaltySystem.Application.DTOs.Rewards;

public class RewardRedemptionDto
{
    public string Id { get; set; } = string.Empty;
    public string RewardId { get; set; } = string.Empty;
    public string RewardTitle { get; set; } = string.Empty;
    public string LoyaltyCardId { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public DateTime RedeemedAt { get; set; }
    public int PointsUsed { get; set; }
    public string RedemptionCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public string? StoreId { get; set; }
    public string? StoreName { get; set; }
}