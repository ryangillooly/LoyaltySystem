namespace LoyaltySystem.Repositories;

using LoyaltySystem.Data;
using Microsoft.Extensions.Configuration;
using LinqToDB;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public interface IMembersRepository
{
    Task<int> AddMemberAsync(Member member);
    Task<Member> GetMemberByIdAsync(int businessId, int memberId);
    Task<List<Member>> GetMembersByBusinessAsync(int businessId);
    Task UpdateMemberAsync(Member member);
    Task DeleteMemberAsync(int businessId, int memberId);
}

public class MembersRepository : IMembersRepository
{
    private readonly IConfiguration _config;

    public MembersRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> AddMemberAsync(Member member)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        // member.JoinedAt = DateTime.UtcNow; // if needed
        var newId = await db.InsertWithIdentityAsync(member);
        return (int)newId;
    }

    public async Task<Member> GetMemberByIdAsync(int businessId, int memberId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        return await db.Members
            .Where(m => m.BusinessId == businessId && m.MemberId == memberId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Member>> GetMembersByBusinessAsync(int businessId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        return await db.Members
            .Where(m => m.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task UpdateMemberAsync(Member member)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        await db.UpdateAsync(member);
    }

    public async Task DeleteMemberAsync(int businessId, int memberId)
    {
        using var db = new Linq2DbConnection(_config.GetConnectionString("DefaultConnection"));
        await db.Members
            .Where(m => m.BusinessId == businessId && m.MemberId == memberId)
            .DeleteAsync();
    }
}
