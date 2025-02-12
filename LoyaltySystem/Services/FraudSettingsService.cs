namespace LoyaltySystem.Services;

using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System.Threading.Tasks;
using System;

public interface IFraudSettingsService
{
    Task<int> CreateAsync(CreateFraudSettingsDto dto);
    Task<FraudSettings> GetByStoreAsync(int storeId);
    Task UpdateAsync(int storeId, UpdateFraudSettingsDto dto);
    Task DeleteAsync(int storeId);
}

public class FraudSettingsService : IFraudSettingsService
{
    private readonly IFraudSettingsRepository _repo;

    public FraudSettingsService(IFraudSettingsRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> CreateAsync(CreateFraudSettingsDto dto)
    {
        var fs = new FraudSettings
        {
            StoreId = dto.StoreId,
            AllowedLatitude = dto.AllowedLatitude,
            AllowedLongitude = dto.AllowedLongitude,
            AllowedRadius = dto.AllowedRadius,
            OperationalStart = dto.OperationalStart,
            OperationalEnd = dto.OperationalEnd,
            MinIntervalMinutes = dto.MinIntervalMinutes
        };
        return await _repo.CreateFraudSettingsAsync(fs);
    }

    public async Task<FraudSettings> GetByStoreAsync(int storeId)
    {
        return await _repo.GetByStoreIdAsync(storeId);
    }

    public async Task UpdateAsync(int storeId, UpdateFraudSettingsDto dto)
    {
        var existing = await _repo.GetByStoreIdAsync(storeId);
        if (existing == null) throw new Exception("FraudSettings not found for store " + storeId);

        existing.AllowedLatitude = dto.AllowedLatitude ?? existing.AllowedLatitude;
        existing.AllowedLongitude = dto.AllowedLongitude ?? existing.AllowedLongitude;
        existing.AllowedRadius = dto.AllowedRadius ?? existing.AllowedRadius;
        existing.OperationalStart = dto.OperationalStart ?? existing.OperationalStart;
        existing.OperationalEnd = dto.OperationalEnd ?? existing.OperationalEnd;
        existing.MinIntervalMinutes = dto.MinIntervalMinutes ?? existing.MinIntervalMinutes;

        await _repo.UpdateFraudSettingsAsync(existing);
    }

    public async Task DeleteAsync(int storeId)
    {
        await _repo.DeleteFraudSettingsAsync(storeId);
    }
}

// Dto classes:
public class CreateFraudSettingsDto
{
    public int StoreId { get; set; }
    public decimal AllowedLatitude { get; set; }
    public decimal AllowedLongitude { get; set; }
    public int AllowedRadius { get; set; }
    public TimeSpan OperationalStart { get; set; }
    public TimeSpan OperationalEnd { get; set; }
    public int MinIntervalMinutes { get; set; } = 240;
}

public class UpdateFraudSettingsDto
{
    public decimal? AllowedLatitude { get; set; }
    public decimal? AllowedLongitude { get; set; }
    public int? AllowedRadius { get; set; }
    public TimeSpan? OperationalStart { get; set; }
    public TimeSpan? OperationalEnd { get; set; }
    public int? MinIntervalMinutes { get; set; }
}
