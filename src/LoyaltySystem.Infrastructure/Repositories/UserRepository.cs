using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;
using LoyaltySystem.Infrastructure.Data.TypeHandlers;

namespace LoyaltySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for User entities using Dapper.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection _dbConnection;
        
        static UserRepository()
        {
            // Initialize type handlers from centralized configuration
            TypeHandlerConfig.Initialize();
        }

        public UserRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        public async Task<User?> GetByIdAsync(string id)
        {
            // Handle prefixed UserId
            string rawUserId = id;
            if (UserId.TryParse<UserId>(id, out var userIdObj))
            {
                rawUserId = userIdObj.Value.ToString();
            }
            else if (!Guid.TryParse(id, out _))
            {
                // Log warning for debugging
                Console.WriteLine($"Warning: Invalid user ID format in GetByIdAsync: {id}");
            }
            
            const string sql = @"
                SELECT 
                    u.id AS ""Id"", 
                    u.username AS ""Username"", 
                    u.email AS ""Email"", 
                    u.password_hash AS ""PasswordHash"", 
                    u.password_salt AS ""PasswordSalt"", 
                    u.customer_id AS ""CustomerId"", 
                    COALESCE(CAST(u.status AS INT), 1) AS ""Status"", 
                    u.created_at AS ""CreatedAt"", 
                    u.updated_at AS ""UpdatedAt"", 
                    u.last_login_at AS ""LastLoginAt""
                FROM users u
                WHERE u.id = @Id::uuid";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = rawUserId });
            
            if (user != null)
            {
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
                    u.id AS ""Id"", 
                    u.username AS ""Username"", 
                    u.email AS ""Email"", 
                    u.password_hash AS ""PasswordHash"", 
                    u.password_salt AS ""PasswordSalt"", 
                    u.customer_id AS ""CustomerId"", 
                    COALESCE(CAST(u.status AS INT), 1) AS ""Status"", 
                    u.created_at AS ""CreatedAt"", 
                    u.updated_at AS ""UpdatedAt"", 
                    u.last_login_at AS ""LastLoginAt""
                FROM users u
                WHERE u.username = @Username";

            var connection = await _dbConnection.GetConnectionAsync();
            
            try
            {
                // Query for the user with our custom mappers
                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    sql,
                    new { Username = username },
                    commandType: CommandType.Text);
                
                if (user != null)
                {
                    // Load roles after fetching user
                    await LoadRolesAsync(user);
                }
                
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByUsernameAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = @"
                SELECT 
                    u.id AS ""Id"", 
                    u.username AS ""Username"", 
                    u.email AS ""Email"", 
                    u.password_hash AS ""PasswordHash"", 
                    u.password_salt AS ""PasswordSalt"", 
                    u.customer_id AS ""CustomerId"", 
                    COALESCE(CAST(u.status AS INT), 1) AS ""Status"", 
                    u.created_at AS ""CreatedAt"", 
                    u.updated_at AS ""UpdatedAt"", 
                    u.last_login_at AS ""LastLoginAt""
                FROM users u
                WHERE u.email = @Email";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
            
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
                    u.id AS ""Id"", 
                    u.username AS ""Username"", 
                    u.email AS ""Email"", 
                    u.password_hash AS ""PasswordHash"", 
                    u.password_salt AS ""PasswordSalt"", 
                    u.customer_id AS ""CustomerId"", 
                    COALESCE(CAST(u.status AS INT), 1) AS ""Status"", 
                    u.created_at AS ""CreatedAt"", 
                    u.updated_at AS ""UpdatedAt"", 
                    u.last_login_at AS ""LastLoginAt""
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
        public async Task<User?> GetByCustomerIdAsync(string customerId)
        {
            // Handle prefixed CustomerId
            string rawCustomerId = customerId;
            if (CustomerId.TryParse<CustomerId>(customerId, out var customerIdObj))
            {
                rawCustomerId = customerIdObj.Value.ToString();
            }
            
            const string sql = @"
                SELECT 
                    u.id AS ""Id"", 
                    u.username AS ""Username"", 
                    u.email AS ""Email"", 
                    u.password_hash AS ""PasswordHash"", 
                    u.password_salt AS ""PasswordSalt"", 
                    u.customer_id AS ""CustomerId"", 
                    COALESCE(CAST(u.status AS INT), 1) AS ""Status"", 
                    u.created_at AS ""CreatedAt"", 
                    u.updated_at AS ""UpdatedAt"", 
                    u.last_login_at AS ""LastLoginAt""
                FROM users u
                WHERE u.customer_id = @CustomerId::uuid";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { CustomerId = rawCustomerId });
            
            if (user != null)
            {
                await LoadRolesAsync(user);
            }
            
            return user;
        }

        /// <summary>
        /// Adds a new user.
        /// </summary>
        public async Task AddAsync(User user, IDbTransaction? externalTransaction = null)
        {
            const string sql = @"
                INSERT INTO users (
                    id, first_name, last_name, username, email, password_hash, password_salt, 
                    customer_id, status, created_at, updated_at, last_login_at
                ) VALUES (
                    @Id, @FirstName, @LastName, @Username, @Email, @PasswordHash, @PasswordSalt, 
                    @CustomerId::uuid, @Status, @CreatedAt, @UpdatedAt, @LastLoginAt
                )";

            var connection = await _dbConnection.GetConnectionAsync();
            bool useExternalTransaction = externalTransaction != null;
            var transaction = externalTransaction;
            
            try
            {
                // Only begin a new transaction if no external transaction is provided
                if (!useExternalTransaction)
                    transaction = await connection.BeginTransactionAsync();
                
                await connection.ExecuteAsync(sql, new
                {
                    Id = user.Id.Value,
                    user.FirstName,
                    user.LastName,
                    user.UserName,
                    user.Email,
                    user.PasswordHash,
                    user.PasswordSalt,
                    CustomerId = user.CustomerId?.Value ?? null,
                    Status = (int)user.Status,
                    user.CreatedAt,
                    user.UpdatedAt,
                    user.LastLoginAt
                }, transaction);
                
                // Add roles
                foreach (var userRole in user.Roles)
                {
                    await AddRoleInternalAsync(user.Id.Value.ToString(), userRole.Role, transaction);
                }
                
                if (!useExternalTransaction)
                    transaction.Commit();
            }
            catch
            {
                // Only rollback if we created the transaction
                if (!useExternalTransaction && transaction != null)
                {
                    transaction.Rollback();
                }
                throw;
            }
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        public async Task UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE users
                SET first_name = @FirstName,
                    last_name = @LastName,
                    username = @UserName,
                    email = @Email,
                    password_hash = @PasswordHash,
                    password_salt = @PasswordSalt,
                    customer_id = @CustomerId::uuid,
                    status = @Status,
                    updated_at = @UpdatedAt,
                    last_login_at = @LastLoginAt
                WHERE id = @Id";
            
            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                Id = user.Id.Value,
                user.FirstName,
                user.LastName,
                user.UserName,
                user.Email,
                user.PasswordHash,
                user.PasswordSalt,
                CustomerId = user.CustomerId?.Value ?? null,
                Status = (int)user.Status,
                user.UpdatedAt,
                user.LastLoginAt
            });
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        public async Task AddRoleAsync(string userId, RoleType role)
        {
            var connection = await _dbConnection.GetConnectionAsync();
            await AddRoleInternalAsync(userId, role, null);
        }

        private async Task AddRoleInternalAsync(string userId, RoleType role, IDbTransaction? transaction = null)
        {
            // Handle prefixed UserId
            string rawUserId = userId;
            if (UserId.TryParse<UserId>(userId, out var userIdObj))
            {
                rawUserId = userIdObj.Value.ToString();
            }
            
            const string sql = @"
                INSERT INTO user_roles (user_id, role, created_at)
                VALUES (@UserId::uuid, @Role, @CreatedAt)
                ON CONFLICT (user_id, role) DO NOTHING";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                UserId = rawUserId,
                Role = role.ToString(),
                CreatedAt = DateTime.UtcNow
            }, transaction);
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        public async Task RemoveRoleAsync(string userId, RoleType role)
        {
            // Handle prefixed UserId
            string rawUserId = userId;
            if (UserId.TryParse<UserId>(userId, out var userIdObj))
            {
                rawUserId = userIdObj.Value.ToString();
            }
            
            const string sql = @"
                DELETE FROM user_roles
                WHERE user_id = @UserId::uuid AND role = @Role";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                UserId = rawUserId,
                Role = role.ToString()
            });
        }

        /// <summary>
        /// Gets all roles for a user.
        /// </summary>
        public async Task<IEnumerable<RoleType>> GetRolesAsync(string userId)
        {
            try
            {
                // Handle prefixed UserId
                string rawUserId = userId;
                if (UserId.TryParse<UserId>(userId, out var userIdObj))
                {
                    rawUserId = userIdObj.Value.ToString();
                }
                else if (!Guid.TryParse(userId, out _))
                {
                    // If we can't parse it, return an empty list rather than throwing an exception
                    Console.WriteLine($"Warning: Invalid user ID format when fetching roles: {userId}");
                    return new List<RoleType>();
                }
                
                const string sql = @"
                    SELECT role
                    FROM user_roles
                    WHERE user_id = @UserId::uuid";

                var connection = await _dbConnection.GetConnectionAsync();
                var roleStrings = await connection.QueryAsync<string>(sql, new { UserId = rawUserId });
                
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRolesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task LoadRolesAsync(User user)
        {
            var roles = await GetRolesAsync(user.Id.Value.ToString());
            foreach (var role in roles)
            {
                user.AddRole(role);
            }
        }
    }
} 