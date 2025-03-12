using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;

namespace LoyaltySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for User entities using Dapper.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        public UserRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            
            // Register type handlers for our custom ID types
            SqlMapper.AddTypeHandler(new EntityIdTypeHandler<UserId>());
            SqlMapper.AddTypeHandler(new EntityIdTypeHandler<CustomerId>());
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        public async Task<User?> GetByIdAsync(UserId id)
        {
            const string sql = @"
                SELECT 
                    u.id AS Id, u.username AS Username, u.email AS Email, 
                    u.password_hash AS PasswordHash, u.password_salt AS PasswordSalt, 
                    u.customer_id AS CustomerId, u.status::int AS Status, 
                    u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt
                FROM users u
                WHERE u.id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id.Value });
            
            if (user != null)
            {
                // Load roles
                await LoadRolesAsync(user);
            }
            
            return user;
        }

        /// <summary>
        /// Gets a user by username.
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = @"
                SELECT 
                    u.id AS Id, 
                    u.username AS Username, 
                    u.email AS Email, 
                    u.password_hash AS PasswordHash, 
                    u.password_salt AS PasswordSalt, 
                    u.customer_id AS CustomerId, 
                    CAST(u.status AS INT) AS Status, 
                    u.created_at AS CreatedAt, 
                    u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt
                FROM users u
                WHERE u.username = @Username";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
            
            if (user != null)
            {
                await LoadRolesAsync(user);
            }
            
            return user;
        }

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = @"
                SELECT 
                    u.id AS Id, u.username AS Username, u.email AS Email, 
                    u.password_hash AS PasswordHash, u.password_salt AS PasswordSalt, 
                    u.customer_id AS CustomerId, u.status::int AS Status, 
                    u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt
                FROM users u
                WHERE u.email = @Email";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            
            if (user != null)
            {
                await LoadRolesAsync(user);
            }
            
            return user;
        }

        /// <summary>
        /// Gets users by role.
        /// </summary>
        public async Task<IEnumerable<User>> GetByRoleAsync(RoleType role, int skip, int take)
        {
            const string sql = @"
                SELECT 
                    u.id AS Id, u.username AS Username, u.email AS Email, 
                    u.password_hash AS PasswordHash, u.password_salt AS PasswordSalt, 
                    u.customer_id AS CustomerId, u.status::int AS Status, 
                    u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt
                FROM users u
                JOIN user_roles ur ON u.id = ur.user_id
                WHERE ur.role = @Role
                ORDER BY u.created_at DESC
                LIMIT @Take OFFSET @Skip";

            var connection = await _dbConnection.GetConnectionAsync();
            var users = await connection.QueryAsync<User>(sql, new 
            { 
                Role = role.ToString(),
                Skip = skip,
                Take = take
            });
            
            var usersList = users.ToList();
            foreach (var user in usersList)
            {
                await LoadRolesAsync(user);
            }
            
            return usersList;
        }

        /// <summary>
        /// Gets a user by customer ID.
        /// </summary>
        public async Task<User?> GetByCustomerIdAsync(CustomerId customerId)
        {
            const string sql = @"
                SELECT 
                    u.id AS Id, u.username AS Username, u.email AS Email, 
                    u.password_hash AS PasswordHash, u.password_salt AS PasswordSalt, 
                    u.customer_id AS CustomerId, u.status::int AS Status, 
                    u.created_at AS CreatedAt, u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt
                FROM users u
                WHERE u.customer_id = @CustomerId";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { CustomerId = customerId.Value });
            
            if (user != null)
            {
                await LoadRolesAsync(user);
            }
            
            return user;
        }

        /// <summary>
        /// Adds a new user.
        /// </summary>
        public async Task AddAsync(User user)
        {
            const string sql = @"
                INSERT INTO users (
                    id, username, email, password_hash, password_salt, 
                    customer_id, status, created_at, updated_at, last_login_at
                ) VALUES (
                    @Id, @Username, @Email, @PasswordHash, @PasswordSalt, 
                    @CustomerId, @Status, @CreatedAt, @UpdatedAt, @LastLoginAt
                )";

            var connection = await _dbConnection.GetConnectionAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        Id = user.Id.Value,
                        user.Username,
                        user.Email,
                        user.PasswordHash,
                        user.PasswordSalt,
                        CustomerId = user.CustomerId?.Value,
                        Status = user.Status.ToString(),
                        user.CreatedAt,
                        user.UpdatedAt,
                        user.LastLoginAt
                    }, transaction);
                    
                    // Add roles
                    foreach (var userRole in user.Roles)
                    {
                        await AddRoleInternalAsync(user.Id, userRole.Role, transaction);
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        public async Task UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE users
                SET username = @Username,
                    email = @Email,
                    password_hash = @PasswordHash,
                    password_salt = @PasswordSalt,
                    customer_id = @CustomerId,
                    status = @Status,
                    updated_at = @UpdatedAt,
                    last_login_at = @LastLoginAt
                WHERE id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                Id = user.Id.Value,
                user.Username,
                user.Email,
                user.PasswordHash,
                user.PasswordSalt,
                CustomerId = user.CustomerId?.Value,
                Status = (int) user.Status,
                user.UpdatedAt,
                user.LastLoginAt
            });
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        public async Task AddRoleAsync(UserId userId, RoleType role)
        {
            var connection = await _dbConnection.GetConnectionAsync();
            await AddRoleInternalAsync(userId, role, null);
        }

        private async Task AddRoleInternalAsync(UserId userId, RoleType role, IDbTransaction? transaction = null)
        {
            const string sql = @"
                INSERT INTO user_roles (user_id, role, created_at)
                VALUES (@UserId, @Role, @CreatedAt)
                ON CONFLICT (user_id, role) DO NOTHING";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId.Value,
                Role = role.ToString(),
                CreatedAt = DateTime.UtcNow
            }, transaction);
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        public async Task RemoveRoleAsync(UserId userId, RoleType role)
        {
            const string sql = @"
                DELETE FROM user_roles
                WHERE user_id = @UserId AND role = @Role";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId.Value,
                Role = role.ToString()
            });
        }

        /// <summary>
        /// Gets all roles for a user.
        /// </summary>
        public async Task<IEnumerable<RoleType>> GetRolesAsync(UserId userId)
        {
            const string sql = @"
                SELECT role
                FROM user_roles
                WHERE user_id = @UserId";

            var connection = await _dbConnection.GetConnectionAsync();
            var roleStrings = await connection.QueryAsync<string>(sql, new { UserId = userId.Value });
            
            var roles = new List<RoleType>();
            foreach (var roleString in roleStrings)
            {
                if (Enum.TryParse<RoleType>(roleString, out var roleType))
                {
                    roles.Add(roleType);
                }
            }
            
            return roles;
        }

        private async Task LoadRolesAsync(User user)
        {
            var roles = await GetRolesAsync(user.Id);
            foreach (var role in roles)
            {
                user.AddRole(role);
            }
        }
    }
} 