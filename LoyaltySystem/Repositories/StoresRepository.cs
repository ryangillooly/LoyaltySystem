namespace LoyaltySystem.Repositories;

using LoyaltySystem.Data;
using Microsoft.Extensions.Configuration;
using LinqToDB;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

public interface IStoresRepository
{
    Task<int> CreateStoreAsync(Store store);
    Task<Store> GetStoreByIdAsync(int businessId, int storeId);
    Task<List<Store>> GetStoresByBusinessAsync(int businessId);
    Task UpdateStoreAsync(Store store);
    Task DeleteStoreAsync(int businessId, int storeId);
}

public class StoresRepository : IStoresRepository
{
    private readonly IConfiguration _config;

    public StoresRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateStoreAsync(Store store)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        store.CreatedAt = DateTime.UtcNow;
        store.UpdatedAt = DateTime.UtcNow;
        var newId = await db.InsertWithIdentityAsync(store);
        return (int)newId;
    }

    public async Task<Store> GetStoreByIdAsync(int businessId, int storeId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        return await db.Stores
            .Where(s => s.BusinessId == businessId && s.StoreId == storeId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Store>> GetStoresByBusinessAsync(int businessId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        return await db.Stores
            .Where(s => s.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task UpdateStoreAsync(Store store)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        store.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(store);
    }

    public async Task DeleteStoreAsync(int businessId, int storeId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        await db.Stores
            .Where(s => s.BusinessId == businessId && s.StoreId == storeId)
            .DeleteAsync();
    }
}
