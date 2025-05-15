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
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByCustomerIdAsync(CustomerId customerId);
        Task<User?> GetByEmailConfirmationTokenAsync(string token);
        Task<IEnumerable<RoleType>> GetRolesAsync(UserId userId);
        
        Task AddAsync(User user, IDbTransaction? transaction = null);
        
        Task UpdateAsync(User user, IDbTransaction? transaction = null);
        Task UpdatePasswordAsync(UserId userId, string newPasswordHash);
        
        Task UpdateLastLoginAsync(UserId userId);
        Task AddRoleAsync(UserId userId, List<RoleType> roles);
        
        Task RemoveRoleAsync(UserId userId, List<RoleType> role);
    }
} 