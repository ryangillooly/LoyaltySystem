using Dapper;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Entities;

namespace LoyaltySystem.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IDatabaseConnection _dbConnection;

    public PasswordResetTokenRepository(IDatabaseConnection connection) =>
        _dbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
    
    public async Task SaveAsync(PasswordResetToken token)
    {
        const string sql = @"
            INSERT INTO 
                password_reset_tokens 
                (id, user_id, token, expires_at, is_used, created_at)
            VALUES 
                (@id, @user_id, @token, @expires_at, @is_used, @created_at)
        ";

        var parameters = new
        {
            id = token.Id,
            user_id = token.UserId,
            token = token.Token,
            expires_at = token.ExpiresAt,
            is_used = token.IsUsed,
            created_at = token.CreatedAt
        };

        var dbConnection = await _dbConnection.GetConnectionAsync();
        await dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task<PasswordResetToken?> GetByUserIdAndTokenAsync(UserId userId, string token)
    {
        const string sql = @"
            SELECT 
                id as Id, 
                user_id as UserId, 
                token as Token, 
                expires_at as ExpiresAt, 
                is_used as IsUsed, 
                created_at as CreatedAt
            FROM 
                password_reset_tokens 
            WHERE 
                user_id = @userId AND
                token = @token
        ";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        var dto = await dbConnection.QuerySingleOrDefaultAsync<PasswordResetTokenDto>(sql, new { userId, token });

        return dto?.ToDomainModel();
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        const string sql = @"
            UPDATE 
                password_reset_tokens
            SET
                expires_at = @expires_at,
                is_used = @is_used,
                created_at = @created_at
            WHERE 
                id = @id AND 
                token = @token
        ";

        var parameters = new
        {
            id = token.Id,
            expires_at = token.ExpiresAt,
            is_used = token.IsUsed,
            created_at = token.CreatedAt,
            token = token.Token,
        };

        var dbConnection = await _dbConnection.GetConnectionAsync();
        await dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task InvalidateAllForUserAsync(Guid userId)
    {
        const string sql = @"
            UPDATE 
                password_reset_tokens
            SET 
                is_used = TRUE
            WHERE 
                user_id = @user_id AND 
                is_used = FALSE
        ";

        var parameters = new  { user_id = userId };

        var dbConnection = await _dbConnection.GetConnectionAsync();
        await dbConnection.ExecuteAsync(sql, parameters);
    }
}