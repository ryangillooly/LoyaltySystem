namespace LoyaltySystem.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using LinqToDB;
using LoyaltySystem.Data;
using System.Linq;

public interface IBusinessRepository
{
    Task<int> CreateBusinessAsync(Business business);
    Task<List<Business>> GetAllBusinessesAsync(int userId);
    Task<Business?> GetBusinessByIdAsync(int businessId);
    Task UpdateBusinessAsync(Business business);
    Task DeleteBusinessAsync(int businessId);

    // Staff (BusinessUser) methods
    Task<List<BusinessUser>> GetStaffAsync(int businessId);
    Task AddStaffAsync(BusinessUser staff);
    Task<BusinessUser?> GetBusinessUserAsync(int businessUserId);
    Task UpdateStaffAsync(BusinessUser staff);
    Task RemoveStaffAsync(int businessUserId);
}

public class BusinessRepository : IBusinessRepository
{
    private readonly IConfiguration _config;

    public BusinessRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> CreateBusinessAsync(Business business)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        business.CreatedAt = System.DateTime.UtcNow;
        business.UpdatedAt = System.DateTime.UtcNow;

        var newId = await db.InsertWithIdentityAsync(business);
        return (int)newId;
    }

    public async Task<List<Business>> GetAllBusinessesAsync(int userId)
    {
        // If you want to filter by businesses the user owns or has access to,
        // you'd join BusinessUser or do your own logic. For now, let's return all.
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // Just returning all for example:
        return await db.Businesses.ToListAsync();
    }

    public async Task<Business?> GetBusinessByIdAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.Businesses
            .Where(b => b.BusinessId == businessId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateBusinessAsync(Business business)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        business.UpdatedAt = System.DateTime.UtcNow;

        await db.UpdateAsync(business);
    }

    public async Task DeleteBusinessAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // Optionally, delete staff records first if you have FKs.
        // e.g. await db.BusinessUsers.Where(bu => bu.BusinessId == businessId).DeleteAsync();

        await db.Businesses
            .Where(b => b.BusinessId == businessId)
            .DeleteAsync();
    }

    // ===================== Staff Methods =====================
    public async Task<List<BusinessUser>> GetStaffAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.BusinessUsers
            .Where(bu => bu.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task AddStaffAsync(BusinessUser staff)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        staff.CreatedAt = System.DateTime.UtcNow;

        await db.InsertAsync(staff);
    }

    public async Task<BusinessUser?> GetBusinessUserAsync(int businessUserId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.BusinessUsers
            .Where(bu => bu.BusinessUserId == businessUserId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateStaffAsync(BusinessUser staff)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.UpdateAsync(staff);
    }

    public async Task RemoveStaffAsync(int businessUserId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.BusinessUsers
            .Where(bu => bu.BusinessUserId == businessUserId)
            .DeleteAsync();
    }
}
