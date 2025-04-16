using System.Data;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByCustomerIdAsync(string customerId);
        Task AddAsync(User user, IDbTransaction? transaction = null);
        Task UpdateAsync(User user);
        Task UpdateLastLoginAsync(UserId userId);
        Task AddRoleAsync(string userId, RoleType role);
        Task RemoveRoleAsync(string userId, RoleType role);
    }
} 