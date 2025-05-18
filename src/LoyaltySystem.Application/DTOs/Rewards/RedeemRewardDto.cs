using System;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms;

public class RedeemRewardDto
{
    public string RewardId { get; set; } = string.Empty;
    public string LoyaltyCardId { get; set; } = string.Empty;
    public string? StoreId { get; set; }
    public string? Notes { get; set; }
}