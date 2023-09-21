using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User newUser);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(User user);
    Task UpdatePermissionsAsync(List<UserPermission> permissions);
}