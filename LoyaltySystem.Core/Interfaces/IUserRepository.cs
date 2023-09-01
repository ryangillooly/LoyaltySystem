using LoyaltySystem.Core.Models;


namespace LoyaltySystem.Core.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateUserAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}