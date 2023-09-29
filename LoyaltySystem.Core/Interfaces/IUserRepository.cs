using LoyaltySystem.Core.Models;


namespace LoyaltySystem.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetByIdAsync(Guid id);
    Task CreateAsync(User entity);
    Task UpdateUserAsync(User updatedUser);
    Task DeleteUserAsync(Guid id);
   
}