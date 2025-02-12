namespace LoyaltySystem.Repositories;

using Microsoft.Extensions.Configuration;
using LoyaltySystem.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using LinqToDB;
using System.Linq;
using System;

public interface IStampsRepository
{
    Task<int> CreateStampAsync(StampTransaction stamp);
    Task<StampTransaction?> GetStampTransactionAsync(int businessId, int stampTransactionId);
    Task<List<StampTransaction>> GetStampsByBusinessAsync(int businessId);
    Task UpdateStampTransactionAsync(StampTransaction stamp);
    Task DeleteStampTransactionAsync(int businessId, int stampTransactionId);
}

public class StampsRepository : IStampsRepository
{
    private readonly IConfiguration _config;

    public StampsRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateStampAsync(StampTransaction stamp)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        stamp.Timestamp = DateTime.UtcNow; // Or set externally
        var newId = await db.InsertWithIdentityAsync(stamp);
        return (int)newId;
    }

    public async Task<StampTransaction?> GetStampTransactionAsync(int businessId, int stampTransactionId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.StampTransactions
            .Where(s => s.BusinessId == businessId && s.StampTransactionId == stampTransactionId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<StampTransaction>> GetStampsByBusinessAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.StampTransactions
            .Where(s => s.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task UpdateStampTransactionAsync(StampTransaction stamp)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.UpdateAsync(stamp);
    }

    public async Task DeleteStampTransactionAsync(int businessId, int stampTransactionId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.StampTransactions
            .Where(s => s.BusinessId == businessId && s.StampTransactionId == stampTransactionId)
            .DeleteAsync();
    }
}
