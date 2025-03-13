using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Domain.Repositories
{
    public interface ILoyaltyRewardsRepository
    {
        Task<Reward> GetByIdAsync(RewardId id);
        Task<IEnumerable<Reward>> GetByProgramIdAsync(LoyaltyProgramId programId);
        Task<IEnumerable<Reward>> GetActiveByProgramIdAsync(LoyaltyProgramId programId);
        Task<Reward> AddAsync(Reward reward);
        Task UpdateAsync(Reward reward);
        Task<bool> DeleteAsync(RewardId id);
        
        // Analytics methods
        Task<int> GetTotalRedemptionsAsync(RewardId rewardId);
        Task<int> GetRedemptionsInLastDaysAsync(RewardId rewardId, int days);
        Task<decimal> GetTotalPointsRedeemedAsync(RewardId rewardId);
        Task<decimal> GetAveragePointsPerRedemptionAsync(RewardId rewardId);
        Task<DateTime?> GetFirstRedemptionDateAsync(RewardId rewardId);
        Task<DateTime?> GetLastRedemptionDateAsync(RewardId rewardId);
        Task<Dictionary<string, int>> GetRedemptionsByStoreAsync(RewardId rewardId);
        Task<Dictionary<string, int>> GetRedemptionsByMonthAsync(RewardId rewardId);
        
        // Program analytics
        Task<int> GetTotalRewardsCountAsync(LoyaltyProgramId programId);
        Task<int> GetActiveRewardsCountAsync(LoyaltyProgramId programId);
        Task<int> GetTotalProgramRedemptionsAsync(LoyaltyProgramId programId);
        Task<decimal> GetTotalProgramPointsRedeemedAsync(LoyaltyProgramId programId);
        Task<List<Reward>> GetTopRewardsByRedemptionsAsync(LoyaltyProgramId programId, int limit = 5);
        Task<Dictionary<string, int>> GetProgramRedemptionsByTypeAsync(LoyaltyProgramId programId);
        Task<Dictionary<string, int>> GetProgramRedemptionsByMonthAsync(LoyaltyProgramId programId);
    }
} 