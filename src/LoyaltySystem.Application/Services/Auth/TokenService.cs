using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.Interfaces.Auth;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Services.Auth;

public class TokenService : ITokenService
{
    private readonly ITokenRepository _repository;

    public TokenService(ITokenRepository repository) =>
        _repository = repository;

    public async Task<string> GenerateAndStoreTokenAsync(UserId userId, VerificationTokenType type, TimeSpan expiry)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(expiry);
        
        await _repository.InvalidateAllTokensAsync(userId, type);
        var token = SecurityUtils.GenerateSecureToken();
        
        await _repository.StoreTokenAsync(new VerificationToken
        {
            UserId = userId,
            Token = token,
            Type = type,
            ExpiresAt = DateTime.UtcNow.Add(expiry),
            IsValid = true
        });

        return token;
    }
    
    public async Task<OperationResult> IsTokenValidAsync(VerificationTokenType type, string token)
    {
        var record = await _repository.GetValidTokenAsync(type, token);
        if (record is not { IsValid: true } || DateTime.UtcNow >= record.ExpiresAt)
            return OperationResult.FailureResult("Invalid or expired verification token.");
        
        return OperationResult.SuccessResult();
    }

    public async Task MarkTokenAsUsedAsync(VerificationTokenType type, string token) =>
        await _repository.MarkTokenAsUsedAsync(type, token);

    public async Task InvalidateAllTokensAsync(UserId userId, VerificationTokenType type) =>
        await _repository.InvalidateAllTokensAsync(userId, type);
    
    public async Task InvalidateTokenAsync(VerificationTokenType type, string token) =>
        await _repository.InvalidateTokenAsync(type, token);

    public async Task<VerificationToken?> GetValidTokenAsync(VerificationTokenType type, string token) =>
        await _repository.GetValidTokenAsync(type, token);
}