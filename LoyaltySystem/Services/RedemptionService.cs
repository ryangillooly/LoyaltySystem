namespace LoyaltySystem.Services;

using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public interface IRedemptionService
{
    Task<int> CreateRedemptionAsync(int businessId, CreateRedemptionDto dto);
    Task<RedemptionTransaction?> GetRedemptionAsync(int businessId, int redemptionId);
    Task<List<RedemptionTransaction>> GetRedemptionsAsync(int businessId);
    Task UpdateRedemptionAsync(int businessId, int redemptionId, UpdateRedemptionDto dto);
    Task DeleteRedemptionAsync(int businessId, int redemptionId);
}

public class RedemptionService : IRedemptionService
{
    private readonly IRedemptionRepository _repo;

    public RedemptionService(IRedemptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> CreateRedemptionAsync(int businessId, CreateRedemptionDto dto)
    {
        // In real logic, you might check if UserLoyaltyCard is valid, if reward is valid, etc.
        // For now, we just create a record.
        var redemption = new RedemptionTransaction
        {
            UserLoyaltyCardId = dto.UserLoyaltyCardId,
            RewardId = dto.RewardId,
            StoreId = dto.StoreId,
            BusinessId = businessId,
            Timestamp = DateTime.UtcNow
        };

        return await _repo.CreateRedemptionAsync(redemption);
    }

    public async Task<RedemptionTransaction?> GetRedemptionAsync(int businessId, int redemptionId)
    {
        return await _repo.GetRedemptionAsync(businessId, redemptionId);
    }

    public async Task<List<RedemptionTransaction>> GetRedemptionsAsync(int businessId)
    {
        return await _repo.GetRedemptionsByBusinessAsync(businessId);
    }

    public async Task UpdateRedemptionAsync(int businessId, int redemptionId, UpdateRedemptionDto dto)
    {
        var existing = await _repo.GetRedemptionAsync(businessId, redemptionId);
        if (existing == null)
        {
            throw new Exception("Redemption not found");
        }

        // Possibly update store or reward if your logic allows
        if (dto.StoreId.HasValue) existing.StoreId = dto.StoreId.Value;
        if (dto.RewardId.HasValue) existing.RewardId = dto.RewardId.Value;
        if (dto.UserLoyaltyCardId.HasValue) existing.UserLoyaltyCardId = dto.UserLoyaltyCardId.Value;

        await _repo.UpdateRedemptionAsync(existing);
    }

    public async Task DeleteRedemptionAsync(int businessId, int redemptionId)
    {
        await _repo.DeleteRedemptionAsync(businessId, redemptionId);
    }
}

// =========== DTOs ===========

public class CreateRedemptionDto
{
    public int UserLoyaltyCardId { get; set; }
    public int RewardId { get; set; }
    public int StoreId { get; set; }
}

public class UpdateRedemptionDto
{
    public int? UserLoyaltyCardId { get; set; }
    public int? RewardId { get; set; }
    public int? StoreId { get; set; }
}
