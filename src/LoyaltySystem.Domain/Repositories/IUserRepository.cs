using System.Data;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(UserId id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByCustomerIdAsync(CustomerId customerId);
        Task AddAsync(User user, IDbTransaction? transaction = null);
        /// <summary>
        /// Updates an existing user.
        /// </summary>
        Task UpdateAsync(User user, IDbTransaction? transaction = null);
        Task UpdateLastLoginAsync(UserId userId);
        Task AddRoleAsync(UserId userId, RoleType role);
        Task RemoveRoleAsync(UserId userId, RoleType role);
        Task<IEnumerable<RoleType>> GetRolesAsync(UserId userId);
    }
} 