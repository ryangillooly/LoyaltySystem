using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Data.Repositories;

public class BusinessRepository : IRepository<Business>
{
   /* private readonly YourDbContext _context;

    public BusinessRepository(YourDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Business>> GetAllAsync()
    {
        return await _context.Business.ToListAsync();
    }

    public async Task<Business> GetByIdAsync(int id)
    {
        return await _context.Business.FindAsync(id);
    }

    public async Task AddAsync(Business entity)
    {
        await _context.Business.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Business entity)
    {
        _context.Business.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        _context.Business.Remove(entity);
        await _context.SaveChangesAsync();
    }
    */
   
   public Task<IEnumerable<Business>> GetAllAsync()
   {
       throw new NotImplementedException();
   }

   public Task<Business> GetByIdAsync(Guid id)
   {
       throw new NotImplementedException();
   }

   public Task<Business> AddAsync(Business entity)
   {
       throw new NotImplementedException();
   }

   public Task UpdateAsync(Business entity)
   {
       throw new NotImplementedException();
   }

   public Task DeleteAsync(Guid id)
   {
       throw new NotImplementedException();
   }
}