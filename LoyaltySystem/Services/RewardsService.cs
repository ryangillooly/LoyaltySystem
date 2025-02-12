namespace LoyaltySystem.Services;

using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public interface IRewardsService
{
    Task<int> CreateRewardAsync(int businessId, CreateRewardDto dto);
    Task<List<Reward>> GetRewardsAsync(int businessId);
    Task<Reward?> GetRewardAsync(int businessId, int rewardId);
    Task UpdateRewardAsync(int businessId, int rewardId, UpdateRewardDto dto);
    Task DeleteRewardAsync(int businessId, int rewardId);
}

public class RewardsService : IRewardsService
{
    private readonly IRewardsRepository _repo;

    public RewardsService(IRewardsRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> CreateRewardAsync(int businessId, CreateRewardDto dto)
    {
        var reward = new Reward
        {
            BusinessId = businessId,
            RewardTitle = dto.RewardTitle,
            WhatYouGet = dto.WhatYouGet,
            FinePrint = dto.FinePrint,
            RewardImageUrl = dto.RewardImageUrl,
            IsGift = dto.IsGift,
            RewardType = dto.RewardType,
            ValidityStart = dto.ValidityStart,
            ValidityEnd = dto.ValidityEnd,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repo.CreateRewardAsync(reward);
    }

    public async Task<List<Reward>> GetRewardsAsync(int businessId)
    {
        return await _repo.GetRewardsByBusinessAsync(businessId);
    }

    public async Task<Reward?> GetRewardAsync(int businessId, int rewardId)
    {
        return await _repo.GetRewardByIdAsync(businessId, rewardId);
    }

    public async Task UpdateRewardAsync(int businessId, int rewardId, UpdateRewardDto dto)
    {
        var existing = await _repo.GetRewardByIdAsync(businessId, rewardId);
        if (existing == null)
        {
            throw new Exception("Reward not found");
        }

        existing.RewardTitle = dto.RewardTitle ?? existing.RewardTitle;
        existing.WhatYouGet = dto.WhatYouGet ?? existing.WhatYouGet;
        existing.FinePrint = dto.FinePrint ?? existing.FinePrint;
        existing.RewardImageUrl = dto.RewardImageUrl ?? existing.RewardImageUrl;
        if (dto.IsGift.HasValue) existing.IsGift = dto.IsGift.Value;
        existing.RewardType = dto.RewardType ?? existing.RewardType;
        if (dto.ValidityStart.HasValue) existing.ValidityStart = dto.ValidityStart.Value;
        if (dto.ValidityEnd.HasValue) existing.ValidityEnd = dto.ValidityEnd.Value;

        await _repo.UpdateRewardAsync(existing);
    }

    public async Task DeleteRewardAsync(int businessId, int rewardId)
    {
        await _repo.DeleteRewardAsync(businessId, rewardId);
    }
}

// =========== DTOs ===========

public class CreateRewardDto
{
    public string RewardTitle { get; set; }
    public string WhatYouGet { get; set; }
    public string FinePrint { get; set; }
    public string RewardImageUrl { get; set; }
    public bool IsGift { get; set; }
    public string RewardType { get; set; }
    public DateTime? ValidityStart { get; set; }
    public DateTime? ValidityEnd { get; set; }
}

public class UpdateRewardDto
{
    public string RewardTitle { get; set; }
    public string WhatYouGet { get; set; }
    public string FinePrint { get; set; }
    public string RewardImageUrl { get; set; }
    public bool? IsGift { get; set; }
    public string RewardType { get; set; }
    public DateTime? ValidityStart { get; set; }
    public DateTime? ValidityEnd { get; set; }
}
