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
        public async Task<User?> GetByIdAsync(UserId id)
        {
            const string sql = @"
                SELECT 
                    u.id AS ""Id"", 
                    u.username AS ""Username"", 
                    u.email AS ""Email"", 
                    u.password_hash AS ""PasswordHash"", 
                    COALESCE(CAST(u.status AS INT), 1) AS ""Status"", 
                    u.created_at AS ""CreatedAt"", 
                    u.updated_at AS ""UpdatedAt"", 
                    u.last_login_at AS ""LastLoginAt""
                FROM users u
                WHERE u.id = @Id::uuid";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id.Value });
            
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
					u.first_name AS ""FirstName"",
					u.last_name AS ""LastName"",
                    u.username AS ""Username"", 
                    u.email AS ""Email"", 
                    u.password_hash AS ""PasswordHash"", 
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
                    u.id AS Id, 
                    u.prefixed_id as PrefixedId,
                    u.first_name AS FirstName,
                    u.last_name AS LastName,
                    u.username AS Username, 
                    u.email AS Email, 
                    u.password_hash AS PasswordHash, 
                    COALESCE(CAST(u.status AS INT), 1) AS Status, 
                    u.created_at AS CreatedAt, 
                    u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt
                FROM 
                    users u
                WHERE 
                    u.email = @Email";

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
                    u.id AS Id, 
                    u.username AS Username, 
                    u.email AS Email, 
                    u.password_hash AS PasswordHash, 
                    COALESCE(CAST(u.status AS INT), 1) AS Status, 
                    u.created_at AS CreatedAt, 
                    u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt
                FROM 
                    users       u INNER JOIN
                    user_roles ur ON u.id = ur.user_id
                WHERE 
                    ur.role = @Role
                ORDER BY 
                    u.created_at DESC
                LIMIT 
                    @Take 
                OFFSET 
                    @Skip";

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
                    u.id AS Id, 
                    u.username AS Username, 
                    u.email AS Email, 
                    u.password_hash AS PasswordHash, 
                    COALESCE(CAST(u.status AS INT), 1) AS Status, 
                    u.created_at AS CreatedAt, 
                    u.updated_at AS UpdatedAt, 
                    u.last_login_at AS LastLoginAt,
                    c.id AS CustomerId
                FROM 
                    users u INNER JOIN
                    customers c ON u.id = c.user_id
                WHERE 
                    c.customer_id = @CustomerId::uuid";

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { CustomerId = customerId.Value });
            
            if (user != null)
            {
                await LoadRolesAsync(user);
            }
            
            return user;
        }

        /// <summary>
        /// Adds a new user, potentially within a transaction.
        /// </summary>
        public async Task AddAsync(User user, IDbTransaction? transaction = null)
        {
            // Generate and assign the PrefixedId before inserting
            var userIdObj = new UserId(user.Id.Value); // Assuming base Entity<T> has Value property
            user.PrefixedId = userIdObj.ToString();

            const string sql = @"
                INSERT INTO users 
                (
                    id, prefixed_id, first_name, last_name, username, email, password_hash, 
                    status, created_at, updated_at, last_login_at
                ) 
                VALUES 
                (
                    @Id, @PrefixedId, @FirstName, @LastName, @Username, @Email, @PasswordHash, 
                    @Status, @CreatedAt, @UpdatedAt, @LastLoginAt
                )";

            var connection = await _dbConnection.GetConnectionAsync();
            
            // Prepare parameters, including the new PrefixedId
            var parameters = new 
            {
                Id = user.Id.Value, // Explicitly name the parameter 'Id'
                user.PrefixedId,
                user.FirstName,
                user.LastName,
                user.UserName,
                user.Email,
                user.PasswordHash,
                Status = (short)user.Status, // Cast enum to short
                user.CreatedAt,
                user.UpdatedAt,
                user.LastLoginAt
            };

            try
            {
                await connection.ExecuteAsync(sql, parameters, transaction: transaction);

                // Add roles if any are defined on the user object (within the same transaction)
                foreach (var userRole in user.Roles)
                {
                    await AddRoleInternalAsync(user.Id, userRole.Role, transaction);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error adding user or roles: {ex.Message}");
                Console.WriteLine($"User data: {System.Text.Json.JsonSerializer.Serialize(parameters)}");
                throw; // Re-throw
            }
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        public async Task UpdateAsync(User user, IDbTransaction? transaction = null)
        {
            const string sql = @"
                UPDATE 
                    users
                SET 
                    first_name = @FirstName,
                    last_name = @LastName,
                    username = @UserName,
                    email = @Email,
                    password_hash = @PasswordHash,
                    status = @Status,
                    updated_at = @UpdatedAt,
                    last_login_at = @LastLoginAt
                WHERE 
                    id = @Id::uuid";
            
            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                Id = user.Id.Value,
                user.FirstName,
                user.LastName,
                user.UserName,
                user.Email,
                user.PasswordHash,
                Status = (int)user.Status,
                user.UpdatedAt,
                user.LastLoginAt
            }, transaction);
        }

        /// <summary>
        /// Updates the last login timestamp for a user.
        /// </summary>
        public async Task UpdateLastLoginAsync(UserId userId)
        {
            const string sql = @"
                UPDATE users
                SET last_login_at = @LastLoginAt,
                    updated_at = @UpdatedAt
                WHERE id = @Id";
            
            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                Id = userId.Value,
                LastLoginAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
                VALUES (@UserId::uuid, @Role, @CreatedAt)
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
                WHERE user_id = @UserId::uuid AND role = @Role";

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
            try
            {
                const string sql = @"
                    SELECT role
                    FROM user_roles
                    WHERE user_id = @UserId::uuid";

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRolesAsync for UserId {userId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
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