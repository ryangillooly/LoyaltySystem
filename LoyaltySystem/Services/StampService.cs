namespace LoyaltySystem.Services;

using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public interface IStampsService
{
    Task<int> CreateStampAsync(int businessId, CreateStampDto dto);
    Task<StampTransaction?> GetStampTransactionAsync(int businessId, int stampTransactionId);
    Task<List<StampTransaction>> GetStampsAsync(int businessId);
    Task UpdateStampTransactionAsync(int businessId, int stampTransactionId, UpdateStampDto dto);
    Task DeleteStampTransactionAsync(int businessId, int stampTransactionId);
}

public class StampsService : IStampsService
{
    private readonly IStampsRepository _repo;
    // Potentially also inject IUserLoyaltyCardRepository if you want to increment stamps

    public StampsService(IStampsRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> CreateStampAsync(int businessId, CreateStampDto dto)
    {
        // Possibly check frequency (fraud prevention), user card status, etc.
        // And increment user’s loyalty card stamp count if needed

        var stamp = new StampTransaction
        {
            UserLoyaltyCardId = dto.UserLoyaltyCardId,
            StoreId = dto.StoreId,
            BusinessId = businessId,
            GeoLocation = dto.GeoLocation,
            Timestamp = DateTime.UtcNow
        };

        // E.g. update user’s loyalty card stamp count if needed
        // ...
        return await _repo.CreateStampAsync(stamp);
    }

    public async Task<StampTransaction?> GetStampTransactionAsync(int businessId, int stampTransactionId)
    {
        return await _repo.GetStampTransactionAsync(businessId, stampTransactionId);
    }

    public async Task<List<StampTransaction>> GetStampsAsync(int businessId)
    {
        return await _repo.GetStampsByBusinessAsync(businessId);
    }

    public async Task UpdateStampTransactionAsync(int businessId, int stampTransactionId, UpdateStampDto dto)
    {
        var existing = await _repo.GetStampTransactionAsync(businessId, stampTransactionId);
        if (existing == null)
        {
            throw new Exception("Stamp transaction not found");
        }

        if (dto.StoreId.HasValue) existing.StoreId = dto.StoreId.Value;
        if (!string.IsNullOrEmpty(dto.GeoLocation)) existing.GeoLocation = dto.GeoLocation;
        if (dto.UserLoyaltyCardId.HasValue) existing.UserLoyaltyCardId = dto.UserLoyaltyCardId.Value;

        // Re-check business logic if store changed, etc.
        await _repo.UpdateStampTransactionAsync(existing);
    }

    public async Task DeleteStampTransactionAsync(int businessId, int stampTransactionId)
    {
        // Possibly decrement stamp count from user’s loyalty card if logic requires
        await _repo.DeleteStampTransactionAsync(businessId, stampTransactionId);
    }
}

// =========== DTOs ===========
public class CreateStampDto
{
    public int UserLoyaltyCardId { get; set; }
    public int StoreId { get; set; }
    public string GeoLocation { get; set; }
}

public class UpdateStampDto
{
    public int? UserLoyaltyCardId { get; set; }
    public int? StoreId { get; set; }
    public string GeoLocation { get; set; }
}
