using System.Data;
using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.TypeHandlers;

namespace LoyaltySystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection _dbConnection;
        
        static UserRepository()
        {
            TypeHandlerConfig.Initialize();
        }

        public UserRepository(IDatabaseConnection dbConnection) =>
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));

        private const string Users = "users";
        private const string UserRoles = "user_roles";
        private const string Customers = "customers";
        private const string EmailConfirmationTokens = "email_confirmation_tokens";
        private const string UserSelectFields = """
            u.id AS Id, 
            u.prefixed_id AS PrefixedId,
            u.first_name AS FirstName,
            u.last_name AS LastName,
            u.username AS Username, 
            u.email AS Email, 
            u.email_confirmed AS IsEmailConfirmed,
            u.password_hash AS PasswordHash, 
            COALESCE(CAST(u.status AS INT), 1) AS Status, 
            u.created_at AS CreatedAt, 
            u.updated_at AS UpdatedAt, 
            u.last_login_at AS LastLoginAt
        """;
        
        public async Task<User?> GetByIdAsync(UserId id)
        {
            ArgumentNullException.ThrowIfNull(id);
            
            const string sql = $"""
                SELECT 
                    {UserSelectFields}
                FROM 
                    {Users} u
                WHERE 
                    u.id = @Id::uuid
            """;

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id.Value });
            
            if (user is { })
                await LoadRolesAsync(user);
            
            return user;
        }
        
        public async Task<User?> GetByUsernameAsync(string username)
        {
            ArgumentNullException.ThrowIfNull(username);
            
            const string sql = $"""
                SELECT 
                    {UserSelectFields}
                FROM 
                    {Users} u
                WHERE 
                    u.username = @Username
            """;

            var connection = await _dbConnection.GetConnectionAsync();
            
            try
            {
                var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
                
                if (user is { })
                    await LoadRolesAsync(user);
                
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByUsernameAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            ArgumentNullException.ThrowIfNull(email);
            const string sql = $"""
                SELECT 
                    {UserSelectFields}
                FROM 
                    {Users} u
                WHERE 
                    u.email = @Email
            """;

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
            
            if (user is { })
                await LoadRolesAsync(user);
            
            return user;
        }
        public async Task<IEnumerable<User>> GetByRoleAsync(RoleType role, int skip, int take)
        {
            ArgumentNullException.ThrowIfNull(role);
            ArgumentNullException.ThrowIfNull(skip);
            ArgumentNullException.ThrowIfNull(take);
            
            const string sql = $"""
                SELECT 
                    {UserSelectFields}
                FROM 
                    {Users} u INNER JOIN
                    {UserRoles} ur ON u.id = ur.user_id
                WHERE 
                    ur.role = @Role
                ORDER BY 
                    u.created_at DESC
                LIMIT 
                    @Take 
                OFFSET 
                    @Skip
            """;

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

        public async Task<User?> GetByEmailConfirmationTokenAsync(string token)
        {
            ArgumentNullException.ThrowIfNull(token);
            
            const string sql = $"""
                SELECT 
                    {UserSelectFields}
                FROM 
                    {EmailConfirmationTokens} ect INNER JOIN 
                    {Users} u ON ect.user_id = u.id
                WHERE 
                    ect.token = @Token
            """;

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { token });
            
            if (user is { })
                await LoadRolesAsync(user);
            
            return user;
        }
        
        public async Task<User?> GetByCustomerIdAsync(CustomerId customerId)
        {
            ArgumentNullException.ThrowIfNull(customerId);
            
            const string sql = $"""
                SELECT 
                    {UserSelectFields},
                    c.id AS CustomerId
                FROM 
                    {Users} u INNER JOIN
                    {Customers} c ON u.id = c.user_id
                WHERE 
                    c.customer_id = @CustomerId::uuid
            """;

            var connection = await _dbConnection.GetConnectionAsync();
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { CustomerId = customerId.Value });
            
            if (user is { })
                await LoadRolesAsync(user);
            
            return user;
        }
        public async Task<IEnumerable<RoleType>> GetRolesAsync(UserId userId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            
            try
            {
                const string sql = $"""
                    SELECT role
                    FROM {UserRoles}
                    WHERE user_id = @UserId::uuid
                """;

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
        
        public async Task AddAsync(User user, IDbTransaction? transaction = null)
        {
            ArgumentNullException.ThrowIfNull(user);

            const string sql = $"""
                INSERT INTO {Users} 
                (
                    id, prefixed_id, first_name, last_name, username, email, password_hash, 
                    email_confirmed, status, created_at, updated_at, last_login_at
                ) 
                VALUES 
                (
                    @Id, @PrefixedId, @FirstName, @LastName, @Username, @Email, @PasswordHash, 
                    @IsEmailConfirmed, @Status, @CreatedAt, @UpdatedAt, @LastLoginAt
                )
            """;

            var connection = await _dbConnection.GetConnectionAsync();
            var parameters = new 
            {
                Id = user.Id.Value, 
                user.PrefixedId,
                user.FirstName,
                user.LastName,
                user.UserName,
                user.Email,
                user.PasswordHash,
                user.IsEmailConfirmed,
                Status = (short)user.Status, 
                user.CreatedAt,
                user.UpdatedAt,
                user.LastLoginAt
            };

            try
            {
                var userRoles = user.Roles.Select(r => r.Role).ToList();
                await connection.ExecuteAsync(sql, parameters, transaction: transaction);
                await AddRoleInternalAsync(user.Id, userRoles, transaction);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user or roles: {ex.Message}");
                Console.WriteLine($"User data: {System.Text.Json.JsonSerializer.Serialize(parameters)}");
                throw; 
            }
        }

        public async Task UpdatePasswordAsync(UserId userId, string newPasswordHash)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(newPasswordHash);
            
            const string sql = @"
                UPDATE users
                SET password_hash = @PasswordHash,
                    updated_at = @UpdatedAt
                WHERE id = @Id::uuid";

            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                Id = userId.Value,
                PasswordHash = newPasswordHash,
                UpdatedAt = DateTime.UtcNow
            });
        }
        
        public async Task AddRoleAsync(UserId userId, List<RoleType> roles)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(roles);
            
            await _dbConnection.GetConnectionAsync();
            await AddRoleInternalAsync(userId, roles, null);
        }
        private async Task AddRoleInternalAsync(UserId userId, List<RoleType> roles, IDbTransaction? transaction = null)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(roles);
            
            string sql = $"""
              INSERT INTO {UserRoles} (user_id, role, created_at)
              VALUES (@UserId, @Role, @CreatedAt)
              ON CONFLICT (user_id, role) DO NOTHING
            """;

            var now = DateTime.UtcNow;
            var parameters = roles
                .Select((role, _) => 
                    new
                    {
                        UserId = userId.Value,
                        Role = role.ToString(),
                        CreatedAt = now
                    }
                )
                .ToList();
            
            var connection = await _dbConnection.GetConnectionAsync();
            await connection.ExecuteAsync(sql, parameters, transaction);
        }
        
        public async Task UpdateAsync(User user, IDbTransaction? transaction = null)
        {
            ArgumentNullException.ThrowIfNull(user);
            
            const string sql = $@"
                UPDATE 
                    {Users}
                SET 
                    first_name = @FirstName,
                    last_name = @LastName,
                    username = @UserName,
                    email = @Email,
                    password_hash = @PasswordHash,
                    status = @Status,
                    updated_at = @UpdatedAt,
                    last_login_at = @LastLoginAt,
                    email_confirmed = @IsEmailConfirmed
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
                user.LastLoginAt,
                user.IsEmailConfirmed
            }, transaction);
        }
        public async Task UpdateLastLoginAsync(UserId userId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            
            const string sql = $@"
                UPDATE {Users}
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
        

        public async Task RemoveRoleAsync(UserId userId, List<RoleType> roles)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(roles);
            
            const string sql = $@"
                DELETE FROM {UserRoles}
                WHERE user_id = @UserId::uuid AND role = ANY(@Roles)";

            var connection = await _dbConnection.GetConnectionAsync();
            
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId.Value,
                Roles = roles.Select(r => r.ToString()).ToArray(),
            });
        }

        private async Task LoadRolesAsync(User userData)
        {
            ArgumentNullException.ThrowIfNull(userData);
            
            var roles = await GetRolesAsync(userData.Id);
            foreach (var role in roles)
            {
                userData.AddRole(role);
            }
        }
    }
} 