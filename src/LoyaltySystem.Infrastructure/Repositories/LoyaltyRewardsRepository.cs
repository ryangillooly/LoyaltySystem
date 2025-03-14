using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Infrastructure.Repositories;

public class LoyaltyRewardsRepository : ILoyaltyRewardsRepository
{

    public Task<Reward> GetByIdAsync(RewardId id) =>
        throw new NotImplementedException();
    public Task<IEnumerable<Reward>> GetByProgramIdAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
    public Task<IEnumerable<Reward>> GetActiveByProgramIdAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
    public Task<Reward> AddAsync(Reward reward) =>
        throw new NotImplementedException();
    public Task UpdateAsync(Reward reward) =>
        throw new NotImplementedException();
    public Task<bool> DeleteAsync(RewardId id) =>
        throw new NotImplementedException();
    public Task<int> GetTotalRedemptionsAsync(RewardId rewardId) =>
        throw new NotImplementedException();
    public Task<int> GetRedemptionsInLastDaysAsync(RewardId rewardId, int days) =>
        throw new NotImplementedException();
    public Task<decimal> GetTotalPointsRedeemedAsync(RewardId rewardId) =>
        throw new NotImplementedException();
    public Task<decimal> GetAveragePointsPerRedemptionAsync(RewardId rewardId) =>
        throw new NotImplementedException();
    public Task<DateTime?> GetFirstRedemptionDateAsync(RewardId rewardId) =>
        throw new NotImplementedException();
    public Task<DateTime?> GetLastRedemptionDateAsync(RewardId rewardId) =>
        throw new NotImplementedException();
    public Task<Dictionary<string, int>> GetRedemptionsByStoreAsync(RewardId rewardId) =>
        throw new NotImplementedException();
    public Task<Dictionary<string, int>> GetRedemptionsByMonthAsync(RewardId rewardId) =>
        throw new NotImplementedException();
    public Task<int> GetTotalRewardsCountAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
    public Task<int> GetActiveRewardsCountAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
    public Task<int> GetTotalProgramRedemptionsAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
    public Task<decimal> GetTotalProgramPointsRedeemedAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
    public Task<List<Reward>> GetTopRewardsByRedemptionsAsync(LoyaltyProgramId programId, int limit = 5) =>
        throw new NotImplementedException();
    public Task<Dictionary<string, int>> GetProgramRedemptionsByTypeAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
    public Task<Dictionary<string, int>> GetProgramRedemptionsByMonthAsync(LoyaltyProgramId programId) =>
        throw new NotImplementedException();
}
