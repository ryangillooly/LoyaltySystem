using System;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class RewardAnalyticsDto
    {
        public string RewardId { get; set; }
        public string RewardTitle { get; set; }
        public int TotalRedemptions { get; set; }
        public int RedemptionsLast30Days { get; set; }
        public int RedemptionsLast7Days { get; set; }
        public decimal AveragePointsPerRedemption { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public DateTime? FirstRedemptionDate { get; set; }
        public DateTime? LastRedemptionDate { get; set; }
        public Dictionary<string, int> RedemptionsByStore { get; set; }
        public Dictionary<string, int> RedemptionsByMonth { get; set; }

        public RewardAnalyticsDto()
        {
            RedemptionsByStore = new Dictionary<string, int>();
            RedemptionsByMonth = new Dictionary<string, int>();
        }
    }

    public class ProgramRewardsAnalyticsDto
    {
        public string ProgramId { get; set; }
        public string ProgramName { get; set; }
        public int TotalRewards { get; set; }
        public int ActiveRewards { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public List<RewardAnalyticsDto> TopRewards { get; set; }
        public Dictionary<string, int> RedemptionsByRewardType { get; set; }
        public Dictionary<string, int> RedemptionsByMonth { get; set; }

        public ProgramRewardsAnalyticsDto()
        {
            TopRewards = new List<RewardAnalyticsDto>();
            RedemptionsByRewardType = new Dictionary<string, int>();
            RedemptionsByMonth = new Dictionary<string, int>();
        }
    }
} 