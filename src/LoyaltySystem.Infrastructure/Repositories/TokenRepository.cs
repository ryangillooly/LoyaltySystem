using Dapper;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using System.Data;

namespace LoyaltySystem.Infrastructure.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly IDatabaseConnection _dbConnection;

    public TokenRepository(IDatabaseConnection connection) =>
        _dbConnection = connection ?? throw new ArgumentNullException(nameof(connection));

    public async Task StoreTokenAsync(VerificationToken token)
    {
        const string sql = @"
            INSERT INTO verification_tokens
                (id, user_id, token, token_type, is_valid, expires_at, created_at)
            VALUES
                (@Id, @UserId, @Token, @TokenType, @IsValid, @ExpiresAt, @CreatedAt)";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        await dbConnection.ExecuteAsync(sql, new
        {
            token.Id,
            UserId = token.UserId.Value,
            token.Token,
            TokenType = token.Type.ToString(),
            token.IsValid,
            token.ExpiresAt,
            token.CreatedAt
        });
    }
    
    public async Task<VerificationToken?> GetValidTokenAsync(VerificationTokenType type, string token)
    {
        const string sql = @"
            SELECT 
                id          as Id, 
                user_id     as UserId, 
                token       as Token, 
                token_type  as Type,
                expires_at  as ExpiresAt,
                is_valid    as IsValid,
                created_at  as CreatedAt
            FROM 
                verification_tokens
            WHERE 
                token_type = @TokenType AND
                token      = @Token     AND
                is_valid   = TRUE       AND
                expires_at > NOW()
            LIMIT 1";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        return await dbConnection.QuerySingleOrDefaultAsync<VerificationToken>(sql, new
        {
            TokenType = type.ToString(),
            Token = token
        });
    }
    
    public async Task InvalidateAllTokensAsync(UserId userId, VerificationTokenType type)
    {
        const string sql = @"
            UPDATE  
                verification_tokens
            SET 
                is_valid = FALSE
            WHERE 
              user_id    = @UserId AND
              token_type = @TokenType";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        await dbConnection.ExecuteAsync(sql, new
        {
            UserId = userId.Value,
            TokenType = type.ToString()
        });
    }

    public async Task InvalidateTokenAsync(VerificationTokenType type, string token)
    {
        const string sql = @"
            UPDATE  
                verification_tokens
            SET 
                is_valid = FALSE
            WHERE 
                token_type = @TokenType AND
                token      = @Token";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        await dbConnection.ExecuteAsync(sql, new
        {
            TokenType = type.ToString(),
            Token     = token 
        });
    }
    
    public async Task MarkTokenAsUsedAsync(VerificationTokenType type, string token)
    {
        const string sql = @"
            UPDATE 
                verification_tokens
            SET 
                is_valid = FALSE
            WHERE 
              token_type   = @TokenType
              AND token    = @Token
              AND is_valid = TRUE";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        await dbConnection.ExecuteAsync(sql, new
        {
            TokenType = type.ToString(),
            Token = token
        });
    }
}
