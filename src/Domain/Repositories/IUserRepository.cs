using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories
{
    /// <summary>
    /// Repository interface for User entities.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        Task<User> GetByIdAsync(UserId id);
        
        /// <summary>
        /// Gets a user by username.
        /// </summary>
        Task<User> GetByUsernameAsync(string username);
        
        /// <summary>
        /// Gets a user by email.
        /// </summary>
        Task<User> GetByEmailAsync(string email);
        
        /// <summary>
        /// Gets users by role.
        /// </summary>
        Task<IEnumerable<User>> GetByRoleAsync(RoleType role, int skip, int take);
        
        /// <summary>
        /// Gets a user by customer ID.
        /// </summary>
        Task<User> GetByCustomerIdAsync(CustomerId customerId);
        
        /// <summary>
        /// Adds a new user.
        /// </summary>
        Task AddAsync(User user);
        
        /// <summary>
        /// Updates an existing user.
        /// </summary>
        Task UpdateAsync(User user);
        
        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        Task AddRoleAsync(UserId userId, RoleType role);
        
        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        Task RemoveRoleAsync(UserId userId, RoleType role);
        
        /// <summary>
        /// Gets all roles for a user.
        /// </summary>
        Task<IEnumerable<RoleType>> GetRolesAsync(UserId userId);
    }
} 