namespace LoyaltySystem.Repositories;

using Microsoft.Extensions.Configuration;
using LoyaltySystem.Data;
using System.Threading.Tasks;
using LinqToDB;
using System.Linq;
using System.Collections.Generic;
using System;

public interface IRedemptionRepository
{
    Task<int> CreateRedemptionAsync(RedemptionTransaction redemption);
    Task<RedemptionTransaction?> GetRedemptionAsync(int businessId, int redemptionId);
    Task<List<RedemptionTransaction>> GetRedemptionsByBusinessAsync(int businessId);
    Task UpdateRedemptionAsync(RedemptionTransaction redemption);
    Task DeleteRedemptionAsync(int businessId, int redemptionId);
}

public class RedemptionRepository : IRedemptionRepository
{
    private readonly IConfiguration _config;

    public RedemptionRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateRedemptionAsync(RedemptionTransaction redemption)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        redemption.Timestamp = DateTime.UtcNow; // or set externally
        var newId = await db.InsertWithIdentityAsync(redemption);
        return (int)newId;
    }

    public async Task<RedemptionTransaction?> GetRedemptionAsync(int businessId, int redemptionId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.RedemptionTransactions
            .Where(r => r.BusinessId == businessId && r.RedemptionTransactionId == redemptionId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<RedemptionTransaction>> GetRedemptionsByBusinessAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.RedemptionTransactions
            .Where(r => r.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task UpdateRedemptionAsync(RedemptionTransaction redemption)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // If you'd like to keep a separate updated timestamp, you'd handle it here
        await db.UpdateAsync(redemption);
    }

    public async Task DeleteRedemptionAsync(int businessId, int redemptionId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.RedemptionTransactions
            .Where(r => r.BusinessId == businessId && r.RedemptionTransactionId == redemptionId)
            .DeleteAsync();
    }
}
