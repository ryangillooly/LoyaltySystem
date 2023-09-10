using LoyaltySystem.Core.Models;


namespace LoyaltySystem.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetByIdAsync(Guid id);
    Task<User> AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(Guid id);
    Task<bool> ValidateAsync(UserLoginDTO userLoginDto);
}