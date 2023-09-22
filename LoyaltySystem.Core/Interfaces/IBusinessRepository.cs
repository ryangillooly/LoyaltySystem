using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessRepository
{
    Task<IEnumerable<Business>> GetAllAsync();
    Task<Business> GetByIdAsync(Guid id);
    Task<Business> CreateBusinessAsync(Business entity);
    Task UpdateAsync(Business entity);
    Task DeleteAsync(Guid id);
    Task UpdatePermissionsAsync(List<Permission> permissions);
}