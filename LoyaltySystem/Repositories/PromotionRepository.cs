namespace LoyaltySystem.Repositories;

using Microsoft.Extensions.Configuration;
using LoyaltySystem.Data;
using LinqToDB;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public interface IPromotionsRepository
{
    Task<int> CreatePromotionAsync(Promotion promo);
    Task<List<Promotion>> GetPromotionsByBusinessAsync(int businessId);
    Task<Promotion?> GetPromotionByIdAsync(int businessId, int promotionId);
    Task UpdatePromotionAsync(Promotion promo);
    Task DeletePromotionAsync(int businessId, int promotionId);
}

public class PromotionsRepository : IPromotionsRepository
{
    private readonly IConfiguration _config;

    public PromotionsRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreatePromotionAsync(Promotion promo)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        promo.CreatedAt = DateTime.UtcNow;
        promo.UpdatedAt = DateTime.UtcNow;

        var newId = await db.InsertWithIdentityAsync(promo);
        return (int)newId;
    }

    public async Task<List<Promotion>> GetPromotionsByBusinessAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.Promotions
            .Where(p => p.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task<Promotion?> GetPromotionByIdAsync(int businessId, int promotionId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.Promotions
            .Where(p => p.BusinessId == businessId && p.PromotionId == promotionId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdatePromotionAsync(Promotion promo)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        promo.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(promo);
    }

    public async Task DeletePromotionAsync(int businessId, int promotionId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.Promotions
            .Where(p => p.BusinessId == businessId && p.PromotionId == promotionId)
            .DeleteAsync();
    }
}
