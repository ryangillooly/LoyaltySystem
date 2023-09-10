using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Data.Repositories;

public class LoyaltyCardRepository : IRepository<LoyaltyCard>
{
    /*private readonly YourDbContext _context;

    public LoyaltyCardRepository(YourDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LoyaltyCard>> GetAllAsync()
    {
        return await _context.LoyaltyCards.ToListAsync();
    }

    public async Task<LoyaltyCard> GetByIdAsync(int id)
    {
        return await _context.LoyaltyCards.FindAsync(id);
    }

    public async Task AddAsync(LoyaltyCard entity)
    {
        await _context.LoyaltyCards.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LoyaltyCard entity)
    {
        _context.LoyaltyCards.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        _context.LoyaltyCards.Remove(entity);
        await _context.SaveChangesAsync();
    }
    */

    public Task<IEnumerable<LoyaltyCard>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<LoyaltyCard> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<LoyaltyCard> AddAsync(LoyaltyCard entity)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(LoyaltyCard entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}