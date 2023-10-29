using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Models;


namespace LoyaltySystem.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetUserAsync(Guid userId);
    Task CreateAsync(User entity, Guid token);
    Task UpdateUserAsync(User updatedUser);
    Task DeleteUserAsync(Guid id);
    Task<List<BusinessUserPermissions>> GetUsersBusinessPermissions(Guid userId);
    Task VerifyEmailAsync(VerifyEmailDto dto);
    Task SendVerificationEmailAsync(string email, Guid userId, Guid token);
}