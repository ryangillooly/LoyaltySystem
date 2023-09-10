using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IBusinessRepository
{
    Task<Business> GetByIdAsync(Guid id);
    Task<IEnumerable<Business>> GetAllAsync();
    Task AddAsync(Business business);
    Task UpdateAsync(Business business);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<User>> GetBusinessUsersAsync(Guid businessId);
    Task AddUserToBusinessAsync(Guid businessId, Guid userId);
    Task RemoveUserFromBusinessAsync(Guid businessId, Guid userId);
}