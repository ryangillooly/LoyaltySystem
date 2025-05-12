using Dapper;
using LoyaltySystem.Application.DTOs.Auth.EmailVerification;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;

namespace LoyaltySystem.Infrastructure.Repositories;

public class EmailVerificationRepository : IEmailVerificationRepository 
{

    private readonly IDatabaseConnection _dbConnection;

    public EmailVerificationRepository(IDatabaseConnection connection) =>
        _dbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
    
    public async Task SaveAsync(EmailConfirmationToken token)
    {
        const string sql = @"
            INSERT INTO 
                email_confirmation_tokens 
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

    public async Task<EmailConfirmationToken?> GetByTokenAsync(string token)
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
                email_confirmation_tokens 
            WHERE 
                token = @token
        ";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        var dto = await dbConnection.QuerySingleOrDefaultAsync<EmailConfirmationTokenDto>(sql, new { token });

        return dto?.ToDomainModel();
    }

    public async Task UpdateAsync(EmailConfirmationToken token)
    {
        const string sql = @"
            UPDATE 
                email_confirmation_tokens
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
                email_confirmation_tokens
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
