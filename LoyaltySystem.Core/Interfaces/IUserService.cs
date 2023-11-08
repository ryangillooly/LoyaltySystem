using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetUserAsync(Guid userId);
    Task<User> CreateAsync(User newUser);
    Task DeleteUserAsync(Guid userId);
    Task<User> UpdateUserAsync(User updatedUser);
    Task<List<BusinessUserPermissions>> GetUsersBusinessPermissions(Guid businessId);
    Task VerifyEmailAsync(VerifyUserEmailDto dto);
}