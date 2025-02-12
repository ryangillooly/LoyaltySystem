namespace LoyaltySystem.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using LoyaltySystem.Data;
using LinqToDB;
using System.Linq;
using System;

public interface ILoyaltyCardRepository
{
    // Templates
    Task<int> CreateTemplateAsync(LoyaltyCardTemplate template);
    Task<List<LoyaltyCardTemplate>> GetTemplatesByBusinessAsync(int businessId);
    Task<LoyaltyCardTemplate?> GetTemplateByIdAsync(int businessId, int templateId);
    Task UpdateTemplateAsync(LoyaltyCardTemplate template);
    Task DeleteTemplateAsync(int businessId, int templateId);

    // User Loyalty Cards
    Task<int> CreateUserCardAsync(UserLoyaltyCard card);
    Task<List<UserLoyaltyCard>> GetUserCardsByBusinessAsync(int businessId);
    Task<UserLoyaltyCard?> GetUserCardByIdAsync(int businessId, int userCardId);
    Task UpdateUserCardAsync(UserLoyaltyCard card);
    Task DeleteUserCardAsync(int businessId, int userCardId);
}

public class LoyaltyCardRepository : ILoyaltyCardRepository
{
    private readonly IConfiguration _config;

    public LoyaltyCardRepository(IConfiguration config)
    {
        _config = config;
    }

    // ================== Templates ==================
    public async Task<int> CreateTemplateAsync(LoyaltyCardTemplate template)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        var newId = await db.InsertWithIdentityAsync(template);
        return (int)newId;
    }

    public async Task<List<LoyaltyCardTemplate>> GetTemplatesByBusinessAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.LoyaltyCardTemplates
            .Where(t => t.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task<LoyaltyCardTemplate?> GetTemplateByIdAsync(int businessId, int templateId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.LoyaltyCardTemplates
            .Where(t => t.BusinessId == businessId && t.LoyaltyCardTemplateId == templateId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateTemplateAsync(LoyaltyCardTemplate template)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        template.UpdatedAt = DateTime.UtcNow;

        await db.UpdateAsync(template);
    }

    public async Task DeleteTemplateAsync(int businessId, int templateId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.LoyaltyCardTemplates
            .Where(t => t.BusinessId == businessId && t.LoyaltyCardTemplateId == templateId)
            .DeleteAsync();
    }

    // ================== User Loyalty Cards ==================
    public async Task<int> CreateUserCardAsync(UserLoyaltyCard card)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        card.CreatedAt = DateTime.UtcNow;
        card.UpdatedAt = DateTime.UtcNow;

        var newId = await db.InsertWithIdentityAsync(card);
        return (int)newId;
    }

    public async Task<List<UserLoyaltyCard>> GetUserCardsByBusinessAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.UserLoyaltyCards
            .Where(c => c.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task<UserLoyaltyCard?> GetUserCardByIdAsync(int businessId, int userCardId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.UserLoyaltyCards
            .Where(c => c.BusinessId == businessId && c.UserLoyaltyCardId == userCardId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateUserCardAsync(UserLoyaltyCard card)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        card.UpdatedAt = DateTime.UtcNow;

        await db.UpdateAsync(card);
    }

    public async Task DeleteUserCardAsync(int businessId, int userCardId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.UserLoyaltyCards
            .Where(c => c.BusinessId == businessId && c.UserLoyaltyCardId == userCardId)
            .DeleteAsync();
    }
}
