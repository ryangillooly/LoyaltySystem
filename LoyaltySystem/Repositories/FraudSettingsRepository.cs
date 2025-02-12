namespace LoyaltySystem.Repositories;

using LoyaltySystem.Data;
using Microsoft.Extensions.Configuration;
using LinqToDB;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

public interface IFraudSettingsRepository
{
    Task<int> CreateFraudSettingsAsync(FraudSettings fs);
    Task<FraudSettings> GetByStoreIdAsync(int storeId);
    Task UpdateFraudSettingsAsync(FraudSettings fs);
    Task DeleteFraudSettingsAsync(int storeId);
}

public class FraudSettingsRepository : IFraudSettingsRepository
{
    private readonly IConfiguration _config;

    public FraudSettingsRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateFraudSettingsAsync(FraudSettings fs)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        fs.CreatedAt = DateTime.UtcNow;
        fs.UpdatedAt = DateTime.UtcNow;
        var newId = await db.InsertWithIdentityAsync(fs);
        return (int)newId;
    }

    public async Task<FraudSettings> GetByStoreIdAsync(int storeId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        return await db.FraudSettings
            .Where(x => x.StoreId == storeId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateFraudSettingsAsync(FraudSettings fs)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        fs.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(fs);
    }

    public async Task DeleteFraudSettingsAsync(int storeId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        await db.FraudSettings
            .Where(x => x.StoreId == storeId)
            .DeleteAsync();
    }
}