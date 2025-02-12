namespace LoyaltySystem.Repositories;

using LoyaltySystem.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB;
using System.Linq;
using System;

public interface IRewardsRepository
{
    Task<int> CreateRewardAsync(Reward reward);
    Task<List<Reward>> GetRewardsByBusinessAsync(int businessId);
    Task<Reward?> GetRewardByIdAsync(int businessId, int rewardId);
    Task UpdateRewardAsync(Reward reward);
    Task DeleteRewardAsync(int businessId, int rewardId);
}

public class RewardsRepository : IRewardsRepository
{
    private readonly IConfiguration _config;

    public RewardsRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateRewardAsync(Reward reward)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        reward.CreatedAt = DateTime.UtcNow;
        reward.UpdatedAt = DateTime.UtcNow;

        var newId = await db.InsertWithIdentityAsync(reward);
        return (int)newId;
    }

    public async Task<List<Reward>> GetRewardsByBusinessAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.Rewards
            .Where(r => r.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task<Reward?> GetRewardByIdAsync(int businessId, int rewardId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.Rewards
            .Where(r => r.BusinessId == businessId && r.RewardId == rewardId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateRewardAsync(Reward reward)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        reward.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(reward);
    }

    public async Task DeleteRewardAsync(int businessId, int rewardId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.Rewards
            .Where(r => r.BusinessId == businessId && r.RewardId == rewardId)
            .DeleteAsync();
    }
}
