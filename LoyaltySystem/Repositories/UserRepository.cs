using System.Threading.Tasks;
using System.Linq;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using LoyaltySystem.Data;

namespace LoyaltySystem.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<int> AddAsync(User user);
    Task UpdateAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly IConfiguration _config;

    public UserRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.Users
            .Where(u => u.Email.ToLower() == email.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        return await db.Users
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> AddAsync(User user)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // InsertWithIdentityAsync returns the new ID if the DB supports it
        var newId = await db.InsertWithIdentityAsync(user);
        user.UserId = (int)newId;
        return user.UserId;
    }

    public async Task UpdateAsync(User user)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        await db.UpdateAsync(user);
    }
}